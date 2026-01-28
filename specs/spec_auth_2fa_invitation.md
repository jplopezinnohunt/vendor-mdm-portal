# Specification: User Invitation & 2FA (Local Auth)

## 1. Overview
This specification defines the "Local Auth" lifecycle adaptation to support creating users via Invitation, enforcing secure Password setup, and mandating Two-Factor Authentication (2FA) via TOTP (Authenticator App) for all local users.

## 2. Requirements
- **Invitation-Only**: Users (Vendors or internal Local users) are invited via email.
- **Secure Onboarding**:
    1. User receives email with Link.
    2. User Click Link -> Validates Token.
    3. User Sets Password.
    4. User Sets up 2FA (QR Code).
    5. User is Active.
- **Login Security**: 
    - Password + TOTP mandatory for Local users.

## 3. Data Model Changes
### [User] Entity
- `InvitationToken` (string, nullable, indexed)
- `InvitationExpiresAt` (DateTime?, nullable)
- `TwoFactorSecret` (string, nullable) - Stores the seeded secret for TOTP.
- `TwoFactorEnabled` (bool) - Default false until setup complete.
- `RecoveryCodes` (string) - JSON array of recovery codes.

## 4. Workflows

### A. Invitation (Admin)
1. **Admin** clicks "Invite User" (Frontend `UserManagement`).
2. **Backend**: 
    - Creates `User` with `Status = "Pending"`.
    - Generates specific `InvitationToken`.
    - Sends Email (Simulated or SMTP).

### B. Onboarding (User)
1. **Frontend**: `/accept-invite?token=xyz`
2. **Step 1: Password**: User enters Password + Confirm.
3. **Step 2: 2FA Setup**: 
    - Backend generates `TwoFactorSecret`.
    - Returns `otpauth://` URI.
    - Frontend renders QR Code (using `qrcode.react`).
    - User scans and enters 6-digit code.
4. **Step 3: Completion**:
    - Backend verifies Code.
    - Sets `TwoFactorEnabled = true`.
    - Sets `Status = "Active"`.
    - Clears `InvitationToken`.

### C. Login (User)
1. **Step 1**: User enters Email/Password.
    - If Valid -> Check `TwoFactorEnabled`.
    - If False (and not pending) -> Allow (or force setup).
    - If True -> Return `2FA_REQUIRED` signal (not a full token).
2. **Step 2**: User enters 6-digit TOTP.
    - Verify against `TwoFactorSecret`.
    - If Valid -> Issue JWT.

## 5. Technology
- **TOTP Library**: `Otpc` or simple HMAC-SHA1 implementation (Standard RFC 6238).
- **QR Code**: Frontend library `qrcode.react`.

## 6. Endpoints
- `POST /api/auth/invite` (Admin only)
- `POST /api/auth/validate-invite` (Public checks token)
- `POST /api/auth/complete-setup` (Public with Token: Password + Verify 2FA)
- `POST /api/auth/login-2fa` (Step 2 of login)
