# Invitation Multi-Factor Authentication (MFA) Implementation Plan

**Feature Branch**: `feature/invitation-mfa-flow`  
**Created**: 2025-12-21  
**Objective**: Enhance invitation security by adding a second-factor verification code sent via email

---

## 1. Current Flow Analysis

### Existing Invitation Process
1. **Approver** creates invitation via API (`POST /api/invitations`)
2. System generates secure token (32-byte cryptographic random)
3. Email sent with invitation link containing token
4. **Vendor** clicks link (validates token + expiration)
5. **Vendor** fills registration form
6. **Vendor** submits registration (completes invitation)

### Current Security Measures
- Cryptographically secure random token (32 bytes)
- Token expiration (configurable, default 7 days)
- One-time use validation
- Status tracking (Pending → Accepted → Completed/Expired)

---

## 2. Proposed MFA Enhancement

### New Flow (Session-Based Verification)
1. **Approver** creates invitation (unchanged)
2. Email sent with invitation link (unchanged)
3. **Vendor** clicks link → Token validated
4. **NEW**: System generates 6-digit verification code
5. **NEW**: Verification code sent to vendor's email
6. **NEW**: Vendor enters verification code on portal
7. **NEW**: System validates code and creates **time-limited session** (e.g., 2 hours)
8. **Vendor** proceeds to registration form (with active session)
9. **Vendor** can save draft and continue working (within session)
10. **NEW**: If vendor leaves and returns later (session expired), **Steps 4-7 repeat** (new code required)
11. **Vendor** submits final registration (completes invitation)

### Security Improvements
- **Multi-access verification**: Code required on EVERY access attempt (even for draft edits)
- **Session-based security**: Time-limited access after verification (2 hours default)
- **Time-limited codes**: 15-minute expiration for verification code
- **Rate limiting**: Max 3 verification attempts per code request
- **Anti-automation**: Prevents bulk token validation attacks
- **Prevents token sharing**: Can't share invitation link without email access
- **Audit trail**: All verification attempts and sessions logged

---

## 3. Technical Design

### 3.1 Database Schema Changes

#### Option A: Add to VendorInvitation (SQL)
```csharp
// Add to VendorInvitation entity (VendorMdm.Shared/Models/SqlEntities.cs)
public class VendorInvitation
{
    // ... existing fields ...
    
    // MFA fields
    public string? VerificationCode { get; set; }          // 6-digit code
    public DateTime? VerificationCodeExpiresAt { get; set; } // 15-min window
    public int VerificationAttempts { get; set; } = 0;     // Track attempts
    public DateTime? VerificationCodeSentAt { get; set; }   // Last sent timestamp
    public DateTime? VerifiedAt { get; set; }               // Successful verification
}
```

#### Option B: Store in Attributes (JSONB) - Preferred
Following the Hybrid Relational-Document Model:
- **Reason**: MFA data is transient, context-specific, not needed for queries
- **Location**: `VendorInvitation.Attributes` JSONB column
- **Structure** (Session-Based Verification):
```json
{
  "mfa": {
    "currentCode": "123456",
    "codeExpiresAt": "2025-12-21T16:15:00Z",
    "codeAttempts": 2,
    "codeSentAt": "2025-12-21T16:00:00Z",
    "maxAttempts": 3,
    "activeSession": {
      "sessionId": "sess_abc123",
      "sessionExpiresAt": "2025-12-21T18:00:00Z",
      "createdAt": "2025-12-21T16:00:30Z",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0..."
    },
    "verificationHistory": [
      {
        "timestamp": "2025-12-21T16:00:30Z",
        "success": true,
        "sessionId": "sess_abc123",
        "ipAddress": "192.168.1.1"
      },
      {
        "timestamp": "2025-12-20T14:30:00Z",
        "success": true,
        "sessionId": "sess_xyz789",
        "ipAddress": "192.168.1.1"
      }
    ]
  }
}
```

**Session Management**:
- Each successful verification creates a new session
- Session is stored in both server (cache/memory) and client (HTTP-only cookie)
- Session expires after 2 hours (configurable)
- Expired/invalid session requires new verification code
- Session ID prevents replay attacks

### 3.2 API Endpoints

#### New Endpoints

**1. Request Verification Code**
```http
POST /api/invitations/verify/request-code
Content-Type: application/json

{
  "invitationToken": "abc123..."
}

Response 200:
{
  "codeSent": true,
  "expiresAt": "2025-12-21T16:15:00Z",
  "attemptsRemaining": 3,
  "email": "vendor@example.com" // masked: "ven***@example.com"
}

Response 429 (Too Many Requests):
{
  "error": "Maximum verification attempts reached",
  "canRetryAt": "2025-12-21T17:00:00Z"
}
```

**2. Verify Code**
```http
POST /api/invitations/verify/validate-code
Content-Type: application/json

{
  "invitationToken": "abc123...",
  "verificationCode": "123456"
}

Response 200:
{
  "verified": true,
  "invitation": {
    "vendorLegalName": "Acme Corp",
    "primaryContactEmail": "vendor@example.com",
    "expiresAt": "2025-12-28T12:00:00Z"
  }
}

Response 400:
{
  "verified": false,
  "error": "Invalid or expired verification code",
  "attemptsRemaining": 2
}
```

#### Modified Endpoint
**GET /api/invitations/details/{token}**
- Add flag: `requiresVerification: bool`
- Add field: `verificationStatus: string` (NotSent, Pending, Verified, Expired, MaxAttemptsReached)

### 3.3 Service Layer Changes

**File**: `backend/VendorMdm.Api/Services/InvitationService.cs`

**New Methods**:
```csharp
public interface IInvitationService
{
    // Existing methods...
    
    // New MFA methods
    Task<RequestVerificationCodeResponse> RequestVerificationCodeAsync(string token);
    Task<VerifyCodeResponse> VerifyCodeAsync(string token, string code);
    Task<bool> ValidateSessionAsync(string token, string sessionId);
    Task<SessionInfo> GetActiveSessionAsync(string token);
    Task InvalidateSessionAsync(string token, string sessionId);
}
```

**Implementation Details**:
1. **RequestVerificationCodeAsync**:
   - Validate invitation token exists and not expired
   - Check if max attempts exceeded (use rate limiting)
   - Generate 6-digit random code
   - Store in `Attributes.mfa.currentCode` JSONB
   - Set code expiration (15 minutes)
   - Send email with code
   - Log event to Cosmos DB
   - Publish event to Service Bus

2. **VerifyCodeAsync**:
   - Retrieve invitation by token
   - Extract MFA data from `Attributes.mfa`
   - Validate code matches and not expired
   - Increment code attempts counter
   - **If successful**:
     - Generate unique session ID
     - Create session object with 2-hour expiration
     - Store session in `Attributes.mfa.activeSession`
     - Add entry to `verificationHistory`
     - Return session ID to client (for cookie storage)
   - **If failed**:
     - Increment attempts
     - Check if max attempts reached (lockout)
   - Log verification attempt (success/failure)

3. **ValidateSessionAsync**:
   - Retrieve invitation by token
   - Extract active session from `Attributes.mfa.activeSession`
   - Validate session ID matches
   - Check session not expired
   - Return true/false

4. **GetActiveSessionAsync**:
   - Retrieve current active session info
   - Return session expiration time, session ID
   - Used by frontend to show session countdown

5. **InvalidateSessionAsync**:
   - Clear active session from JSONB
   - Used on logout or manual session termination

### 3.4 Email Templates

**New Template**: `invitation-verification-code.html`

**Location**: `backend/VendorMdm.Shared/Templates/`

**Content**:
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Verification Code - Vendor Onboarding</title>
</head>
<body>
    <h2>Verification Code</h2>
    <p>Hello,</p>
    <p>You've requested to access your vendor invitation. Please use the verification code below:</p>
    
    <div style="background: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;">
        <h1 style="color: #333; font-size: 36px; letter-spacing: 5px; margin: 0;">{{VerificationCode}}</h1>
    </div>
    
    <p><strong>This code will expire in 15 minutes.</strong></p>
    
    <p>If you didn't request this code, please ignore this email.</p>
    
    <hr>
    <p style="font-size: 12px; color: #666;">
        This is an automated message from {{CompanyName}} Vendor Management System.
    </p>
</body>
</html>
```

### 3.5 Frontend Changes

**New Component**: `frontend/src/components/VerificationCodeInput.tsx`

**Flow (Session-Based Access)**:
1. User lands on invitation page with token in URL
2. Frontend checks for existing session cookie
3. **If no valid session**:
   - Show verification code request screen
   - User clicks "Send Code" → API call to request code
   - Email received with 6-digit code
   - User enters code → API validates
   - **On success**: 
     - Session cookie stored (HTTP-only, secure)
     - Session countdown timer starts (2 hours)
     - Proceed to registration form
   - **On failure** (3 attempts): Show lockout message
4. **If valid session exists**:
   - Validate session with backend
   - If valid, proceed directly to form
   - If expired, return to step 3
5. User works on registration (can save drafts)
6. **Session monitoring**:
   - Show session expiration countdown
   - Warning at 10 minutes remaining
   - Auto-logout on session expiration
7. **On return visit** (later):
   - Check session cookie
   - If expired, require NEW verification code (back to step 3)

**UI Components**:
- **VerificationCodeInput**: 6-digit input with auto-focus
- **SessionTimer**: Countdown display (e.g., "Session expires in 1h 23m")
- **SessionWarning**: Alert when <10 minutes remaining
- **ResendCode**: Button with cooldown timer
- **AttemptsIndicator**: Shows remaining attempts (3, 2, 1)
- **LockoutMessage**: Shown after max attempts
- Error/success feedback with accessibility support

---

## 4. Configuration & Feature Flags

**File**: `backend/VendorMdm.Api/appsettings.json`

```json
{
  "Features": {
    "InvitationMfa": {
      "Enabled": true,                    // Master toggle
      "CodeLength": 6,                    // Digit count
      "CodeExpirationMinutes": 15,        // Code validity window
      "MaxVerificationAttempts": 3,       // Before lockout per code
      "ResendCooldownSeconds": 60,        // Prevent code spam
      "SessionDurationHours": 2,          // How long session stays active
      "SessionSlidingExpiration": false   // If true, session extends on activity
    }
  }
}
```

**Environment-specific**:
- **Development**: 
  - Disabled by default (for testing convenience)
  - Longer session duration (4 hours)
- **Production**: 
  - Enabled (security-first)
  - Standard session duration (2 hours)

**Session Storage**:
- **Server-side**: JSONB `Attributes.mfa.activeSession` (persistent across API instances)
- **Client-side**: HTTP-only secure cookie with session ID
- **Validation**: Every API request checks session validity before allowing access

---

## 5. Implementation Phases

### Phase 1: Backend Foundation (Day 1-2)
- [ ] Create EF migration for `Attributes` JSONB column (if not exists)
- [ ] Add MFA attribute models to `AttributeModels.cs`
- [ ] Implement `RequestVerificationCodeAsync` service method
- [ ] Implement `VerifyCodeAsync` service method
- [ ] Add verification code generation utility
- [ ] Create email template for verification code
- [ ] Add unit tests for MFA service methods

### Phase 2: API Endpoints (Day 2-3)
- [ ] Create `InvitationMfaController` or extend `InvitationController`
- [ ] Implement `POST /api/invitations/verify/request-code`
- [ ] Implement `POST /api/invitations/verify/validate-code`
- [ ] Update `GET /api/invitations/details/{token}` response
- [ ] Add API integration tests
- [ ] Add rate limiting middleware

### Phase 3: Frontend UI (Day 3-4)
- [ ] Create `VerificationCodeInput` component
- [ ] Create verification code page/modal
- [ ] Integrate with invitation flow
- [ ] Add countdown timer component
- [ ] Add error handling and user feedback
- [ ] Add accessibility (ARIA labels, keyboard navigation)

### Phase 4: Testing & Polish (Day 4-5)
- [ ] End-to-end testing (entire invitation + MFA flow)
- [ ] Security testing (brute force, timing attacks)
- [ ] Email delivery testing
- [ ] Performance testing (rate limiting, concurrent requests)
- [ ] Documentation updates
- [ ] User acceptance testing

### Phase 5: Deployment (Day 5)
- [ ] Deploy to Dev environment
- [ ] Smoke testing
- [ ] Deploy to Production (with feature flag off initially)
- [ ] Monitor logs and metrics
- [ ] Enable feature flag in Production
- [ ] Post-deployment validation

---

## 6. Security Considerations

### Code Generation
- Use cryptographically secure random number generator
- Avoid predictable patterns (e.g., sequential numbers)
- Consider using alphanumeric codes (harder to guess than digits only)

### Storage
- Store hashed verification codes (SHA256 + salt)
- Never log verification codes in plaintext
- Auto-expire codes after 15 minutes
- Clear codes after successful verification

### Rate Limiting
- Max 3 attempts per invitation token
- Lockout period: 1 hour after max attempts
- IP-based rate limiting (optional, for extra protection)
- Exponential backoff on resend requests

### Audit Trail
- Log all verification attempts (success/failure) to Cosmos DB
- Include IP address, timestamp, user agent
- Alert on suspicious patterns (multiple failures across invitations)

---

## 7. Monitoring & Metrics

### Key Metrics
- **MFA Enrollment Rate**: % of invitations using MFA
- **Verification Success Rate**: % successful verifications
- **Average Attempts**: Average tries before success
- **Code Expiration Rate**: % codes that expire unused
- **Lockout Rate**: % invitations reaching max attempts

### Alerts
- High verification failure rate (> 50%)
- Unusual spike in code requests (potential attack)
- High lockout rate (UX issue or attack)

### Logs
- All verification requests and results
- Failed attempts with details
- Code generation and expiration events

---

## 8. Testing Strategy

### Unit Tests
- [ ] Verification code generation (uniqueness, format)
- [ ] Code validation logic (expiration, attempts)
- [ ] Attribute helpers (JSON serialization/deserialization)

### Integration Tests
- [ ] Full invitation + MFA flow
- [ ] Rate limiting enforcement
- [ ] Email sending integration

### Security Tests
- [ ] Brute force protection
- [ ] Token replay attacks
- [ ] Timing attack resistance
- [ ] Code enumeration prevention

### User Acceptance Tests
- [ ] Happy path (successful verification)
- [ ] Code expiration scenario
- [ ] Max attempts lockout
- [ ] Resend code functionality

---

## 9. Rollback Plan

### If Issues Arise
1. **Quick Fix**: Disable feature flag (`Features:InvitationMfa:Enabled = false`)
2. **Database**: No schema changes needed (using JSONB)
3. **API Backward Compatibility**: Existing endpoints unchanged
4. **Frontend**: Graceful degradation (skip MFA if disabled)

### Success Criteria
- ✅ Zero critical bugs in production
- ✅ MFA completion rate > 95%
- ✅ Average verification time < 2 minutes
- ✅ No performance degradation
- ✅ Positive user feedback

---

## 10. Future Enhancements (Out of Scope)

- SMS-based verification (alternative to email)
- Biometric authentication (mobile app)
- TOTP/Authenticator app support
- Backup codes (for email delivery failures)
- Adaptive MFA (risk-based triggering)

---

## 11. Dependencies & Prerequisites

### Required
- ✅ Email service configured and functional
- ✅ Service Bus configured (for async notification)
- ✅ Cosmos DB configured (for audit trail)
- ✅ JSONB `Attributes` column on `VendorInvitation`

### Nice to Have
- Rate limiting middleware (can be added)
- Redis cache (for distributed rate limiting)
- Monitoring dashboard (Application Insights)

---

## 12. Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Email delivery failures | High | Implement retry logic, add SMS fallback |
| Code enumeration attacks | Medium | Rate limiting, lockout after 3 attempts |
| User confusion (UX) | Medium | Clear instructions, support contact |
| Performance overhead | Low | Async processing, caching |
| Feature flag misconfiguration | Low | Environment-specific defaults, tests |

---

## 13. Compliance & Documentation

### Documentation Updates
- [ ] API documentation (Swagger annotations)
- [ ] User guide (How to use MFA during onboarding)
- [ ] Admin guide (How to troubleshoot MFA issues)
- [ ] Architecture diagrams (updated flow)

### Compliance
- [ ] GDPR: Log retention policy for verification attempts
- [ ] Security audit: Pen testing for MFA implementation
- [ ] Accessibility: WCAG 2.1 AA compliance for UI

---

## 14. Success Metrics (3 Months Post-Launch)

- **Security**: Zero unauthorized invitation completions
- **Usability**: < 5% support requests related to MFA
- **Performance**: < 1 second verification response time
- **Adoption**: 100% of invitations using MFA (if mandatory)

---

## 15. Next Steps

1. ✅ Create feature branch: `feature/invitation-mfa-flow`
2. ⏳ Review and approve this implementation plan
3. ⏳ Begin Phase 1 development (Backend Foundation)
4. ⏳ Daily standups to track progress
5. ⏳ Demo to stakeholders after Phase 3

---

**Document Status**: Draft  
**Author**: Development Team  
**Last Updated**: 2025-12-21  
**Review Required**: Yes (Architecture Team, Security Team)
