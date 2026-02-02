---
description: Invitation Flow Specification
---

# Specification: Vendor Invitation Flow

## Compliance Sidebar
- **Data Model**: [data-model-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/data-model-standards.md) - Hybrid Model
- **Security**: [moderngoldenrules.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/moderngoldenrules.md) - Section 7 (Iron Dome)
- **Architecture**: [hexagonal-architecture-standards.md](file:///Users/jplopez/projects/vendor-mdm-portal/.agent/rules/standards/hexagonal-architecture-standards.md) - Ontology Pattern

## Overview
The Invitation Flow allows internal approvers to invite vendors to register in the system. The flow includes token generation, email delivery, MFA verification, and multi-stage form completion.

## Business Rules
1. **Account Group Determination**: Vendor Type → Account Group mapping (handled by `VendorConcept`)
2. **Token Expiration**: Default 14 days, configurable per invitation
3. **MFA Required**: All invitations require email-based MFA before form access
4. **Stage Progression**: InvitationSent → MfaVerified → InitialInfoCompleted → Enriched

## Data Model
- **Structured Columns**: `VendorLegalName`, `PrimaryContactEmail`, `Status`, `ExpiresAt`
- **JSONB Attributes**: `Currency`, `SapLanguage`, `TaxCode1`, `TaxCode2`, `mfaCode`

## Security
- Tokens are cryptographically secure (32 bytes)
- MFA codes expire after 10 minutes
- Input sanitization via `IInputSanitizer`

## API Endpoints
- `POST /api/invitation/create` - Create invitation (Approver only)
- `GET /api/invitation/validate/{token}` - Validate token (Public)
- `POST /api/invitation/complete/{token}` - Submit application (Public)
