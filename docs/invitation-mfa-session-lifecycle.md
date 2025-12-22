# Invitation MFA Flow - Session Lifecycle

## Scenario Examples

### 📋 Scenario 1: First-Time User (Complete in One Session)
**Timeline**: Day 1, 10:00 AM
1. ✉️ Receives invitation email
2. 🔗 Clicks link (10:00 AM)
3. 📧 Requests verification code → Email arrives
4. ✅ Enters code correctly → Session created (expires 12:00 PM)
5. 📝 Fills out form completely
6. 💾 Submits registration (10:45 AM)
7. ✅ **Success - No verification needed again**

**Result**: ✅ Smooth experience, single verification

---

### 📋 Scenario 2: User Saves Draft and Returns Within Session
**Timeline**: Day 1
1. 🔗 Clicks link (10:00 AM)
2. ✅ Verifies with code → Session created (expires 12:00 PM)
3. 📝 Fills out partial form
4. 💾 Saves as draft (10:30 AM)
5. 🚪 Closes browser
6. 🔙 Returns at 11:00 AM → **Session cookie still valid**
7. ✅ **Goes directly to form (no new code needed)**
8. 📝 Completes and submits

**Result**: ✅ No re-verification needed (session active)

---

### 📋 Scenario 3: User Returns After Session Expires ⚠️
**Timeline**: Day 1-2
1. 🔗 Clicks link (Day 1, 10:00 AM)
2. ✅ Verifies with code → Session created (expires 12:00 PM)
3. 📝 Fills out partial form
4. 💾 Saves as draft (10:30 AM)
5. 🚪 Closes browser
6. 🔙 Returns **Day 2, 9:00 AM** → Session expired (24 hours later)
7. 🚫 **Session validation fails**
8. 📧 **NEW verification code required**
9. ✅ Enters new code → New session created (expires 11:00 AM)
10. 📝 Continues working on draft
11. 💾 Submits registration

**Result**: ⚠️ Re-verification required (session expired)

---

### 📋 Scenario 4: Multiple Short Sessions (Power User)
**Timeline**: Day 1
1. **Session 1** (9:00 AM - 9:15 AM):
   - Verifies → Session created (expires 11:00 AM)
   - Starts form
   - Leaves to get documents
2. **Return** (10:30 AM):
   - Session still valid (expires 11:00 AM)
   - Continues working
   - Saves draft at 10:45 AM
3. **Return** (2:00 PM):
   - Session expired (11:00 AM expiration already passed)
   - **NEW code required**
   - Gets verified → New session (expires 4:00 PM)
   - Completes registration

**Result**: ⚠️ 2 verifications needed (spread across day)

---

### 📋 Scenario 5: User Exceeds Max Attempts (Lockout)
**Timeline**: Day 1, 10:00 AM
1. 🔗 Clicks link
2. 📧 Requests code → Email arrives with "934578"
3. ❌ Enters wrong code "123456" → **Attempt 1/3 failed**
4. ❌ Enters wrong code "654321" → **Attempt 2/3 failed**
5. ❌ Enters wrong code "999999" → **Attempt 3/3 failed**
6. 🔒 **LOCKED OUT for 1 hour**
7. ⏰ Waits until 11:00 AM
8. 📧 Requests NEW code → Email arrives
9. ✅ Enters correct code
10. ✅ Session created → Proceeds to form

**Result**: ⚠️ Temporary lockout (security protection)

---

### 📋 Scenario 6: Slow Email Delivery
**Timeline**: Day 1, 10:00 AM
1. 🔗 Clicks link
2. 📧 Requests code → Email delayed (server issues)
3. ⏰ Waits 2 minutes... no email
4. 🔄 Clicks "Resend Code" → Cooldown active (60 seconds)
5. ⏰ Waits 60 seconds
6. 🔄 Clicks "Resend Code" again → NEW code sent
7. ✉️ Email arrives (5 minutes total)
8. ✅ Enters code → Session created
9. 📝 Proceeds to form

**Result**: ⚠️ Slight delay but resend option helps

---

## Session State Management

### Server-Side (Database JSONB)
```json
{
  "mfa": {
    "currentCode": "934578",
    "codeExpiresAt": "2025-12-21T10:15:00Z",   // 15 min from request
    "codeAttempts": 0,
    "codeSentAt": "2025-12-21T10:00:00Z",
    "maxAttempts": 3,
    "activeSession": {
      "sessionId": "sess_abc123xyz",
      "sessionExpiresAt": "2025-12-21T12:00:00Z", // 2 hours from verification
      "createdAt": "2025-12-21T10:00:30Z",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0 ..."
    },
    "verificationHistory": [
      {
        "timestamp": "2025-12-21T10:00:30Z",
        "success": true,
        "sessionId": "sess_abc123xyz",
        "ipAddress": "192.168.1.100"
      }
    ]
  }
}
```

### Client-Side (Cookie)
```
Name: vendor_mfa_session
Value: sess_abc123xyz
Expires: 2025-12-21T12:00:00Z (2 hours)
HttpOnly: true
Secure: true (HTTPS only)
SameSite: Strict
Path: /vendor/invitation
```

---

## API Call Sequence

### First Access (No Session)
```http
1. GET /api/invitations/validate/{token}
   Response: { requiresMfa: true, hasActiveSession: false }

2. POST /api/invitations/verify/request-code
   Body: { invitationToken: "abc123..." }
   Response: { codeSent: true, expiresAt: "...", attemptsRemaining: 3 }

3. POST /api/invitations/verify/validate-code
   Body: { invitationToken: "abc123...", verificationCode: "934578" }
   Response: { 
     verified: true, 
     sessionId: "sess_abc123",
     sessionExpiresAt: "...",
     invitation: { vendorLegalName: "...", ... }
   }
   Set-Cookie: vendor_mfa_session=sess_abc123; HttpOnly; Secure

4. GET /api/invitations/details/{token}
   Cookie: vendor_mfa_session=sess_abc123
   Header: X-Session-Id: sess_abc123
   Response: { ... full invitation details ... }
```

### Return Visit (Active Session)
```http
1. GET /api/invitations/validate/{token}
   Cookie: vendor_mfa_session=sess_abc123
   Response: { requiresMfa: true, hasActiveSession: true }

2. POST /api/invitations/session/validate
   Body: { invitationToken: "abc123...", sessionId: "sess_abc123" }
   Response: { valid: true, expiresAt: "..." }

3. GET /api/invitations/details/{token}
   Cookie: vendor_mfa_session=sess_abc123
   Response: { ... proceed to form ... }
```

### Return Visit (Expired Session)
```http
1. GET /api/invitations/validate/{token}
   Cookie: vendor_mfa_session=sess_abc123 (expired)
   Response: { requiresMfa: true, hasActiveSession: false }

2. → Back to "First Access" flow (request new code)
```

---

## Frontend State Machine

```
┌──────────────────┐
│  INITIAL_LOAD    │
│  (Check cookie)  │
└────────┬─────────┘
         │
         ├─── Has valid cookie?
         │
    YES  │  NO
         │
         ↓
┌──────────────────┐      ┌──────────────────┐
│ VALIDATE_SESSION │      │ REQUEST_CODE     │
└────────┬─────────┘      └────────┬─────────┘
         │                         │
    Valid? Invalid?           Code sent?
         │                         │
    YES  │  NO                YES  │
         │                         ↓
         │                ┌──────────────────┐
         │                │  ENTER_CODE      │
         │                └────────┬─────────┘
         │                         │
         │                    Valid? Invalid?
         │                         │
         │                    YES  │  NO (attempts < 3)
         │                         │
         └─────────────────────────┤
                                   │
                                   ↓
                          ┌──────────────────┐
                          │  ACTIVE_SESSION  │
                          │  (Show form +    │
                          │   session timer) │
                          └────────┬─────────┘
                                   │
                            Save draft? Submit?
                                   │
                          ┌────────┴─────────┐
                          │                  │
                    DRAFT_SAVED         COMPLETED
                          │
                    (User can return
                     within session)
```

---

## Configuration Recommendations

### Development Environment
```json
{
  "Features": {
    "InvitationMfa": {
      "Enabled": false,                   // Disable for testing convenience
      "SessionDurationHours": 24,         // Long sessions for dev work
      "CodeExpirationMinutes": 60,        // Longer code validity
      "MaxVerificationAttempts": 10       // More lenient
    }
  }
}
```

### Staging Environment
```json
{
  "Features": {
    "InvitationMfa": {
      "Enabled": true,                    // Test full flow
      "SessionDurationHours": 4,          // Longer for testing
      "CodeExpirationMinutes": 30,        // Medium validity
      "MaxVerificationAttempts": 5        // Moderate
    }
  }
}
```

### Production Environment
```json
{
  "Features": {
    "InvitationMfa": {
      "Enabled": true,                    // REQUIRED
      "SessionDurationHours": 2,          // Security-first
      "CodeExpirationMinutes": 15,        // Standard security
      "MaxVerificationAttempts": 3,       // Strict limit
      "ResendCooldownSeconds": 60         // Prevent spam
    }
  }
}
```

---

## Monitoring Queries

### Check Active Sessions (SQL)
```sql
SELECT 
  id,
  vendor_legal_name,
  primary_contact_email,
  attributes->'mfa'->'activeSession'->>'sessionId' as session_id,
  attributes->'mfa'->'activeSession'->>'sessionExpiresAt' as expires_at,
  CASE 
    WHEN (attributes->'mfa'->'activeSession'->>'sessionExpiresAt')::timestamp > NOW()
    THEN 'ACTIVE'
    ELSE 'EXPIRED'
  END as session_status
FROM vendor_invitations
WHERE status = 'Pending'
  AND attributes->'mfa'->'activeSession' IS NOT NULL;
```

### Verification Attempts Metrics
```sql
SELECT 
  DATE(created_at) as date,
  COUNT(*) as total_invitations,
  AVG((attributes->'mfa'->>'codeAttempts')::int) as avg_attempts,
  COUNT(*) FILTER (
    WHERE (attributes->'mfa'->>'codeAttempts')::int >= 3
  ) as lockouts
FROM vendor_invitations
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY DATE(created_at)
ORDER BY date DESC;
```

---

**Document Status**: ✅ Ready for Implementation  
**Last Updated**: 2025-12-21  
**Version**: 1.0 (Session-Based MFA)
