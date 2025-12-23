# Invitation MFA Flow - Quick Reference

## What Changed Based on Your Feedback

### ✅ Your Requirement
> "The verification code should be used every time that the user enters. For example, if you enter first time and do a draft, next time that you enter you will need a verification code."

### 🎯 Implementation Approach

**Session-Based Verification** (not one-time verification)

#### How It Works:
1. **First Access**:
   - User clicks invitation link → Gets verification code via email
   - Enters code → Creates 2-hour session
   - Can work on form, save drafts during this session

2. **Return Visit (session active)**:
   - Session cookie still valid → Direct access to form
   - Session timer shown (e.g., "Session expires in 1h 23m")

3. **Return Visit (session expired)**:
   - Session expired → **NEW verification code required**
   - Email sent with fresh code
   - Enter code → New 2-hour session created

### Key Security Features

✅ **Multi-access protection**: Can't use invitation without email access  
✅ **Time-limited sessions**: 2-hour working window  
✅ **Code expiration**: 15-minute verification window  
✅ **Rate limiting**: Max 3 attempts per code  
✅ **Audit trail**: All verification attempts logged  
✅ **Session monitoring**: Auto-logout on expiration  

### User Experience Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Click Invitation Link → Token Validated                  │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Check Session Cookie                                     │
│    ├─ Valid Session? → Go to Form (Step 6)                 │
│    └─ No/Expired? → Continue to Step 3                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Request Verification Code → Email Sent                   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Enter 6-Digit Code (15-min to use, max 3 attempts)      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Code Validated → Session Created (2 hours)              │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Work on Registration Form                                │
│    - Fill out vendor details                                │
│    - Save drafts (session stays active)                     │
│    - Upload documents                                        │
│    - Session timer shows remaining time                     │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. Return Later?                                            │
│    ├─ Session Active → Continue working                     │
│    └─ Session Expired → Back to Step 2 (New code required) │
└─────────────────────────────────────────────────────────────┘
```

## Technical Architecture

### Data Storage (JSONB)
```json
{
  "mfa": {
    "currentCode": "123456",           // Current verification code
    "codeExpiresAt": "...",            // Code expires in 15 min
    "activeSession": {
      "sessionId": "sess_abc123",      // Unique session ID
      "sessionExpiresAt": "...",       // Session expires in 2 hours
      "createdAt": "..."               // When verified
    },
    "verificationHistory": [...]       // Audit trail
  }
}
```

### API Endpoints
- `POST /api/invitations/verify/request-code` - Send verification code
- `POST /api/invitations/verify/validate-code` - Verify code & create session
- `GET /api/invitations/session/validate` - Check if session is valid

### Configuration
```json
{
  "SessionDurationHours": 2,          // How long user can work
  "CodeExpirationMinutes": 15,        // How long to enter code
  "MaxVerificationAttempts": 3        // Attempts before lockout
}
```

## Security Benefits

1. **Prevents token sharing**: Can't share invitation link without email access
2. **Time-boxed access**: Must verify every 2 hours
3. **Audit trail**: Track all access attempts
4. **Rate limiting**: Prevents brute force attacks
5. **Session isolation**: Each verification creates unique session

## User Impact

### Positive
✅ Stronger security (email as second factor)  
✅ Fair UX (2-hour session = enough time to complete)  
✅ Clear feedback (session timer shows remaining time)  
✅ Recovery (can request new code if expired)  

### Considerations
⚠️ Requires email access for each session  
⚠️ 2-hour limit may interrupt long work sessions (configurable)  
⚠️ Email delays could frustrate users (add resend option)  

## Next Steps

1. **Review & Approve** this plan
2. **Start Phase 1**: Backend implementation (MFA service layer)
3. **Build APIs**: Request code, validate code, session management
4. **Frontend UI**: Verification code input, session timer
5. **Testing**: Security testing, UX testing
6. **Deploy**: Feature flag rollout (dev → prod)

## Quick Decision Points

| Question | Decision |
|----------|----------|
| Session duration? | **2 hours** (configurable per environment) |
| Code expiration? | **15 minutes** |
| Max attempts? | **3 attempts** before 1-hour lockout |
| Storage? | **JSONB Attributes** (per architecture standards) |
| Cookie type? | **HTTP-only, Secure, SameSite=Strict** |
| Resend cooldown? | **60 seconds** |

---

**Status**: ✅ Implementation Plan Ready  
**Branch**: `feature/invitation-mfa-flow`  
**Estimated Effort**: 4-5 days (with testing)  
**Risk Level**: Medium (requires careful testing)
