# Solution Spec: Integrations

**Focus**: External Systems, APIs, Third-Party Services
**Last Updated**: 2026-02-05 | **Integrations**: 10

---

## Integration Map

```
                         ┌─────────────────┐
                         │  Vendor Portal  │
                         └────────┬────────┘
                                  │
    ┌─────────────┬───────────────┼───────────────┬─────────────┐
    │             │               │               │             │
    ▼             ▼               ▼               ▼             ▼
┌───────┐   ┌─────────┐   ┌───────────┐   ┌──────────┐   ┌──────────┐
│  SAP  │   │  Email  │   │ Sanctions │   │   Bank   │   │   MFA    │
│ (BAPI)│   │ Service │   │ Screening │   │Validation│   │ (TOTP)   │
└───────┘   └─────────┘   └───────────┘   └──────────┘   └──────────┘
 ⏸️ Mock     ✅ Active     ✅ Active       ✅ Active      ✅ Active
```

---

## 1. SAP Integration

| Attribute | Value |
|-----------|-------|
| **Status** | ⏸️ Mock Available |
| **Protocol** | BAPI (RFC) |
| **Environment** | D01 (pending access) |
| **Pattern** | Mock/Real swap via config |
| **Controller** | SapController |

**Operations**:
- `POST /sap/vendor/search` - Duplicate detection (Levenshtein fuzzy matching)
- `GET /sap/vendor/{vendorNumber}` - Get vendor master data
- `POST /sap/vendor` - Create new vendor
- `PUT /sap/vendor/{vendorNumber}` - Update vendor
- `POST /sap/validate/name` - Name validation (35 char max)
- `POST /sap/validate/bank` - Bank validation (country-specific)
- `POST /sap/bank/check-duplicate` - Duplicate IBAN check

---

## 2. Email Service

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Provider** | Azure Communication Services |
| **Pattern** | Queue → Function → Send |
| **Controller** | HealthController (status) |

**Triggers**:
- Invitation created
- MFA code sent
- Magic link sent
- Application status change
- User invitation
- Document request

---

## 3. Sanctions Screening

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ **Active** (Mock available) |
| **Provider** | Internal + OFAC lists |
| **Pattern** | Sync screening with batch support |
| **Controller** | SanctionsController |

**Operations**:
- `POST /sanctions/screen` - Screen single entity
- `POST /sanctions/screen/batch` - Batch screening
- `GET /sanctions/{screeningId}` - Get screening result
- `GET /sanctions/lists/info` - List update status

**Screening Status Values**: NotScreened, Screened, Sanctioned

---

## 4. Bank Validation Service

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Standards** | ISO 13616 (IBAN), ISO 9362 (SWIFT) |
| **Controller** | BankController |

**Operations**:
- `POST /bank/configuration` - Get country-specific field rules
- `POST /bank/validate-iban` - IBAN validation (MOD-97)
- `POST /bank/validate-swift` - SWIFT/BIC validation

**Country Rules**:
- SEPA countries: IBAN required
- US: ABA routing + account number
- Others: SWIFT + account number

---

## 5. MFA / 2FA Service

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Type** | TOTP (Time-based One-Time Password) |
| **Pattern** | Email code for invitations, TOTP app for users |

**Invitation MFA**:
- `POST /invitation/trigger-mfa/{token}` - Send 6-digit code via email
- `POST /invitation/verify-mfa/{token}` - Verify code

**User 2FA**:
- `POST /auth/verify-2fa-setup` - Initial 2FA setup
- `POST /auth/login-2fa` - Login with 2FA code
- Recovery codes stored in User.RecoveryCodes

---

## 6. Magic Link Authentication

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Pattern** | Passwordless email authentication |
| **Expiry** | Configurable (default: 15 minutes) |

**Operations**:
- `POST /auth/magic-link` - Send magic link email
- `POST /auth/verify-magic-link` - Verify and issue JWT

---

## 7. Azure AD Integration

| Attribute | Value |
|-----------|-------|
| **Status** | ⚠️ Configurable (disabled for dev) |
| **Pattern** | OAuth 2.0 / OIDC |
| **Mapping** | AzureAdObjectId in User entity |

**Auth Discovery**:
- `GET /auth/discover` - Lookup user's auth method by email

---

## 8. Azure Services

| Service | Purpose | Status | Endpoint |
|---------|---------|--------|----------|
| SQL Database | Relational data | ✅ Active | - |
| Cosmos DB | Documents + Events | ✅ Active | - |
| Service Bus | Async messaging | ✅ Active | Outbox pattern |
| Key Vault | Secrets | ✅ Active | - |
| Blob Storage | File storage | ✅ Active | AttachmentController, FilesController |
| App Insights | Monitoring | ✅ Active | Telemetry |
| Communication Services | Email | ✅ Active | SMTP replacement |

---

## 9. File Storage (Azure Blob)

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Pattern** | SAS token for direct upload |
| **Max Size** | 10MB per file |
| **Controller** | AttachmentController, FilesController |

**Operations**:
- `POST /attachment/request-upload` - Get SAS token for upload
- `POST /attachment/confirm-upload` - Confirm and store metadata
- `GET /attachment/download-url/{blobName}` - Get temporary download URL (15 min)
- `DELETE /attachment/{blobName}` - Soft delete
- `GET /attachment/vendor/{vendorId}` - List vendor attachments

**File Operations**:
- `POST /files/upload` - Upload file (with context)
- `GET /files/{fileId}` - Get metadata
- `GET /files/download/{fileId}` - Download file
- `GET /files/list` - List by entity
- `DELETE /files/{fileId}` - Delete

---

## 10. GDPR Compliance Layer

| Attribute | Value |
|-----------|-------|
| **Status** | ✅ Active |
| **Articles** | 15-21 implemented |
| **Controller** | GdprController |

**GDPR Rights Endpoints**:
| Right | Article | Endpoint | Method |
|-------|---------|----------|--------|
| Access | 15 | `/gdpr/data-export/{vendorId}` | GET |
| Rectification | 16 | `/gdpr/data-correction/{vendorId}` | PUT |
| Erasure | 17 | `/gdpr/data-deletion/{vendorId}` | DELETE |
| Portability | 20 | `/gdpr/data-portability/{vendorId}` | GET |
| Restriction | 18 | `/gdpr/restrict-processing/{vendorId}` | POST |
| Object | 21 | `/gdpr/object-processing/{vendorId}` | POST |

---

## System Health Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | General health check |
| `GET /health/email-service` | Email service status |
| `GET /system/data-sources` | All data source status |
| `GET /system/services` | Mock vs Real status |

---

## Config Pattern

```json
{
  "Services": {
    "Sap": { "UseMock": true },
    "Email": { "UseMock": false },
    "Sanctions": { "UseMock": true },
    "FileStorage": { "UseMock": false },
    "Bank": { "UseMock": false }
  }
}
```

---

## Integration Status Summary

| Integration | Status | Mock Available | Production Ready |
|-------------|--------|----------------|------------------|
| SAP BAPI | ⏸️ Mock | ✅ Yes | ❌ Pending access |
| Email Service | ✅ Active | ✅ Yes | ✅ Yes |
| Sanctions Screening | ✅ Active | ✅ Yes | ⚠️ Phase 1 |
| Bank Validation | ✅ Active | ❌ No | ✅ Yes |
| MFA/2FA | ✅ Active | ❌ No | ✅ Yes |
| Magic Link | ✅ Active | ❌ No | ✅ Yes |
| Azure AD | ⚠️ Configurable | N/A | ✅ Yes |
| Blob Storage | ✅ Active | ✅ Yes | ✅ Yes |
| GDPR | ✅ Active | ❌ No | ✅ Yes |
