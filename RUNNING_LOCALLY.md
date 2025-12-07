# 🚀 Running Locally - Development Mode

## Current Status

### ✅ Frontend
- **Status**: Starting/Running
- **URL**: http://localhost:5173
- **Framework**: React 19 + Vite
- **Hot Reload**: Enabled

### ⏳ Backend
- **Status**: Starting (if .NET SDK available)
- **URL**: http://localhost:5001
- **Swagger UI**: http://localhost:5001/swagger
- **Framework**: ASP.NET Core 8

---

## Access the Application

### Frontend
Open in browser: **http://localhost:5173**

### Backend API
- **API**: http://localhost:5001/api
- **Swagger**: http://localhost:5001/swagger

---

## Development Features

### Frontend
- ✅ Hot Module Replacement (HMR)
- ✅ Fast refresh
- ✅ Source maps
- ✅ TypeScript checking

### Backend
- ✅ Swagger UI for API testing
- ✅ Hot reload (with `dotnet watch`)
- ✅ Development logging
- ✅ CORS enabled for localhost:5173

---

## Quick Commands

### Start Frontend
```bash
cd frontend
npm run dev
```

### Start Backend
```bash
export PATH="$PATH:$HOME/.dotnet"
cd backend/VendorMdm.Api
dotnet run
```

### Start Both (Separate Terminals)
```bash
# Terminal 1: Frontend
cd frontend && npm run dev

# Terminal 2: Backend
export PATH="$PATH:$HOME/.dotnet"
cd backend/VendorMdm.Api && dotnet run
```

---

## Testing the App

1. **Open**: http://localhost:5173
2. **Login**: Click "Log in as Administrator"
3. **Navigate**: Admin Dashboard → Invite Vendor
4. **Test**: Create an invitation

---

## Troubleshooting

### Frontend Not Loading
- Check if port 5173 is in use
- Kill process: `lsof -ti:5001 | xargs kill`
- Restart: `npm run dev`

### Backend Not Starting
- Verify .NET SDK: `dotnet --version`
- Check port 5001 availability
- Review error messages in terminal

### API Calls Failing
- Ensure backend is running
- Check CORS configuration
- Verify API base URL in browser console

---

## Development URLs

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5001/api
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/swagger (if available)

---

**Both services are starting! Check the terminals for status.** 🚀

