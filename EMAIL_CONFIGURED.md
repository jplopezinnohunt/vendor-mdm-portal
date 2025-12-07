# ✅ Email Configuration - Almost Ready!

## What I've Done

✅ **SMTP Enabled**: Set `Enabled: true`  
✅ **Gmail Host**: Configured `smtp.gmail.com`  
✅ **App Password**: Added your app password (spaces removed)  
✅ **Port & SSL**: Configured for Gmail (587, SSL enabled)

---

## ⚠️ Action Required: Add Your Email Address

I've configured everything **except your Gmail address**. You need to:

1. **Open**: `backend/VendorMdm.Api/appsettings.Development.json`

2. **Find** this section (around line 32-33):
   ```json
   "Username": "YOUR_EMAIL@gmail.com",
   "FromEmail": "YOUR_EMAIL@gmail.com",
   ```

3. **Replace** `YOUR_EMAIL@gmail.com` with your actual Gmail address

   For example, if your email is `john.doe@gmail.com`:
   ```json
   "Username": "john.doe@gmail.com",
   "FromEmail": "john.doe@gmail.com",
   ```

---

## After Adding Your Email

1. **Save** the file
2. **Restart the backend**:
   ```bash
   # Stop backend (Ctrl+C)
   # Then restart:
   cd backend/VendorMdm.Api
   dotnet run
   ```
3. **Test** by resending an invitation

---

## Current Configuration

```json
{
  "EmailService": {
    "Smtp": {
      "Enabled": true,
      "Host": "smtp.gmail.com",
      "Port": 587,
      "Username": "YOUR_EMAIL@gmail.com",  ← Replace this
      "Password": "mxobzmcgiggvrwqb",      ← ✅ Already set
      "FromEmail": "YOUR_EMAIL@gmail.com", ← Replace this
      "FromName": "Vendor Management",
      "UseSsl": true
    }
  }
}
```

---

## Security Note

⚠️ **Your app password is in the config file**. For production, use User Secrets:

```bash
cd backend/VendorMdm.Api
dotnet user-secrets set "EmailService:Smtp:Password" "mxobzmcgiggvrwqb"
dotnet user-secrets set "EmailService:Smtp:Username" "your-email@gmail.com"
```

Then remove the password from the config file.

---

## Next Steps

1. ✅ Add your Gmail address to the config
2. ✅ Save the file
3. ✅ Restart backend
4. ✅ Test email sending!

Once you add your email and restart, emails will be sent! 🎉

