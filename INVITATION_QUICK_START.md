# Invitation-Based Onboarding - Quick Start Guide

## Overview
The invitation-based onboarding ensures only pre-vetted vendors can register with your organization. This guide will walk you through using the feature.

## For Internal Users (Admin/Approver)

### Creating a Vendor Invitation

1. **Login** to the Vendor Portal with Admin or Approver credentials

2. **Navigate** to "Invite Vendor" in the sidebar navigation

3. **Fill out the invitation form**:
   - **Vendor Legal Name**: Official company name (e.g., "Acme Corporation Ltd.")
   - **Primary Contact Email**: Email of person who will complete registration
   - **Expiration Period**: Choose 7, 14, or 30 days (14 days recommended)
   - **Internal Notes** (optional): Why are you onboarding this vendor?

4. **Submit** the form

5. **Copy the invitation link** from the success page

6. **Send the link** to the vendor contact via email
   > **Note:** In production, this will be automated via email service

### Managing Invitations

1. **Navigate** to "Manage Invitations" (or "Invitations" for Approvers)

2. **View all invitations** in the table:
   - Status badges: Pending, Accepted, Completed, Expired
   - Creation and expiration dates
   - Links to completed applications

3. **Filter by status** using the dropdown

4. **Resend invitations** for Pending or Expired invitations:
   - Click "Resend" in the Actions column
   - New token generated with extended expiration

5. **Track completion**:
   - "Expiring Soon" indicator for invitations expiring within 3 days
   - "View Application" button for completed invitations

## For Vendors (External Users)

### Completing Your Registration

1. **Receive invitation email** with unique link

2. **Click the invitation link**
   - Link format: `https://portal.company.com/invitation/register/{token}`

3. **Verify pre-filled information**:
   - Company Name (read-only, from invitation)
   - Email Address (read-only, from invitation)

4. **Complete required fields**:
   - **Tax ID / VAT Number**: Your company's tax identification
   - **Contact Person**: Full name of primary contact

5. **Submit application**

6. **Confirmation**:
   - See success message
   - Receive confirmation email
   - Application  enters approval workflow

## Invitation Statuses

| Status | Description |
|--------|-------------|
| **Pending** | Invitation created but not yet used by vendor |
| **Accepted** | Vendor validated token but hasn't submitted application yet |
| **Completed** | Vendor submitted application, linked to invitation |
| **Expired** | Invitation expired before use (can be resent) |
| **Cancelled** | Manually cancelled by admin (future feature) |

## Security & Best Practices

### For Internal Users
✅ **DO:**
- Verify vendor email address before creating invitation
- Use  14-day expiration for most cases
- Add internal notes for audit trail
- Monitor invitation status regularly
- Resend expired invitations if still needed

❌ **DON'T:**
- Share invitation links publicly
- Create duplicate invitations for same email
- Use very long expiration periods (30+ days)

### For Vendors
✅ **DO:**
- Complete registration as soon as possible
- Contact support if invitation expired
- Provide accurate information

❌ **DON'T:**
- Share invitation link with others
- Ignore expiration warnings

## Troubleshooting

### "Invalid invitation link"
**Cause:** Token doesn't exist in system  
**Solution:** Contact the person who sent you the invitation

### "This invitation has expired"
**Cause:** Link was not used within expiration period  
**Solution:** Contact your internal representative to resend the invitation

### "An active invitation already exists for this email"
**Cause:** Duplicate invitation attempt  
**Solution:** Check existing invitations list, resend if needed

### "A vendor application already exists for this email"
**Cause:** Vendor already registered  
**Solution:** Use existing vendor account or contact support

## API Reference (For Developers)

### Create Invitation
```http
POST /api/invitation/create
Content-Type: application/json

{
  "vendorLegalName": "Acme Corporation",
  "primaryContactEmail": "contact@acme.com",
  "expirationDays": 14,
  "notes": "New supplier for Project X"
}
```

### Validate Invitation
```http
GET /api/invitation/validate/{token}
```

### Complete Registration
```http
POST /api/invitation/complete/{token}
Content-Type: application/json

{
  "companyName": "Acme Corporation",
  "taxId": "US-123456789",
  "contactName": "John Doe",
  "email": "contact@acme.com"
}
```

## Metrics & Monitoring

Track these metrics in your invitation management dashboard:
- **Total Invitations**: All time count
- **Pending**: Awaiting vendor action
- **Completed**: Successfully registered
- **Expired**: Not used in time
- **Acceptance Rate**: Completed / Total
- **Average Time to Complete**: From creation to submission

## Future Enhancements

Coming soon:
- 📧 Automated email sending
- 🔔 Expiration reminder emails
- 📊 Invitation analytics dashboard
- 📝 Customizable email templates
- 🌐 Multi-language support
- 📤 Bulk invitation upload (CSV)

## Support

For questions or issues:
- **Internal Users**: Contact IT Support
- **Vendors**: Email vendorsupport@company.com
- **Technical Issues**: Check system logs or contact development team
