# 📊 Service Calls Documentation - Vendor MDM Portal

## Overview
This document provides a comprehensive breakdown of all service calls across the application, indicating which are **Mock** (client-side simulation) and which are **Real** (backend API calls).

---

## 🔐 Authentication System

### **Login Page** (`/login`)
| Feature | Service Type | Implementation | Notes |
|---------|-------------|----------------|-------|
| User Authentication | **MOCK** | `AuthContext.tsx` - localStorage | Simulates Azure AD B2C login |
| Role Assignment | **MOCK** | Client-side | Three roles: Vendor, Approver, Admin |
| Session Management | **MOCK** | localStorage (`mdm_user`) | Persists across page refreshes |

**Service Call Flow:**
```typescript
// Mock Authentication (AuthContext.tsx)
login(role: UserRole) → localStorage.setItem('mdm_user', JSON.stringify(mockUser))
```

**Status Indicators on Login Page:**
- 🟡 **Mock Authentication** - Uses localStorage
- 🔴 **Backend: Offline** - No real API connection

---

## 👤 Vendor Portal

### **Vendor Profile** (`/profile`)
| Feature | Service Type | Implementation | Fallback |
|---------|-------------|----------------|----------|
| Get Current Vendor Data | **HYBRID** | `VendorService.getCurrentVendor()` | Falls back to mock if API fails |
| Display SAP Master Data | **MOCK** | `MOCK_VENDOR_DATA` | Hardcoded vendor data |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 118-146
getCurrentVendor() {
  try {
    // 1. Try Real Backend API
    const response = await api.get('/vendor/100450');
    return mapBackendData(response.data);
  } catch (error) {
    // 2. Fallback to Mock Data
    console.warn('Backend unreachable, using Mock Data');
    return MOCK_VENDOR_DATA; // Simulated delay: 800ms
  }
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (backend offline)
- 📊 **Mock Data**: Acme Corp (SAP ID: 100450)

---

### **Dashboard** (`/dashboard`)
| Feature | Service Type | Implementation | Fallback |
|---------|-------------|----------------|----------|
| Get Change Requests | **HYBRID** | `VendorService.getChangeRequests()` | Falls back to mock |
| Calculate Statistics | **CLIENT** | Computed from request data | No API call |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 148-159
getChangeRequests() {
  try {
    // 1. Try Real Backend API
    const response = await api.get('/changerequest/vendor/100450');
    return response.data;
  } catch (error) {
    // 2. Fallback to Mock Data
    const myRequests = MOCK_REQUESTS_DB.filter(r => r.vendorId === '100450');
    return myRequests; // Simulated delay: 600ms
  }
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (3 sample requests)
- 📊 **Statistics**: Calculated client-side

---

### **Change Request Form** (`/requests/new`)
| Feature | Service Type | Implementation | Fallback |
|---------|-------------|----------------|----------|
| Submit Change Request | **HYBRID** | `VendorService.submitChangeRequest()` | Falls back to mock |
| File Upload | **CLIENT** | Browser File API | Not sent to backend |
| Form Validation | **CLIENT** | React Hook Form | Client-side only |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 161-201
submitChangeRequest(deltaItems, attachments) {
  try {
    // 1. Try Real Backend API
    const payload = {
      requesterId: '00000000-0000-0000-0000-000000000001',
      sapVendorId: '100450',
      payload: { items: deltaItems }
    };
    const response = await api.post('/changerequest', payload);
    return mapBackendResponse(response.data);
  } catch (error) {
    // 2. Fallback to Mock Submission
    const newReq = createMockRequest(deltaItems);
    MOCK_REQUESTS_DB.unshift(newReq); // Add to mock database
    return newReq; // Simulated delay: 1000ms
  }
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Submission**: Mock (adds to client-side array)
- 📁 **File Uploads**: Not persisted

---

### **Request History** (`/requests`)
| Feature | Service Type | Implementation | Fallback |
|---------|-------------|----------------|----------|
| List All Requests | **HYBRID** | Same as Dashboard | Falls back to mock |
| Filter/Sort Requests | **CLIENT** | JavaScript array methods | No API call |

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (same as Dashboard)
- 🔍 **Filtering**: Client-side

---

## 👔 Approver Portal

### **Approver Worklist** (`/approver/worklist`)
| Feature | Service Type | Implementation | Notes |
|---------|-------------|----------------|-------|
| Get All Change Requests | **MOCK** | `VendorService.getAllChangeRequests()` | Returns all mock requests |
| Get Onboarding Requests | **MOCK** | `VendorService.getOnboardingRequests()` | Returns mock applications |
| Filter by Status | **CLIENT** | JavaScript filter | No API call |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 231-235
getAllChangeRequests() {
  return new Promise((resolve) => {
    setTimeout(() => resolve([...MOCK_REQUESTS_DB]), 600);
  });
}

// vendorService.ts - Line 205-209
getOnboardingRequests() {
  return new Promise((resolve) => {
    setTimeout(() => resolve([...MOCK_ONBOARDING_DB]), 600);
  });
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (shared with Vendor portal)
- 📊 **Mock Data**: 3 change requests, 2 onboarding applications

---

### **Request Review** (`/approver/requests/:id`)
| Feature | Service Type | Implementation | Fallback |
|---------|-------------|----------------|----------|
| Get Request Details | **HYBRID** | `VendorService.getChangeRequestById()` | Falls back to mock |
| Approve Request | **HYBRID** | `VendorService.processChangeRequest()` | Falls back to mock |
| Reject Request | **MOCK** | Not implemented in backend | Mock only |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 237-248
getChangeRequestById(id) {
  try {
    // 1. Try Real Backend API
    const response = await api.get(`/changerequest/${id}`);
    return response.data;
  } catch (error) {
    // 2. Fallback to Mock Data
    const req = MOCK_REQUESTS_DB.find(r => r.id === id);
    return req; // Simulated delay: 400ms
  }
}

// vendorService.ts - Line 250-271
processChangeRequest(id, status, comment) {
  try {
    // 1. Try Real Backend API (Approve only)
    if (status === 'APPROVED') {
      await api.post(`/changerequest/${id}/approve`, {});
    } else {
      throw new Error("Reject not implemented");
    }
  } catch (error) {
    // 2. Fallback to Mock Processing
    const reqIndex = MOCK_REQUESTS_DB.findIndex(r => r.id === id);
    MOCK_REQUESTS_DB[reqIndex].status = status;
    MOCK_REQUESTS_DB[reqIndex].updatedAt = new Date().toISOString();
    // Simulated delay: 800ms
  }
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock
- ✅ **Approve**: Attempts real API, falls back to mock
- 🟡 **Reject**: Mock only

---

### **Onboarding Review** (`/approver/onboarding/:id`)
| Feature | Service Type | Implementation | Notes |
|---------|-------------|----------------|-------|
| Get Application Details | **MOCK** | `VendorService.getOnboardingRequestById()` | Pure mock |
| Approve/Reject Application | **MOCK** | `VendorService.processOnboardingRequest()` | Pure mock |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 211-216
getOnboardingRequestById(id) {
  return new Promise((resolve) => {
    const app = MOCK_ONBOARDING_DB.find(a => a.id === id);
    setTimeout(() => resolve(app), 400);
  });
}

// vendorService.ts - Line 218-229
processOnboardingRequest(id, status) {
  return new Promise((resolve) => {
    const idx = MOCK_ONBOARDING_DB.findIndex(a => a.id === id);
    MOCK_ONBOARDING_DB[idx].status = status;
    setTimeout(resolve, 800);
  });
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (2 sample applications)
- 🟡 **Processing**: Mock only (updates client-side array)

---

## 🔧 Admin Portal

### **Admin Dashboard** (`/admin/dashboard`)
| Feature | Service Type | Implementation | Notes |
|---------|-------------|----------------|-------|
| Get Workflow Rules | **MOCK** | `VendorService.getWorkflowRules()` | Returns JSON string |
| System Statistics | **CLIENT** | Computed from mock data | No API call |

**Service Call Flow:**
```typescript
// vendorService.ts - Line 273-281
getWorkflowRules() {
  const mockRules = {
    rules: [
      { field: 'BANKN', risk: 'HIGH', approvers: 2 },
      { field: 'STRAS', risk: 'LOW', approvers: 1 }
    ]
  };
  return JSON.stringify(mockRules, null, 2);
}
```

**Current Status:**
- ✅ **UI**: Fully functional
- 🟡 **Data Source**: Mock (hardcoded rules)
- 📊 **Statistics**: Calculated from mock data

---

## 📝 Vendor Registration

### **Registration Form** (`/register`)
| Feature | Service Type | Implementation | Notes |
|---------|-------------|----------------|-------|
| Form Validation | **CLIENT** | React Hook Form | Client-side only |
| Submit Application | **NOT IMPLEMENTED** | No service call | Form submission not connected |

**Current Status:**
- ✅ **UI**: Fully functional
- 🔴 **Submission**: Not implemented
- ✅ **Validation**: Client-side only

---

## 🔄 Service Call Summary

### Real Backend API Endpoints (Attempted)
| Endpoint | Method | Status | Fallback |
|----------|--------|--------|----------|
| `/vendor/:id` | GET | ❌ Offline | ✅ Mock data |
| `/changerequest/vendor/:id` | GET | ❌ Offline | ✅ Mock data |
| `/changerequest` | POST | ❌ Offline | ✅ Mock submission |
| `/changerequest/:id` | GET | ❌ Offline | ✅ Mock data |
| `/changerequest/:id/approve` | POST | ❌ Offline | ✅ Mock approval |

### Mock-Only Services
| Service | Implementation | Data Persistence |
|---------|----------------|------------------|
| Authentication | localStorage | ✅ Persists across sessions |
| Onboarding Requests | In-memory array | ❌ Lost on page refresh |
| Workflow Rules | Hardcoded JSON | N/A |
| User Roles | localStorage | ✅ Persists across sessions |

---

## 🎯 Service Call Indicators

### Visual Indicators on UI
Each screen now includes service call indicators:

**Login Page:**
- 🟡 **Mock Authentication** - Yellow dot (pulsing)
- 🔴 **Backend: Offline** - Red indicator

**Vendor Cards:**
- 🔵 **Form Validation: Client-side** - Blue dot
- 🟡 **Auth: Mock (localStorage)** - Yellow dot (pulsing)

**Internal Access:**
- 🟡 **Mock Roles** - Yellow dot (pulsing)
- ⚪ **No Azure AD** - Gray dot

---

## 🔧 Backend Integration Status

### ✅ Ready for Backend Integration
The following services have **hybrid implementation** and will automatically use real backend when available:

1. **Get Current Vendor** - `GET /vendor/:id`
2. **Get Change Requests** - `GET /changerequest/vendor/:id`
3. **Submit Change Request** - `POST /changerequest`
4. **Get Request Details** - `GET /changerequest/:id`
5. **Approve Request** - `POST /changerequest/:id/approve`

### ⚠️ Needs Backend Implementation
The following services are **mock-only** and need backend endpoints:

1. **Get Onboarding Requests** - No endpoint
2. **Process Onboarding Request** - No endpoint
3. **Get Workflow Rules** - No endpoint
4. **Reject Change Request** - No endpoint
5. **Vendor Registration** - No endpoint

---

## 📊 Mock Data Structure

### Mock Vendor Data
```typescript
{
  sapVendorId: '100450',
  name: 'Acme Corp Global',
  legalForm: 'Inc.',
  taxNumber1: 'US123456789',
  address: { street, city, postalCode, country, region },
  email: 'finance@acme.com',
  phone: '+1 555 0123',
  banks: [{ bankCountry, bankKey, bankAccount, accountHolder }]
}
```

### Mock Change Requests
- **cr-001**: Address change (APPLIED)
- **cr-002**: Bank data change (IN_REVIEW)
- **cr-003**: General change (NEW)

### Mock Onboarding Applications
- **app-001**: Stark Industries (SUBMITTED)
- **app-002**: Wayne Enterprises (SUBMITTED)

### Mock Users
- **Vendor**: Acme Corp Admin (SAP ID: 100450)
- **Approver**: Jane Doe (Finance)
- **Admin**: System Administrator

---

## 🚀 How to Enable Real Backend

### Step 1: Start Backend API
```bash
cd backend/VendorMdm.Api
dotnet run
```

### Step 2: Verify API is Running
- Backend should be at: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### Step 3: Update Frontend API Base URL
The frontend is configured to use `/api` which works with Azure Static Web Apps. For local development, update `src/services/api.ts`:

```typescript
// Change from:
const API_BASE_URL = '/api';

// To:
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
```

### Step 4: Test Integration
1. Login as Vendor
2. Navigate to Profile - should fetch from real API
3. Submit a change request - should POST to real API
4. Check browser console for API calls

---

## 📈 Service Call Flow Diagram

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │
       ├─ Authentication ──────────► localStorage (Mock)
       │
       ├─ Get Vendor Data ─────────┬─► Try: GET /vendor/:id
       │                           └─► Fallback: MOCK_VENDOR_DATA
       │
       ├─ Get Requests ───────────┬─► Try: GET /changerequest/vendor/:id
       │                          └─► Fallback: MOCK_REQUESTS_DB
       │
       ├─ Submit Request ─────────┬─► Try: POST /changerequest
       │                          └─► Fallback: Add to MOCK_REQUESTS_DB
       │
       ├─ Approve Request ────────┬─► Try: POST /changerequest/:id/approve
       │                          └─► Fallback: Update MOCK_REQUESTS_DB
       │
       └─ Onboarding ─────────────► MOCK_ONBOARDING_DB (Pure Mock)
```

---

## 🎨 UI Enhancements

### New Design Features
1. **Animated Gradient Background** - Subtle moving gradients
2. **Glassmorphism Effects** - Frosted glass appearance
3. **Service Status Indicators** - Real-time service status
4. **Loading States** - Spinner animations during login
5. **Premium Card Designs** - Gradient borders and shadows
6. **Smooth Animations** - Fade-in and slide-in effects

### Custom CSS Classes
- `.card-premium` - Premium card with hover effects
- `.text-gradient` - Gradient text effect
- `.glass` - Glassmorphism background
- `.btn-premium` - Premium button with gradient
- `.animate-fade-in` - Fade-in animation
- `.animate-blob` - Floating blob animation

---

**Last Updated:** 2025-12-03  
**Version:** v8  
**Status:** Mock Mode (Backend Offline)
