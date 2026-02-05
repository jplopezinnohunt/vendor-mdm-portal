# Solution Spec: Flows

**Focus**: State Machines & Technical Flows
**Last Updated**: 2026-02-05 | **Flows**: 15

> **Business Processes**: See [processes/](../processes/) for role-based business flows.

---

## State Machine Flows

### 1. Vendor Invitation Flow

```
Approver creates invitation
        │
        ▼
   [PENDING] ──────────────┐
        │                  │
   Vendor clicks          Time exceeds
        │                  │
        ▼                  ▼
   [DRAFT] ◄──────────[EXPIRED]
        │                  │
   MFA Verify         Admin resends
        │                  │
        ▼                  │
   [MFA_VERIFIED]          │
        │                  │
   Initial Info            │
        │                  │
        ▼                  │
   [INITIAL_COMPLETED]     │
        │                  │
   Enrichment              │
        │                  │
        ▼                  │
   [ENRICHED]              │
        │                  │
   Submit                  │
        │                  │
        ▼                  │
   [PENDING_REVIEW]        │
        │                  │
   ┌────┴────┐             │
   ▼         ▼             │
[APPROVED] [REJECTED]      │
   │                       │
   ▼                       │
[COMPLETED] ◄──────────────┘
```

**States**: Pending → Draft → MfaVerified → InitialInfoCompleted → Enriched → PendingReview → Approved/Rejected → Completed | Expired | Cancelled

**CurrentStage Values**: InvitationSent, MfaVerified, InitialInfoCompleted, Enriched

---

### 2. Vendor Application Flow

```
   [PENDING]
       │
   Review starts
       ▼
   [UNDER_REVIEW]
       │
   Sanctions screening
       │
   ┌───┴───┐
   ▼       ▼
[APPROVED] [REJECTED]
   │
   ▼
[SAP_CREATED]
```

**Sanctions Status**: NotScreened → Screened → (Sanctioned if flagged)

---

### 3. Change Request Flow

```
   [DRAFT]
       │
   Submit
       ▼
   [SUBMITTED]
       │
   Review
       ▼
   ┌───┴───┐
   ▼       ▼
[APPROVED] [REJECTED]
       │
   SAP Sync
       ▼
   [INTEGRATED]
```

---

### 4. Event Participant Flow

```
   [PENDING]
       │
   Invitation sent
       ▼
   [INVITED]
       │
   Vendor confirms
       ▼
   [CONFIRMED]
       │
   SAP vendor created
       ▼
   [SAP_CREATED]
```

**Tier System**: Tier_1 (priority), Tier_2, Tier_3

---

### 5. User Invitation Flow

```
Admin invites user
       │
       ▼
   [PENDING] (InvitationToken set)
       │
   User clicks link
       ▼
   Token validated
       │
   User sets password
       │
   ┌────┴────┐
   ▼         ▼
[2FA Setup] [Skip 2FA]
   │         │
   ▼         │
Verify TOTP  │
   │         │
   └────┬────┘
        ▼
   [ACTIVE] (IsBlocked=false)
```

**Auth Methods**: MagicLink, AzureAd, LocalStrong

---

### 6. Document Lifecycle Flow

```
   [PENDING]
       │
   Verification
       ▼
   ┌───┴───┐
   ▼       ▼
[VERIFIED] [REJECTED]
       │
   Expiry check
       ▼
   [ARCHIVED]
```

**Security Levels**: 1=Public, 2=Internal, 3=Confidential, 4=PII

---

## Authentication Flows

### 7. Magic Link Authentication Flow

```
User enters email
       │
       ▼
POST /auth/magic-link
       │
       ▼
Email sent with token
       │
       ▼
User clicks link
       │
       ▼
POST /auth/verify-magic-link
       │
   ┌───┴───┐
   ▼       ▼
[Success] [Expired/Invalid]
   │
   ▼
JWT issued
```

---

### 8. Local Auth + 2FA Flow

```
User enters email/password
       │
       ▼
POST /auth/login-local
       │
   ┌───┴───────────────┐
   ▼                   ▼
[2FA Required]     [Login Success]
   │                   │
   ▼                   ▼
Enter TOTP code      JWT issued
   │
   ▼
POST /auth/login-2fa
   │
   ▼
JWT issued
```

---

### 9. MFA Verification Flow (Invitation)

```
Vendor accesses invitation
       │
       ▼
POST /invitation/trigger-mfa/{token}
       │
       ▼
Email sent with 6-digit code
       │
       ▼
POST /invitation/verify-mfa/{token}
       │
   ┌───┴───┐
   ▼       ▼
[Verified] [Invalid/Expired]
   │
   ▼
CurrentStage = MfaVerified
```

---

## Integration Flows

### 10. Sanctions Screening Flow

```
Entity submitted for screening
       │
       ▼
POST /sanctions/screen
       │
       ▼
Check against all lists
       │
   ┌───┴───────────┐
   ▼               ▼
[CLEAR]        [MATCH_FOUND]
   │               │
   ▼               ▼
SanctionsStatus  Manual review
= Screened       required
```

**Batch**: POST /sanctions/screen/batch for multiple entities

---

### 11. SAP Duplicate Detection Flow

```
Vendor name entered
       │
       ▼
POST /sap/vendor/search
       │
       ▼
Levenshtein fuzzy matching
       │
   ┌───┴───────────┐
   ▼               ▼
[No Match]     [Potential Duplicates]
   │               │
   ▼               ▼
Proceed        User reviews matches
               │
               ▼
           Force create or select existing
```

---

### 12. Bank Validation Flow

```
Bank details entered
       │
       ├── IBAN validation (ISO 13616 MOD-97)
       │
       ├── SWIFT/BIC validation (ISO 9362)
       │
       ├── Country-specific rules (SEPA/US/Others)
       │
       └── Duplicate check across vendors
       │
   ┌───┴───┐
   ▼       ▼
[Valid] [Invalid]
```

---

## Data Flows

### 13. Hybrid Data Flow (Every Write)

```
1. API receives request
       │
2. Input sanitization (IInputSanitizer)
       │
3. Validate & process in Domain
       │
4. SQL: Save metadata + Attributes JSON
       │
5. Cosmos: Save full artifact (immutable)
       │
6. Cosmos: Emit domain event
       │
7. Outbox: Add for guaranteed delivery
       │
8. Return response
```

---

### 14. Real-Time Update Flow

```
Domain Event created
       │
       ▼
EventDispatcher
       │
       ├──► SignalR Hub ──► Connected Clients
       │
       └──► OutboxEvent ──► Service Bus ──► Workers
```

**SignalR Events**:
- StatusChanged
- VendorCreated
- TaskAssigned
- Notification
- SapSyncResult

---

### 15. GDPR Rights Flow

```
User requests data action
       │
       ▼
   ┌───┴───────────────────────────────┐
   │                                   │
   ▼                                   │
[Access]  [Rectification]  [Erasure]   │
GET       PUT              DELETE      │
/gdpr/    /gdpr/           /gdpr/      │
data-     data-            data-       │
export    correction       deletion    │
   │           │              │        │
   ▼           ▼              ▼        │
Export    Update data    Anonymize     │
as JSON                  (soft)        │
                                       │
[Portability] [Restriction] [Object]   │
GET           POST          POST       │
/gdpr/        /gdpr/        /gdpr/     │
data-         restrict-     object-    │
portability   processing    processing │
```

---

## Flow Summary

| # | Flow | Type | Endpoints |
|---|------|------|-----------|
| 1 | Vendor Invitation | State Machine | InvitationController |
| 2 | Vendor Application | State Machine | ReviewController |
| 3 | Change Request | State Machine | ChangeRequestController |
| 4 | Event Participant | State Machine | EventController |
| 5 | User Invitation | State Machine | AuthController |
| 6 | Document Lifecycle | State Machine | FilesController |
| 7 | Magic Link Auth | Authentication | AuthController |
| 8 | Local Auth + 2FA | Authentication | AuthController |
| 9 | MFA Verification | Authentication | InvitationController |
| 10 | Sanctions Screening | Integration | SanctionsController |
| 11 | SAP Duplicate Detection | Integration | SapController |
| 12 | Bank Validation | Integration | BankController |
| 13 | Hybrid Data | Technical | All Controllers |
| 14 | Real-Time Update | Technical | SignalR Hub |
| 15 | GDPR Rights | Compliance | GdprController |
