# Chat Moderation APIs - Quick Reference

## Base URL
```
/api/chat/sessions
```

## Authentication
All endpoints require JWT Bearer token in Authorization header:
```
Authorization: Bearer <JWT_TOKEN>
```

---

## 1. Pause Chat Session

### Request
```http
POST /api/chat/sessions/{sessionId}/pause
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

### Parameters
| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| sessionId | GUID | URL Path | Yes | The chat session to pause |

### Response (200 OK)
```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "paused",
  "isPaused": true,
  "isActive": true,
  "timestamp": "2025-01-19T10:30:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Error Responses
| Status | Scenario |
|--------|----------|
| 400 | Chat already paused, or user lacks permission |
| 403 | User is not a participant in this session |
| 404 | Chat session not found |
| 401 | Missing or invalid JWT token |

### Permission Rules
- **OneToOne sessions**: Only experts can pause
- **Peer sessions**: Any participant can pause

---

## 2. Resume Chat Session

### Request
```http
POST /api/chat/sessions/{sessionId}/resume
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

### Parameters
| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| sessionId | GUID | URL Path | Yes | The chat session to resume |

### Response (200 OK)
```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "resumed",
  "isPaused": false,
  "isActive": true,
  "timestamp": "2025-01-19T10:35:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Error Responses
| Status | Scenario |
|--------|----------|
| 400 | Chat not paused, or user lacks permission |
| 403 | User is not a participant in this session |
| 404 | Chat session not found |
| 401 | Missing or invalid JWT token |

### Permission Rules
- **OneToOne sessions**: Only experts can resume
- **Peer sessions**: Any participant can resume

---

## 3. End Chat Session

### Request
```http
POST /api/chat/sessions/{sessionId}/end
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

### Parameters
| Parameter | Type | Location | Required | Description |
|-----------|------|----------|----------|-------------|
| sessionId | GUID | URL Path | Yes | The chat session to end |

### Response (200 OK)
```json
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "ended",
  "isPaused": false,
  "isActive": false,
  "timestamp": "2025-01-19T10:40:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Error Responses
| Status | Scenario |
|--------|----------|
| 400 | Chat already ended |
| 403 | User is not a participant in this session |
| 404 | Chat session not found |
| 401 | Missing or invalid JWT token |

### Permission Rules
- **OneToOne & Peer sessions**: Any participant can end

---

## 4. Get Message History

### Request
```http
GET /api/chat/sessions/{sessionId}/messages?page=1&pageSize=50
Authorization: Bearer <JWT_TOKEN>
```

### Parameters
| Parameter | Type | Location | Required | Default | Description |
|-----------|------|----------|----------|---------|-------------|
| sessionId | GUID | URL Path | Yes | - | The chat session ID |
| page | int | Query | No | 1 | Page number (1-based) |
| pageSize | int | Query | No | 50 | Messages per page (max 500) |

### Response (200 OK)
```json
[
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Hello, how are you?",
    "sentAt": "2025-01-19T10:00:00Z",
    "isAnonymous": false
  },
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Chat paused by expert",
    "sentAt": "2025-01-19T10:30:00Z",
    "isAnonymous": false
  },
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Chat resumed by expert",
    "sentAt": "2025-01-19T10:35:00Z",
    "isAnonymous": false
  }
]
```

### Error Responses
| Status | Scenario |
|--------|----------|
| 400 | Invalid pagination parameters |
| 403 | User is not a participant in this session |
| 404 | Chat session not found |
| 401 | Missing or invalid JWT token |

### Notes
- Messages are returned in **chronological order** (oldest first)
- **Paused/ended chats** can still be viewed
- **System messages** (pause, resume, end actions) are included in history
- **Deleted messages** are excluded
- **Anonymity** is respected - messages from users with IsIdentityRevealed=false show isAnonymous=true

---

## System Messages

When pause/resume/end actions occur, system messages are automatically created and broadcast via SignalR:

### Pause Message
```
"Chat paused by expert"  // for OneToOne
"Chat paused by user"    // for Peer
```

### Resume Message
```
"Chat resumed by expert"  // for OneToOne
"Chat resumed by user"    // for Peer
```

### End Message
```
"Chat ended by expert"  // for OneToOne
"Chat ended by user"    // for Peer
```

---

## SignalR Integration

All moderation actions are broadcast to connected clients via SignalR:

### Example Broadcast (via ChatHub)
When pause/resume/end occurs, all connected clients in the session receive:
```
Clients.Group(sessionId).SendAsync("ReceiveMessage", {
  "chatSessionId": "...",
  "senderId": "...",
  "content": "Chat paused by expert",
  "sentAt": "2025-01-19T...",
  "isAnonymous": false
})
```

### Message Sending Blocked
When trying to send a message to a paused or ended chat via SignalR:
```csharp
// Throws HubException with message:
"Chat is currently paused."
// or
"Chat has been ended."
```

---

## Example Usage Scenarios

### Scenario 1: Expert Pauses OneToOne Chat
```powershell
# 1. Expert pauses the chat
curl -X POST "https://api.example.com/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/pause" \
  -H "Authorization: Bearer <JWT_TOKEN>"

# Response: Chat paused, system message broadcasted via SignalR

# 2. User tries to send message (via SignalR)
# Result: HubException - "Chat is currently paused."

# 3. Expert resumes the chat
curl -X POST "https://api.example.com/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/resume" \
  -H "Authorization: Bearer <JWT_TOKEN>"

# Response: Chat resumed, user can now send messages again
```

### Scenario 2: Get Message History with Pagination
```powershell
# Get first 50 messages
curl "https://api.example.com/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?page=1&pageSize=50" \
  -H "Authorization: Bearer <JWT_TOKEN>"

# Get next 50 messages
curl "https://api.example.com/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?page=2&pageSize=50" \
  -H "Authorization: Bearer <JWT_TOKEN>"
```

### Scenario 3: User Ends Peer Chat
```powershell
# Either peer participant can end the session
curl -X POST "https://api.example.com/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/end" \
  -H "Authorization: Bearer <JWT_TOKEN>"

# Response: Chat ended, both users notified via SignalR
# Both users can no longer send messages
```

---

## Status Codes Reference

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (invalid/missing JWT) |
| 403 | Forbidden (not a participant or no permission) |
| 404 | Not Found (session doesn't exist) |

---

## Notes

- All timestamps are in **UTC** (ISO 8601 format)
- User IDs are **GUIDs**
- Only **participants** can call these endpoints
- **System messages** persist in the database and appear in message history
- **Anonymity** is controlled by `IsIdentityRevealed` on ChatParticipant
- **Message pagination** goes from newest (DESC) in DB but returns chronologically (ASC)
