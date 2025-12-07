# 📧 Setup Email Sending - Step by Step

## Current Status
❌ **SMTP is currently DISABLED** - emails are only logged to console

---

## Quick Setup (5 minutes)

### Step 1: Choose Your Email Provider

**Option A: Gmail (Easiest)**
- Free
- Easy to set up
- Good for testing

**Option B: Outlook/Hotmail**
- Free
- Similar to Gmail

**Option C: SendGrid (Production)**
- Free tier: 100 emails/day
- Professional service
- Better for production

---

## Step 2: Configure SMTP Settings

### For Gmail:

1. **Enable 2-Factor Authentication** on your Google account
   - Go to: https://myaccount.google.com/security
   - Enable 2-Step Verification

2. **Generate App Password**:
   - Go to: https://myaccount.google.com/apppasswords
   - Select "Mail" and your device
   - Click "Generate"
   - **Copy the 16-character password** (looks like: `abcd efgh ijkl mnop`)

3. **Edit** `backend/VendorMdm.Api/appsettings.Development.json`:

```json
{
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
}
```

**Replace**:
- `your-email@gmail.com` → Your Gmail address
- `your-16-char-app-password` → The 16-character app password (remove spaces)

### For Outlook/Hotmail:

```json
{
  "EmailService": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp-mail.outlook.com",
      "Port": 587,
      "Username": "your-email@outlook.com",
      "Password": "your-outlook-password",
      "FromEmail": "your-email@outlook.com",
      "FromName": "Vendor Management",
      "UseSsl": true
    }
  }
}
```

---

## Step 3: Restart Backend

**Important**: After changing configuration, you MUST restart the backend:

1. Stop the backend (Ctrl+C in terminal)
2. Start it again:
   ```bash
   cd backend/VendorMdm.Api
   dotnet run
   ```

---

## Step 4: Test Email Sending

1. **Go to**: `http://localhost:3002/admin/invitations`
2. **Click the Mail icon** (📧) next to any pending invitation
3. **Check your email inbox** (and spam folder)
4. **Check backend console** for:
   ```
   ✅ Email sent via SMTP to: vendor@example.com
   ```

---

## Troubleshooting

### ❌ "SMTP configuration incomplete"
- Make sure `Enabled: true`
- Check that `Host`, `Username`, and `Password` are all filled in
- Verify no empty strings

### ❌ "Authentication failed"
**For Gmail**:
- ✅ Use App Password (not your regular password)
- ✅ Make sure 2FA is enabled
- ✅ Remove spaces from app password

**For Outlook**:
- ✅ Use your regular password
- ✅ Make sure account is not locked

### ❌ "Connection timeout"
- Check your internet connection
- Verify firewall isn't blocking port 587
- Try port 465 with `UseSsl: true`

### ❌ Email not received
- Check spam/junk folder
- Verify recipient email is correct
- Check backend console for errors
- Look for: `❌ SMTP error: [error message]`

---

## Security: Use User Secrets (Recommended)

**Don't commit passwords to git!**

```bash
cd backend/VendorMdm.Api

# Initialize user secrets (if not done)
dotnet user-secrets init

# Set password securely
dotnet user-secrets set "EmailService:Smtp:Password" "your-app-password"
dotnet user-secrets set "EmailService:Smtp:Username" "your-email@gmail.com"
```

Then in `appsettings.Development.json`, leave password empty:
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

## Example Configuration

### Complete Gmail Example:

```json
{
  "EmailService": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "mycompany@gmail.com",
      "Password": "abcd efgh ijkl mnop",
      "FromEmail": "mycompany@gmail.com",
      "FromName": "Vendor Management Portal",
      "UseSsl": true
    }
  }
}
```

**Important**: Remove spaces from the app password when pasting!

---

## Verify Configuration

After configuring, you should see in backend console when sending:

**Success**:
```
✅ Email sent via SMTP to: vendor@example.com
```

**Error**:
```
❌ SMTP error: Authentication failed
```

---

## Next Steps

1. ✅ Configure SMTP settings
2. ✅ Set `Enabled: true`
3. ✅ Restart backend
4. ✅ Test by resending an invitation
5. ✅ Check email inbox!

---

## Still Not Working?

1. **Check backend console** for error messages
2. **Verify SMTP settings** are correct
3. **Test with a simple email client** (like Mail app) using same settings
4. **Check firewall/antivirus** isn't blocking SMTP
5. **Try different port**: 465 (SSL) instead of 587 (TLS)

