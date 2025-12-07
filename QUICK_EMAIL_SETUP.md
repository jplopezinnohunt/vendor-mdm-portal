# ⚡ Quick Email Setup (2 Minutes)

## The Problem
Emails are not being sent because **SMTP is disabled**. You need to enable and configure it.

---

## Quick Fix

### 1. Edit Configuration File

Open: `backend/VendorMdm.Api/appsettings.Development.json`

Find this section:
```json
"EmailService": {
  "Smtp": {
    "Enabled": false,  ← Change to true
    "Host": "",        ← Add your SMTP host
    "Username": "",    ← Add your email
    "Password": "",    ← Add your password
```

### 2. For Gmail (Easiest):

**First, get a Gmail App Password:**
1. Go to: https://myaccount.google.com/apppasswords
2. Generate password for "Mail"
3. Copy the 16-character password

**Then update the config:**
```json
"EmailService": {
  "Smtp": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-16-char-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Vendor Management",
    "UseSsl": true
  }
}
```

### 3. Restart Backend

**Stop** the backend (Ctrl+C) and **restart**:
```bash
cd backend/VendorMdm.Api
dotnet run
```

### 4. Test

1. Go to invitations page
2. Click the Mail icon (📧) to resend
3. Check your email inbox!

---

## That's It! 🎉

If you see `✅ Email sent via SMTP` in the backend console, it's working!

For detailed instructions, see: `SETUP_EMAIL_SENDING.md`

