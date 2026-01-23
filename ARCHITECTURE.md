# Chat Moderation Architecture & Flow

## Component Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                       REST API Layer                             │
├─────────────────────────────────────────────────────────────────┤
│  ChatSessionsController                                           │
│  ├── POST   /{sessionId}/pause     → PauseSession()             │
│  ├── POST   /{sessionId}/resume    → ResumeSession()            │
│  ├── POST   /{sessionId}/end       → EndSession()               │
│  └── GET    /{sessionId}/messages  → GetMessages()              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                     Service Layer (Business Logic)               │
├─────────────────────────────────────────────────────────────────┤
│  IChatModerationService                                          │
│  ├── PauseSessionAsync()                                        │
│  ├── ResumeSessionAsync()                                       │
│  ├── EndSessionAsync()                                          │
│  └── GetSessionMessagesAsync()                                  │
│                                                                   │
│  ChatModerationService (Implementation)                          │
│  ├── Validates participant membership                            │
│  ├── Enforces role-based permissions                            │
│  ├── Creates system messages                                     │
│  ├── Updates session state                                       │
│  └── Handles state validation (no double-pause, etc.)           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Repository Layer (Data Access)                │
├─────────────────────────────────────────────────────────────────┤
│  IChatSessionRepository                                          │
│  ├── GetByIdWithParticipantsAsync()                             │
│  └── SaveChangesAsync()                                          │
│                                                                   │
│  IMessageRepository                                              │
│  ├── AddAsync()                                                  │
│  ├── GetBySessionIdAsync() [with pagination]                    │
│  └── SaveChangesAsync()                                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                   EF Core / SQL Server                            │
├─────────────────────────────────────────────────────────────────┤
│  ChatSessions Table                                              │
│  ├── Id (GUID)                                                   │
│  ├── SessionType (string: "OneToOne", "Peer", etc.)             │
│  ├── IsPaused (bool)         ← Updated by pause/resume          │
│  ├── IsActive (bool)         ← Updated by end                   │
│  ├── EndedAt (DateTime?)     ← Set when chat ends               │
│  └── Participants (ICollection)                                  │
│                                                                   │
│  Messages Table                                                  │
│  ├── Id (GUID)                                                   │
│  ├── ChatSessionId (GUID)                                        │
│  ├── SenderId (GUID)                                             │
│  ├── Content (string)                                            │
│  ├── IsSystemMessage (bool)  ← true for pause/resume/end        │
│  ├── IsDeleted (bool)                                            │
│  ├── CreatedAt (DateTime)                                        │
│  └── SentAt (DateTime)                                           │
│                                                                   │
│  ChatParticipants Table                                          │
│  ├── Id (GUID)                                                   │
│  ├── ChatSessionId (GUID)                                        │
│  ├── UserId (GUID)                                               │
│  ├── Role (string: "User", "Expert")                             │
│  └── IsIdentityRevealed (bool) ← Controls anonymity              │
└─────────────────────────────────────────────────────────────────┘
```

---

## Pause Session Flow

```
Client (REST)
    │
    ├── POST /api/chat/sessions/{sessionId}/pause
    │
    ↓
ChatSessionsController.PauseSession()
    │
    ├── Extract userId from JWT claims
    ├── Call _moderationService.PauseSessionAsync(sessionId, userId)
    │
    ↓
ChatModerationService.PauseSessionAsync()
    │
    ├── Load session with participants
    │   └── Call _sessionRepo.GetByIdWithParticipantsAsync()
    │
    ├── ✓ Validate session exists
    │
    ├── ✓ Validate user is participant
    │
    ├── ✓ Validate permissions:
    │   ├── If OneToOne → user.Role must be "Expert"
    │   └── If Peer    → any participant allowed
    │
    ├── ✓ Validate state: session.IsPaused == false
    │
    ├── Update session
    │   └── session.IsPaused = true
    │
    ├── Create system message
    │   ├── content = "Chat paused by " + participant.Role
    │   ├── isSystemMessage = true
    │   ├── senderId = userId
    │   └── Call _messageRepo.AddAsync()
    │
    ├── Persist changes
    │   └── Call _sessionRepo.SaveChangesAsync()
    │
    └── Return ChatControlResponseDto
        │
        ↓ (via SignalR broadcast)
    Chat Hub
        │
        ├── Clients.Group(sessionId).SendAsync("ReceiveMessage", 
        │       {system message payload})
        │
        └── All connected clients in session receive update
```

---

## SendMessage (with Pause/End Validation) Flow

```
Client (SignalR)
    │
    ├── Hub.SendMessage(chatSessionId, messageText)
    │
    ↓
ChatHub.SendMessage()
    │
    ├── Extract userId from context
    ├── Load session with participants
    │   └── Call _sessionRepo.GetByIdAsync()
    │
    ├── ✓ Validate session exists
    │
    ├── ✓ Validate user is participant
    │
    ├── 🚫 CHECK: IsPaused
    │   ├── If session.IsPaused == true
    │   └── Throw HubException("Chat is currently paused.")
    │
    ├── 🚫 CHECK: IsActive
    │   ├── If session.IsActive == false
    │   └── Throw HubException("Chat has been ended.")
    │
    ├── ✓ All validations pass
    │
    ├── Create Message entity
    │   ├── content = messageText
    │   ├── isSystemMessage = false
    │   ├── senderId = userId
    │   ├── sentAt = DateTime.UtcNow
    │   └── createdAt = DateTime.UtcNow
    │
    ├── Persist
    │   └── Call _messageRepo.SaveChangesAsync()
    │
    └── Broadcast to session group
        │
        ├── Clients.Group(sessionId).SendAsync("ReceiveMessage", 
        │       {message payload})
        │
        └── All participants receive message in real-time
```

---

## Get Message History Flow

```
Client (REST)
    │
    ├── GET /api/chat/sessions/{sessionId}/messages?page=1&pageSize=50
    │
    ↓
ChatSessionsController.GetMessages()
    │
    ├── Validate pagination: 1 ≤ pageSize ≤ 500
    │
    ├── Extract userId from JWT claims
    │
    ├── Call _moderationService.GetSessionMessagesAsync(
    │       sessionId, userId, page, pageSize)
    │
    ↓
ChatModerationService.GetSessionMessagesAsync()
    │
    ├── Load session with participants
    │   └── Call _sessionRepo.GetByIdWithParticipantsAsync()
    │
    ├── ✓ Validate session exists
    │
    ├── ✓ Validate user is participant
    │
    ├── Fetch paginated messages (DESC order in DB)
    │   └── Call _messageRepo.GetBySessionIdAsync(sessionId, page, pageSize)
    │       ├── Skip: (page - 1) * pageSize
    │       ├── Take: pageSize
    │       └── Filter: IsDeleted == false
    │
    ├── Map to DTOs with anonymity masking
    │   ├── For each message:
    │   │   ├── Find sender participant
    │   │   ├── isAnonymous = !sender.IsIdentityRevealed
    │   │   └── Create ChatMessageDto
    │   │
    │   └── Sort results by SentAt (ASC) for chronological display
    │
    └── Return List<ChatMessageDto>
        │
        └── Client receives messages in chronological order
            (oldest first, newest last)
```

---

## State Machine: Chat Session Lifecycle

```
┌─────────────┐
│   CREATED   │  (IsActive=true, IsPaused=false)
└─────────────┘
       │
       ├─── pause()
       │      └──→ ┌─────────────┐
       │           │   PAUSED    │  (IsActive=true, IsPaused=true)
       │           └─────────────┘
       │                  │
       │                  ├─── resume()
       │                  │      └──→ (back to CREATED state)
       │                  │
       │                  └─── end()
       │                         └──→ (to ENDED state)
       │
       └─── end()
              └──→ ┌─────────────┐
                   │   ENDED     │  (IsActive=false, IsPaused=?)
                   └─────────────┘
                          │
                          └─ No messages sendable
                          └─ Message history still viewable
```

---

## Permission Matrix

| Action | OneToOne | Peer | Any Non-Participant |
|--------|----------|------|---------------------|
| Pause  | Expert only | Any participant | Forbidden (403) |
| Resume | Expert only | Any participant | Forbidden (403) |
| End    | Any participant | Any participant | Forbidden (403) |
| View Messages | Any participant | Any participant | Forbidden (403) |

---

## Validation Rules

### Pause Session
- ✓ Session must exist (404 if not)
- ✓ User must be participant (403 if not)
- ✓ OneToOne: User role must be "Expert" (400 if not)
- ✓ Session must not already be paused (400 if paused)
- ✓ Session must be active (400 if ended)

### Resume Session
- ✓ Session must exist (404 if not)
- ✓ User must be participant (403 if not)
- ✓ OneToOne: User role must be "Expert" (400 if not)
- ✓ Session must be paused (400 if not paused)
- ✓ Session must be active (400 if ended)

### End Session
- ✓ Session must exist (404 if not)
- ✓ User must be participant (403 if not)
- ✓ Session must be active (400 if already ended)

### Get Message History
- ✓ Session must exist (404 if not)
- ✓ User must be participant (403 if not)
- ✓ 1 ≤ page (400 if invalid)
- ✓ 1 ≤ pageSize ≤ 500 (400 if invalid)

---

## System Message Generation

Whenever a moderation action occurs, a system message is created and persisted:

### Pause
```csharp
new Message {
    Id = Guid.NewGuid(),
    ChatSessionId = sessionId,
    SenderId = userId,
    Content = "Chat paused by expert",  // or "Chat paused by user" for Peer
    IsSystemMessage = true,
    SentAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    IsDeleted = false
}
```

### Resume
```csharp
new Message {
    Content = "Chat resumed by expert",  // or "Chat resumed by user"
    IsSystemMessage = true,
    // ... other fields
}
```

### End
```csharp
new Message {
    Content = "Chat ended by expert",  // or "Chat ended by user"
    IsSystemMessage = true,
    // ... other fields
}
```

---

## Anonymity Masking

```
ChatParticipant:
├── IsIdentityRevealed = true   → isAnonymous = false (in DTO)
└── IsIdentityRevealed = false  → isAnonymous = true  (in DTO)

Applied in GetSessionMessagesAsync():
├── Find message sender in session participants
└── If sender.IsIdentityRevealed == false
    └── Mark message as isAnonymous = true
```

---

## Real-Time Broadcast (SignalR)

When pause/resume/end occurs:

1. **Create system message** in DB
2. **Broadcast to session group**:
   ```csharp
   await Clients.Group(sessionId.ToString())
       .SendAsync("ReceiveMessage", 
       {
           ChatSessionId = sessionId,
           SenderId = userId,
           Content = "Chat paused by expert",
           SentAt = DateTime.UtcNow,
           IsAnonymous = false  // System messages are always "public"
       });
   ```
3. **All connected clients** in the session group receive the update
4. **Clients refresh UI** to show pause/resume/end status

---

## Error Handling Strategy

```
ChatModerationService
├── Session not found
│   └── throw ArgumentException("Chat session not found.")
│       └── Controller: catch + return 404
│
├── User not participant
│   └── throw ArgumentException("User is not a participant in this session.")
│       └── Controller: catch + return 403
│
├── Permission denied (expert-only on OneToOne)
│   └── throw ArgumentException("Only experts can pause one-to-one chats.")
│       └── Controller: catch + return 400
│
└── Invalid state (already paused, etc.)
    └── throw ArgumentException("Chat session is already paused.")
        └── Controller: catch + return 400
```

---

## Performance Considerations

### Database Queries
- **GetByIdWithParticipantsAsync()**: Single query with Include(Participants)
- **GetBySessionIdAsync()**: Paginated query (skip + take)
- **SaveChangesAsync()**: Batches all changes in transaction

### Pagination
- Queries newest first (DESC), returns oldest first (ASC)
- Max 500 messages per page (prevents OOM)
- Efficient OFFSET/FETCH pattern

### SignalR Broadcasting
- Uses group-based broadcasting (efficient for large user bases)
- Messages only sent to connected clients in session group
- No unnecessary database calls per broadcast

---

## Sequence Diagram: Pause & Resume

```
User         REST API         Service         Repository      Database
 │              │                │                │               │
 │──Pause────→   │                │                │               │
 │              │─GetByIdWith───→  │                │               │
 │              │                  │─Query────────→ │──SELECT──────→ │
 │              │                  │←──session──────←──loaded────── │
 │              │─Validate────────→ │ (checks role,  │               │
 │              │                  │  state, etc.)  │               │
 │              │─CreateMessage─→  │                │               │
 │              │                  │─AddAsync──────→ │               │
 │              │                  │                │               │
 │              │─UpdateState──→   │                │               │
 │              │ (IsPaused=true)  │                │               │
 │              │                  │                │               │
 │              │─SaveChanges─────→ │                │               │
 │              │                  │─SaveAsync────→ │──UPDATE──────→ │
 │              │                  │                │──INSERT──────→ │
 │              │                  │                │←──OK──────────← │
 │              │←─Response────────←─Response───────←─             │
 │←─200 OK──────← (ChatControlResponse)                             │
 │  (paused)     │                │                │               │
 │              │ (SignalR Broadcast)              │               │
 │              │──────Clients.Group(sessionId)──────────────────→ │
 │              │    .SendAsync("ReceiveMessage", systemMsg)        │
 │
 │  [time passes]
 │
 │──Resume─────→ │                │                │               │
 │              │─GetByIdWith───→  │                │               │
 │              │                  │─Query────────→ │──SELECT──────→ │
 │              │                  │←──session──────←──loaded────── │
 │              │─Validate────────→ │ (checks state)  │               │
 │              │─CreateMessage─→  │                │               │
 │              │─UpdateState──→   │                │               │
 │              │ (IsPaused=false)  │                │               │
 │              │-SaveChanges──────→ │                │               │
 │              │                  │─SaveAsync────→ │──UPDATE──────→ │
 │              │                  │                │──INSERT──────→ │
 │              │                  │                │←──OK──────────← │
 │              │←─Response────────←─Response───────←─             │
 │←─200 OK──────← (ChatControlResponse)                             │
 │  (resumed)    │                │                │               │
 │              │ (SignalR Broadcast)              │               │
 │              │──────Clients.Group(sessionId)──────────────────→ │
 │              │    .SendAsync("ReceiveMessage", systemMsg)        │
```

---

## Testing Recommendations

### Unit Tests
- Mock `IChatSessionRepository` and `IMessageRepository`
- Test all validation rules
- Test permission enforcement
- Test system message creation

### Integration Tests
- Use in-memory SQLite database
- Test full pause/resume/end workflows
- Verify message persistence
- Verify state transitions

### End-to-End Tests
- Test REST endpoints with JWT authentication
- Test SignalR message broadcasting
- Test concurrent operations (race conditions)
- Test pagination edge cases

---

## Scalability Notes

✅ **Stateless services** - No session affinity required
✅ **Efficient queries** - Include patterns, pagination
✅ **SignalR groups** - Scales well for large user bases
✅ **Database transactions** - EF Core handles atomicity
✅ **Async/await** - Non-blocking I/O throughout

Future optimizations:
- Add query result caching for frequently accessed sessions
- Use SignalR backplane for distributed deployments
- Implement message archival for old sessions
