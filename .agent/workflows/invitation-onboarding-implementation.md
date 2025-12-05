---
description: Invitation-Based Vendor Onboarding Implementation Plan
---

# Invitation-Based Vendor Onboarding Flow

## Overview
Implement a new invitation-based onboarding flow while keeping the existing self-registration option. The invitation flow starts from the internal team and generates unique, time-bound invitation links.

## Key Requirements
1. **Keep existing self-registration flow** (no changes to current `/register` route)
2. **Add invitation-based flow** that:
   - Starts from internal team (Approver/Admin role)
   - Uses the same full vendor registration form
   - Generates unique, time-bound invitation links (7, 14, or 30 days)
   - Links are tied to specific email addresses
   - Single-use access for account creation
   - After link validation, vendor creates credentials and completes the form

## Architecture

### Database Schema Changes

#### New Entity: `VendorInvitation`
```csharp
- Id (Guid, PK)
- InvitationToken (string, unique, indexed)
- VendorLegalName (string, required)
- PrimaryContactEmail (string, required, indexed)
- InvitedBy (Guid, FK to UserRole)
- InvitedByName (string)
- CreatedAt (DateTime)
- ExpiresAt (DateTime)
- Status (enum: Pending, Accepted, Expired, Completed)
- CompletedAt (DateTime, nullable)
- VendorApplicationId (Guid, nullable, FK to VendorApplication)
- Notes (string, nullable) // Internal notes about why vendor is being onboarded
```

### Backend Components

#### 1. New API Endpoints (`VendorController.cs` or new `InvitationController.cs`)

**POST `/api/invitation/create`** (Auth: Approver/Admin only)
- Input: `{ vendorLegalName, primaryContactEmail, expirationDays, notes }`
- Validates requester has appropriate role
- Checks for existing invitation/application with same email
- Generates unique secure token (GUID + hash)
- Creates `VendorInvitation` record
- Triggers email notification (Azure Function)
- Returns: `{ invitationId, invitationLink, expiresAt }`

**GET `/api/invitation/validate/{token}`**
- Input: Unique invitation token
- Validates token exists and is not expired
- Returns: `{ isValid, vendorLegalName, email, expiresAt }`

**POST `/api/invitation/accept/{token}`**
- Input: Token + new account credentials (username, password)
- Validates token
- Creates user account (encrypted password)
- Marks invitation as "Accepted"
- Returns: `{ success, redirectUrl }`

**POST `/api/invitation/complete/{token}`**
- Input: Token + full vendor registration form data
- Validates token is accepted but not completed
- Creates `VendorApplication` record
- Links to invitation
- Marks invitation as "Completed"
- Returns: `{ applicationId, status }`

**GET `/api/invitation/list`** (Auth: Approver/Admin only)
- Returns paginated list of all invitations
- Filters: status, date range, invited by
- Returns: `{ invitations[], totalCount, page }`

**POST `/api/invitation/resend/{id}`** (Auth: Approver/Admin only)
- Generates new token and extends expiration
- Resends email
- Returns: `{ success, newExpiresAt }`

#### 2. Email Service Integration (Azure Function)

**Function: `SendInvitationEmail`**
- Triggered by Service Bus message from invitation creation
- Email template with:
  - Company branding
  - Clear subject: "Action Required: Invitation to Register as Vendor with [Company]"
  - Context and rationale
  - Large "Start Registration Now" button with unique link
  - Time estimate (15-20 minutes)
  - List of required documents
  - Support contact
  - Expiration notice

### Frontend Components

#### 1. New Page: `/admin/invite-vendor` (Internal)

**Component: `InviteVendorForm.tsx`**
- Form fields:
  - Vendor Legal Name (required)
  - Primary Contact Email (required, validated)
  - Expiration Period (dropdown: 7, 14, 30 days)
  - Internal Notes (textarea, optional)
- On submit:
  - Calls POST `/api/invitation/create`
  - Shows success message with copy-to-clipboard link
  - Option to send another invitation
- Only accessible to Approver/Admin roles

#### 2. New Page: `/admin/invitations` (Internal)

**Component: `InvitationManagement.tsx`**
- Table showing all invitations with:
  - Vendor Name
  - Contact Email
  - Status (with badges)
  - Invited By
  - Created Date
  - Expires Date
  - Actions (Resend, View Details)
- Filters: Status, Date Range, Invited By
- Pagination
- Resend functionality

#### 3. New Page: `/invitation/accept/:token` (Public)

**Component: `AcceptInvitation.tsx`**
- Validates token on load
- If invalid/expired: Show error with support contact
- If valid:
  - Display vendor name and email (pre-filled, read-only)
  - Account creation form:
    - Username (auto-suggest from email)
    - Password (with strength indicator)
    - Confirm Password
    - Terms acceptance checkbox
  - On submit:
    - Calls POST `/api/invitation/accept/{token}`
    - Redirects to invitation form completion

#### 4. Updated Page: `/invitation/complete/:token` or use existing `/register`

**Component: `CompleteInvitation.tsx` or extend `VendorRegistration.tsx`**
- Pre-fills vendor name and email from invitation
- Shows all form fields (same as self-registration)
- Locked fields: Company Name, Email (from invitation)
- Additional fields: Tax ID, Contact Person, etc.
- On submit:
  - Calls POST `/api/invitation/complete/{token}`
  - Shows success page
  - Sends confirmation email

#### 5. Update Login Page

**Component: Update `Login.tsx`**
- Keep existing "New Vendor? Register Here" link
- Add note: "Have an invitation link? Click the link in your email"

### Routing Updates (`App.tsx`)

```tsx
// Public routes (no auth required)
<Route path="/invitation/accept/:token" element={<AcceptInvitation />} />
<Route path="/invitation/complete/:token" element={<CompleteInvitation />} />

// Admin/Approver routes
<Route path="admin/invite-vendor" element={
  <ProtectedRoute allowedRoles={['Admin', 'Approver']}>
    <InviteVendorForm />
  </ProtectedRoute>
} />
<Route path="admin/invitations" element={
  <ProtectedRoute allowedRoles={['Admin', 'Approver']}>
    <InvitationManagement />
  </ProtectedRoute>
} />
```

### Email Template

**Subject:** Action Required: Invitation to Register as Vendor with [Company Name]

**Body:**
```
Dear [Vendor Legal Name] Team,

You have been invited to register as an approved vendor with [Company Name].

[Brief context about why they're being onboarded]

To complete your registration, please click the button below. This process should take approximately 15-20 minutes.

[Start Registration Now - Large Button]

This invitation link will expire on [Expiration Date].

What you'll need:
• Tax ID (W-9/W-8) or VAT Number
• Legal Entity Information
• Banking Details (IBAN, Account Number)
• Certificate of Insurance
• Primary Contact Information

If you have any questions or encounter issues during registration, please contact our Vendor Management team at:
Email: vendorsupport@company.com
Phone: [Support Number]

Important: This is an automated message. Please do not reply to this email.

Best regards,
[Company Name] Procurement Team

---
Security Notice: This invitation link is unique to your email address ([Email]). Do not share this link with others.
```

## Implementation Steps

### Phase 1: Backend Foundation (Steps 1-4)
1. **Create database model** (`VendorInvitation` entity)
2. **Update DbContext** (add DbSet, migrations)
3. **Create InvitationController** with all endpoints
4. **Create InvitationService** (business logic, token generation)

### Phase 2: Email Integration (Steps 5-6)
5. **Create Azure Function** for sending invitation emails
6. **Create email template** (HTML with inline CSS)

### Phase 3: Frontend - Internal Tools (Steps 7-9)
7. **Create InviteVendorForm** component and page
8. **Create InvitationManagement** component and page
9. **Update navigation** for Admin/Approver users

### Phase 4: Frontend - Vendor Flow (Steps 10-12)
10. **Create AcceptInvitation** component and page
11. **Create/Update CompleteInvitation** component
12. **Update routing** in App.tsx

### Phase 5: Testing & Documentation (Steps 13-15)
13. **Test complete flow** (invite → accept → complete → approval)
14. **Update documentation** (README, API docs)
15. **Deploy and monitor**

## Security Considerations

1. **Token Security:**
   - Use cryptographically secure random tokens (GUID + HMAC hash)
   - Store only hashed tokens in database
   - Implement rate limiting on validation endpoints

2. **Email Validation:**
   - Verify email format
   - Check for duplicate invitations
   - Prevent email enumeration attacks

3. **Expiration:**
   - Enforce strict expiration checks
   - Auto-expire tokens via background job

4. **Access Control:**
   - Only Approver/Admin can create invitations
   - Log all invitation activities for audit trail

5. **CSRF Protection:**
   - Use anti-forgery tokens on forms
   - Validate referer headers

## Migration Strategy

1. **No breaking changes** to existing self-registration flow
2. **Gradual rollout:**
   - Phase 1: Internal testing with test invitations
   - Phase 2: Pilot with select vendors
   - Phase 3: Full rollout
3. **Feature flag:** Add config to enable/disable invitation flow
4. **Analytics:** Track adoption rate of each onboarding method

## Success Metrics

- % of vendors onboarded via invitation vs self-registration
- Average time from invitation sent to completed registration
- Invitation acceptance rate
- Token expiration rate (should be low)
- Support tickets related to invitation flow
