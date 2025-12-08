# 📧 Email Configuration Guide

## Current Status

**In local development**, emails are currently **logged to the console** and not actually sent. To send real emails, you need to configure SMTP.

---

## How to Enable Email Sending

### Option 1: Configure SMTP (Recommended for Local Development)

1. **Edit** `backend/VendorMdm.Api/appsettings.Development.json`

2. **Add your SMTP settings**:

```json
{
  "EmailService": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password",
      "FromEmail": "your-email@gmail.com",
      "FromName": "Vendor Management",
      "UseSsl": true
    }
  }
}
```

### Gmail Configuration

1. **Enable 2-Factor Authentication** on your Google account
2. **Generate an App Password**:
   - Go to: https://myaccount.google.com/apppasswords
   - Select "Mail" and your device
   - Copy the 16-character password
3. **Use the App Password** in the configuration (not your regular password)

**Gmail Settings**:
```json
{
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true
}
```

### Outlook/Hotmail Configuration

```json
{
  "Host": "smtp-mail.outlook.com",
  "Port": 587,
  "UseSsl": true
}
```

### Other SMTP Providers

- **SendGrid**: `smtp.sendgrid.net` (Port: 587)
- **Mailgun**: `smtp.mailgun.org` (Port: 587)
- **Amazon SES**: `email-smtp.us-east-1.amazonaws.com` (Port: 587)
- **Custom SMTP**: Use your provider's SMTP settings

---

## Configuration Options

| Setting | Description | Example |
|---------|-------------|---------|
| `Enabled` | Enable/disable SMTP sending | `true` |
| `Host` | SMTP server hostname | `smtp.gmail.com` |
| `Port` | SMTP server port | `587` (TLS) or `465` (SSL) |
| `Username` | SMTP username/email | `your-email@gmail.com` |
| `Password` | SMTP password/app password | `your-app-password` |
| `FromEmail` | Sender email address | `noreply@yourcompany.com` |
| `FromName` | Sender display name | `Vendor Management` |
| `UseSsl` | Use SSL/TLS encryption | `true` |

---

## Testing Email Sending

1. **Configure SMTP** in `appsettings.Development.json`
2. **Set `Enabled: true`**
3. **Restart the backend API**
4. **Create or resend an invitation**
5. **Check your email inbox** (and spam folder)

### Verify in Backend Console

You should see:
```
✅ Email sent via SMTP to: vendor@example.com
```

If there's an error:
```
❌ SMTP error: [error message]
```

---

## Security Notes

⚠️ **Never commit SMTP passwords to git!**

### Use User Secrets (Recommended)

```bash
cd backend/VendorMdm.Api
dotnet user-secrets init
dotnet user-secrets set "EmailService:Smtp:Password" "your-app-password"
dotnet user-secrets set "EmailService:Smtp:Username" "your-email@gmail.com"
```

Then remove the password from `appsettings.Development.json`:
```json
{
  "EmailService": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "",  // Will use user secret
      "Password": "",  // Will use user secret
      "FromEmail": "your-email@gmail.com",
      "FromName": "Vendor Management",
      "UseSsl": true
    }
  }
}
```

---

## Troubleshooting

### "SMTP configuration incomplete"
- Check that `Host`, `Username`, and `Password` are all set
- Verify `Enabled: true`

### "Authentication failed"
- For Gmail: Use an App Password, not your regular password
- Check that 2FA is enabled
- Verify username and password are correct

### "Connection timeout"
- Check firewall settings
- Verify SMTP host and port are correct
- Try different ports (587 for TLS, 465 for SSL)

### "Email not received"
- Check spam/junk folder
- Verify recipient email address is correct
- Check SMTP logs in backend console

---

## Current Behavior

**Without SMTP configured**:
- ✅ Emails are logged to console
- ✅ Email content is visible in backend terminal
- ❌ Emails are NOT actually sent

**With SMTP configured**:
- ✅ Emails are sent via SMTP
- ✅ Recipients receive actual emails
- ✅ Email content is also logged for debugging

---

## Next Steps

1. Choose an SMTP provider (Gmail, Outlook, SendGrid, etc.)
2. Configure SMTP settings in `appsettings.Development.json`
3. Set `Enabled: true`
4. Restart backend API
5. Test by creating/resending an invitation
6. Check email inbox!

