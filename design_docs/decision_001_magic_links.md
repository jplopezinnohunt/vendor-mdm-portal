# Design Decision: Magic Links (Passwordless) vs. Password + 2FA

## 1. Context
We currently implemented **Password + TOTP (Authenticator App)** for local users (Vendors).
The user suggested evaluating **One-Time Links (Magic Links)**, citing SAP Concur's expiration-based link strategy as a reference.

![Reference Image](file:///Users/jplopez/.gemini/antigravity/brain/be49d805-031a-453e-9217-5a4c2b5ee978/uploaded_media_1769340133476.png)

## 2. Comparison

| Feature | Password + TOTP (Current) | Magic Links (Concur Style) |
| :--- | :--- | :--- |
| **User Experience** | **High Friction**. Requires setup, scanning QR codes, and remembering passwords. | **Low Friction**. User just needs access to email. "Click and Go". |
| **Security** | **High**. Protects even if email is compromised (requires 2nd device). | **Medium**. Relies entirely on Email Security. If email is hacked, account is accessible. |
| **Vendor Suitability**| **Low**. Vendors log in infrequently and often forget passwords/lose 2FA setups. | **High**. Ideal for sporadic access. No passwords to forget. |
| **Support Burden** | **High**. "I lost my phone", "I forgot my password". | **Low**. "I didn't get the email" (Spam folder) is the main issue. |
| **Implementation** | Complex state (Secrets, Recovery Codes). | Simpler state (Token generation, Expiry). |

## 3. Threat Model: Vendor Access
*   **Risk**: Unauthorized access to Vendor Data (Bank Details, Invoices).
*   **Mitigation**: 
    *   **Magic Link** proves ownership of the email address on record.
    *   Most corporate email accounts (vendors) are already protected by their own corporate 2FA.
    *   **Conclusion**: Relying on the Vendor's secure email is an acceptable risk transfer for this use case.

## 4. Proposed Hybrid Strategy

### A. Internal Users (Staff)
*   **Mechanism**: **Azure AD SSO (OIDC)**.
*   **Why**: Centralized control, existing corporate credentials.

### B. External Users (Vendors)
*   **Mechanism**: **Magic Links (Passwordless)**.
*   **Flow**:
    1.  User enters Email.
    2.  System checks if Email exists & is a Vendor.
    3.  System sends email with link (valid for 1 hour).
    4.  User clicks link -> Authenticated JWT issued.
*   **Why**: Eliminates support tickets for password resets; matches industry standard for B2B portals (Concur, Slack, etc.).

## 5. Implementation Changes Required
If adopted, we will refactor the recent "Local Auth" implementation:
1.  **Remove**: `PasswordHash`, `TwoFactorSecret` from UI/Logic (or keep as fallback).
2.  **Add**: `MagicLinkToken`, `MagicLinkExpiresAt` to User entity (Reuse `InvitationToken` fields?).
3.  **Login Page**: Remove Password/TOTP inputs. Replace with "Send Sign-in Link".
4.  **Invitation**: Becomes simply "You are invited, click here to sign in".

## 6. Recommendation
**Adopt Magic Links for Vendors.**
The friction of forcing external vendors to manage a specific password and 2FA app for *our* specific portal is a barrier to adoption. The SAP Concur model is proven and user-friendly.
