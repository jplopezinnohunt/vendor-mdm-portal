# Service Status Display - Frontend Integration Guide

## Overview

The backend now provides an API endpoint that shows which service implementations are currently active (Mock vs Real). This can be displayed in the frontend footer or a dedicated status page.

---

## API Endpoint

```
GET /api/system/services
```

**No authentication required** - this is a public status endpoint.

---

## Response Format

```typescript
interface ServiceStatusResponse {
  environment: "Local" | "Azure (Mock)" | "Azure (Real)" | "Unknown";
  description: string;
  services: {
    sap: ServiceConfig;
    fileStorage: ServiceConfig;
    sanctionsScreening: ServiceConfig;
    rbac: ServiceConfig;
    masterData: ServiceConfig;
    workflow: ServiceConfig;
    email: ServiceConfig;
  };
  lastChecked: string; // ISO 8601 datetime
}

interface ServiceConfig {
  mode: "Mock" | "Real";
  implementation: string; // e.g., "Simulation/Hardcoded", "OpenSanctions", "Azure Blob"
  configPath: string;
}
```

---

## Example Responses

### Local Development
```json
{
  "environment": "Local",
  "description": "Running locally with Mock services for development",
  "services": {
    "sap": {
      "mode": "Mock",
      "implementation": "Simulation/Hardcoded",
      "configPath": "Services:SAP"
    },
    "fileStorage": {
      "mode": "Mock",
      "implementation": "Simulation/Hardcoded",
      "configPath": "Services:FileStorage"
    },
    "sanctionsScreening": {
      "mode": "Mock",
      "implementation": "Simulation/Hardcoded",
      "configPath": "Services:SanctionsScreening"
    }
  },
  "lastChecked": "2025-12-21T10:56:00.000Z"
}
```

### Azure with Mock Services (Testing)
```json
{
  "environment": "Azure (Mock)",
  "description": "Deployed to Azure using Mock services for testing",
  "services": {
    "sap": {
      "mode": "Mock",
      "implementation": "Simulation/Hardcoded",
      "configPath": "Services:SAP"
    },
    "fileStorage": {
      "mode": "Real",
      "implementation": "AzureBlob",
      "configPath": "Services:FileStorage"
    },
    "sanctionsScreening": {
      "mode": "Mock",
      "implementation": "Simulation/Hardcoded",
      "configPath": "Services:SanctionsScreening"
    }
  },
  "lastChecked": "2025-12-21T10:56:00.000Z"
}
```

### Azure with Real Services (Production)
```json
{
  "environment": "Azure (Real)",
  "description": "Deployed to Azure with Real service integrations",
  "services": {
    "sap": {
      "mode": "Real",
      "implementation": "SapNco",
      "configPath": "Services:SAP"
    },
    "fileStorage": {
      "mode": "Real",
      "implementation": "AzureBlob",
      "configPath": "Services:FileStorage"
    },
    "sanctionsScreening": {
      "mode": "Real",
      "implementation": "OpenSanctions",
      "configPath": "Services:SanctionsScreening"
    }
  },
  "lastChecked": "2025-12-21T10:56:00.000Z"
}
```

---

## Frontend Implementation

### 1. React Component Example

```typescript
import { useState, useEffect } from 'react';

interface ServiceStatus {
  environment: string;
  description: string;
  services: Record<string, { mode: string; implementation: string }>;
  lastChecked: string;
}

export function ServiceStatusFooter() {
  const [status, setStatus] = useState<ServiceStatus | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  useEffect(() => {
    fetch('/api/system/services')
      .then(res => res.json())
      .then(data => setStatus(data))
      .catch(err => console.error('Failed to load service status', err));
  }, []);

  if (!status) return null;

  return (
    <footer className="service-status-footer">
      <div className="status-badge"onClick={() => setIsOpen(!isOpen)}>
        <span className={`env-indicator ${getEnvClass(status.environment)}`}>
          {status.environment}
        </span>
        <span className="status-text">{status.description}</span>
      </div>

      {isOpen && (
        <div className="status-details">
          <h3>Service Configuration</h3>
          <table>
            <thead>
              <tr>
                <th>Service</th>
                <th>Mode</th>
                <th>Implementation</th>
              </tr>
            </thead>
            <tbody>
              {Object.entries(status.services).map(([name, config]) => (
                <tr key={name}>
                  <td>{formatServiceName(name)}</td>
                  <td>
                    <span className={`mode-badge ${config.mode.toLowerCase()}`}>
                      {config.mode}
                    </span>
                  </td>
                  <td>{config.implementation}</td>
                </tr>
              ))}
            </tbody>
          </table>
          <p className="last-checked">
            Last checked: {new Date(status.lastChecked).toLocaleString()}
          </p>
        </div>
      )}
    </footer>
  );
}

function getEnvClass(env: string): string {
  if (env.includes('Local')) return 'local';
  if (env.includes('Mock')) return 'mock';
  if (env.includes('Real')) return 'real';
  return 'unknown';
}

function formatServiceName(name: string): string {
  return name
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim();
}
```

### 2. CSS Styling Example

```css
.service-status-footer {
  position: fixed;
  bottom: 0;
  right: 0;
  z-index: 1000;
  margin: 1rem;
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  cursor: pointer;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.env-indicator {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.env-indicator.local {
  background: #e3f2fd;
  color: #1976d2;
  border: 1px solid #1976d2;
}

.env-indicator.mock {
  background: #fff3e0;
  color: #f57c00;
  border: 1px solid #f57c00;
}

.env-indicator.real {
  background: #e8f5e9;
  color: #388e3c;
  border: 1px solid #388e3c;
}

.status-details {
  position: absolute;
  bottom: 100%;
  right: 0;
  margin-bottom: 0.5rem;
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 1rem;
  min-width: 400px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.mode-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 500;
}

.mode-badge.mock {
  background: #fff3e0;
  color: #f57c00;
}

.mode-badge.real {
  background: #e8f5e9;
  color: #388e3c;
}

.last-checked {
  margin-top: 0.5rem;
  font-size: 0.75rem;
  color: #666;
}
```

---

## UI Recommendations

### 1. Footer Badge (Minimal)
Display a small badge in the bottom-right corner showing just the environment:

```
┌──────────────┐
│ Local  🟢     │  ← Click to expand
└──────────────┘
```

### 2. Expanded View
When clicked, show full service details:

```
┌─────────────────────────────────────────┐
│ Service Configuration                    │
├─────────────────────────────────────────┤
│ SAP                Mock    Simulation    │
│ File Storage       Mock    Simulation    │
│ Sanctions          Mock    Simulation    │
│ RBAC              Mock    Simulation    │
└─────────────────────────────────────────┘
```

### 3. Admin Page
Create a dedicated `/admin/services` page with:
- Full service details
- Refresh button
- Color-coded status indicators
- History of configuration changes

---

## Color Coding Guidelines

**Environment Indicators:**
- 🟢 **Green**: `Azure (Real)` - Production with real integrations
- 🟡 **Orange**: `Azure (Mock)` - Testing with mock services
- 🔵 **Blue**: `Local` - Local development

**Mode Indicators:**
- ✅ **Green Badge**: `Real` - Active integration
- ⚠️ **Orange Badge**: `Mock` - Simulation mode

---

## Use Cases

### 1. Development Team
Shows developers exactly what's Mock vs Real without checking config files.

### 2. QA Team
Confirms which services are being tested (Mock vs Real).

### 3. Operations Team
Verifies production deployment is using Real services.

### 4. Compliance Team
Confirms sanctions screening is using real API (not Mock) in production.

---

## Frontend Integration Steps

1. **Add the API call** to fetch service status on app load
2. **Create footer component** to display environment badge
3. **Add expandable panel** to show full service details
4. **Style with environment colors** for quick visual identification
5. **Add to admin page** for detailed view
6. **Optional**: Add refresh button to check current status

---

## Testing

```bash
# Local testing
curl http://localhost:5000/api/system/services | jq

# Azure testing
curl https://your-api.azurewebsites.net/api/system/services | jq
```

---

## Security Note

This endpoint does NOT expose:
- API keys
- Connection strings
- Passwords
- Sensitive configuration

It only shows:
- Which mode is active (Mock/Real)
- Provider names (public knowledge)
- Configuration paths (non-sensitive)

Safe to expose to authenticated users.
