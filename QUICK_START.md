# Quick Start Guide: Chat Moderation APIs

## 🚀 Getting Started

### Prerequisites
- JWT Bearer token from authentication endpoint
- Session ID from an existing chat session (OneToOne or Peer)
- User must be a participant in the session

---

## 📌 Common Use Cases

### Use Case 1: Expert Pauses a One-to-One Chat

```bash
# Step 1: Expert calls pause endpoint
curl -X POST "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/pause" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json"

# Response (200 OK):
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "paused",
  "isPaused": true,
  "isActive": true,
  "timestamp": "2026-01-19T10:30:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}

# Step 2: SignalR clients receive system message
# Event: ReceiveMessage
// {
//   "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
//   "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
//   "content": "Chat paused by expert",
//   "sentAt": "2026-01-19T10:30:00Z",
//   "isAnonymous": false
// }

# Step 3: User tries to send message (blocked)
connection.invoke("SendMessage", sessionId, "Hello")
  .catch(error => {
    console.error(error);  // HubException: "Chat is currently paused."
  });
```

### Use Case 2: Get Message History with Pagination

```bash
# Get first page (50 messages)
curl "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?page=1&pageSize=50" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."

# Response (200 OK):
[
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Hello there!",
    "sentAt": "2026-01-19T10:00:00Z",
    "isAnonymous": false
  },
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Hi, how can I help?",
    "sentAt": "2026-01-19T10:05:00Z",
    "isAnonymous": false
  },
  {
    "chatSessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "senderId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "content": "Chat paused by expert",
    "sentAt": "2026-01-19T10:30:00Z",
    "isAnonymous": false
  }
]

# Get next page
curl "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?page=2&pageSize=50" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### Use Case 3: Resume and Continue Chat

```bash
# Resume the chat
curl -X POST "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/resume" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json"

# Response (200 OK):
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "resumed",
  "isPaused": false,
  "isActive": true,
  "timestamp": "2026-01-19T10:35:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}

# Now user can send messages again
connection.invoke("SendMessage", sessionId, "Thanks for pausing, I needed a moment")
  .then(() => console.log("Message sent"))
  .catch(error => console.error(error));
```

### Use Case 4: End a Chat Session

```bash
# Either participant can end the session
curl -X POST "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/end" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json"

# Response (200 OK):
{
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "action": "ended",
  "isPaused": false,
  "isActive": false,
  "timestamp": "2026-01-19T10:40:00Z",
  "actionBy": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}

# Now neither user can send messages
connection.invoke("SendMessage", sessionId, "Goodbye")
  .catch(error => {
    console.error(error);  // HubException: "Chat has been ended."
  });

# But both can still view message history
curl "https://localhost:5001/api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
  
# Response: Complete message history including "Chat ended by user" system message
```

---

## 🔑 Key Points to Remember

### ✅ DO:
- Use valid JWT token from authentication
- Check user role before calling pause/resume on OneToOne chats
- Handle pagination (page ≥ 1, pageSize ≤ 500)
- Implement UI feedback when chat is paused/ended
- Implement retry logic for network failures
- Store JWT securely (HttpOnly cookies recommended)

### ❌ DON'T:
- Don't send messages when `IsPaused = true` (will get HubException)
- Don't send messages when `IsActive = false` (will get HubException)
- Don't try to pause a session you don't have permission for
- Don't hardcode session IDs (make them dynamic from chat list)
- Don't forget to handle 403 Forbidden responses
- Don't use expired JWT tokens

---

## 🔄 State Transitions

```
Active & Not Paused     Active & Paused          Not Active
┌──────────────────┐   ┌──────────────┐    ┌──────────────┐
│  Normal Chat     │   │ Paused Chat  │    │ Ended Chat   │
│                  │   │              │    │              │
│ Can send msgs    │   │ Cannot send  │    │ Cannot send  │
│ Can pause        │──→│ Can resume   │    │ Cannot resume│
│ Can end          │   │ Can end      │    │ Permanent    │
└──────────────────┘   └──────────────┘    └──────────────┘
       ↓                       ↓                  ↑
       └───────────────────────┴──────────────────┘
              Can end anytime
```

---

## 🛠️ Frontend Integration Examples

### React Example

```javascript
// API Service
const chatAPI = {
  pauseChat: async (sessionId, token) => {
    const response = await fetch(
      `https://localhost:5001/api/chat/sessions/${sessionId}/pause`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }
    return response.json();
  },

  resumeChat: async (sessionId, token) => {
    const response = await fetch(
      `https://localhost:5001/api/chat/sessions/${sessionId}/resume`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }
    return response.json();
  },

  endChat: async (sessionId, token) => {
    const response = await fetch(
      `https://localhost:5001/api/chat/sessions/${sessionId}/end`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }
    return response.json();
  },

  getMessages: async (sessionId, token, page = 1, pageSize = 50) => {
    const response = await fetch(
      `https://localhost:5001/api/chat/sessions/${sessionId}/messages?page=${page}&pageSize=${pageSize}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error);
    }
    return response.json();
  }
};

// React Component
function ChatPanel({ sessionId, token, userRole }) {
  const [isPaused, setIsPaused] = useState(false);
  const [isActive, setIsActive] = useState(true);
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handlePause = async () => {
    try {
      setLoading(true);
      const result = await chatAPI.pauseChat(sessionId, token);
      setIsPaused(result.isPaused);
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleResume = async () => {
    try {
      setLoading(true);
      const result = await chatAPI.resumeChat(sessionId, token);
      setIsPaused(result.isPaused);
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleEnd = async () => {
    try {
      setLoading(true);
      const result = await chatAPI.endChat(sessionId, token);
      setIsActive(result.isActive);
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleLoadMessages = async () => {
    try {
      setLoading(true);
      const msgs = await chatAPI.getMessages(sessionId, token);
      setMessages(msgs);
      setError(null);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="chat-panel">
      <div className="chat-header">
        <h2>Chat Session</h2>
        <div className="status">
          {!isActive && <span className="badge badge-danger">Ended</span>}
          {isPaused && isActive && <span className="badge badge-warning">Paused</span>}
          {isActive && !isPaused && <span className="badge badge-success">Active</span>}
        </div>
      </div>

      <div className="chat-controls">
        {userRole === 'Expert' && isActive && !isPaused && (
          <button onClick={handlePause} disabled={loading}>
            Pause Chat
          </button>
        )}
        {userRole === 'Expert' && isActive && isPaused && (
          <button onClick={handleResume} disabled={loading}>
            Resume Chat
          </button>
        )}
        {isActive && (
          <button onClick={handleEnd} disabled={loading}>
            End Chat
          </button>
        )}
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="chat-messages">
        {messages.map(msg => (
          <div key={msg.sentAt} className={`message ${msg.isSystemMessage ? 'system' : ''}`}>
            <span className="sender">
              {msg.isAnonymous ? 'Anonymous' : msg.senderId}
            </span>
            <span className="content">{msg.content}</span>
            <span className="time">{new Date(msg.sentAt).toLocaleTimeString()}</span>
          </div>
        ))}
      </div>

      {isPaused && (
        <div className="alert alert-info">
          This chat is currently paused. No new messages can be sent.
        </div>
      )}

      {!isActive && (
        <div className="alert alert-warning">
          This chat has ended. You can view history but cannot send messages.
        </div>
      )}

      <div className="chat-input">
        <input
          type="text"
          placeholder="Type a message..."
          disabled={isPaused || !isActive}
        />
        <button disabled={isPaused || !isActive}>Send</button>
      </div>
    </div>
  );
}

export default ChatPanel;
```

---

## 🧪 Testing with Postman

### Step 1: Get JWT Token
```
POST /api/auth/login
Body: { "email": "expert@example.com", "password": "..." }
Response: { "token": "eyJhbGciOiJIUzI1NiIs..." }
```

### Step 2: Create Test Session
```
POST /api/chat/sessions
Authorization: Bearer <TOKEN>
Body: { "expertId": "...", "isAnonymous": false }
Response: { "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

### Step 3: Test Pause
```
POST /api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/pause
Authorization: Bearer <TOKEN>
```

### Step 4: Test Resume
```
POST /api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/resume
Authorization: Bearer <TOKEN>
```

### Step 5: Get Message History
```
GET /api/chat/sessions/3fa85f64-5717-4562-b3fc-2c963f66afa6/messages?page=1&pageSize=50
Authorization: Bearer <TOKEN>
```

---

## 🐛 Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 403 Forbidden on pause | Not expert on OneToOne | Verify user role, use expert account |
| 404 Not Found | Session ID invalid | Double-check session ID, create new session |
| "Already paused" error | Chat is paused | Call resume first, then pause again |
| HubException on SendMessage | Chat paused or ended | Check session status first |
| 401 Unauthorized | Missing/expired JWT | Re-authenticate, get new token |
| Invalid pagination | page < 1 or pageSize > 500 | Use page ≥ 1 and pageSize ≤ 500 |

---

## 📚 Additional Resources

- **API Reference:** [API_REFERENCE.md](API_REFERENCE.md)
- **Architecture:** [ARCHITECTURE.md](ARCHITECTURE.md)
- **Implementation Summary:** [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)
- **Code Checklist:** [CODE_CHECKLIST.md](CODE_CHECKLIST.md)

---

## ✅ Validation Checklist

Before using in production:
- [ ] JWT token is valid and not expired
- [ ] User is a participant in the session
- [ ] For OneToOne pause/resume: user role is "Expert"
- [ ] For Peer operations: either user is participant
- [ ] Pagination parameters are valid (page ≥ 1, size ≤ 500)
- [ ] Error handling is implemented in frontend
- [ ] UI shows pause/end status clearly
- [ ] Users are informed when chat is paused
- [ ] Retry logic for network failures
- [ ] Loading states during API calls

---

**Last Updated:** 2026-01-19
**Status:** ✅ Production Ready
