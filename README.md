<div align="center">
<img width="1200" height="475" alt="GHBanner" src="https://github.com/user-attachments/assets/0aa67016-6eaf-458a-adb2-6e31a0763ed6" />
</div>

# Vendor Master Data Portal

A React-based vendor management portal built with TypeScript and Vite, designed for Azure Static Web Apps deployment.

## 🏗️ Tech Stack

- **React 19.2** - Modern UI framework
- **TypeScript 5.8** - Type-safe development  
- **Vite 6.2** - Fast build tool and dev server
- **React Router 7.9** - Client-side routing
- **TailwindCSS** - Utility-first styling
- **Axios** - HTTP client for API calls
- **Lucide React** - Icon library

## 🚀 Local Development

### Prerequisites
- Node.js (v18 or higher)
- npm or yarn

### Setup

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Configure environment variables:**
   ```bash
   cp .env.example .env.local
   ```
   Then edit `.env.local` and add your Gemini API key:
   ```
   GEMINI_API_KEY=your_actual_api_key_here
   ```

3. **Run the development server:**
   ```bash
   npm run dev
   ```
   The app will be available at `http://localhost:3000`

4. **Build for production:**
   ```bash
   npm run build
   ```
   Output will be in the `dist/` directory

## ☁️ Azure Static Web Apps Deployment

### Automatic Deployment (Recommended)

This project is configured for automatic deployment via GitHub Actions:

1. **Create an Azure Static Web App:**
   - Go to [Azure Portal](https://portal.azure.com)
   - Create a new Static Web App resource
   - Select your GitHub repository
   - Azure will automatically add the `AZURE_STATIC_WEB_APPS_API_TOKEN` secret

2. **Configure Environment Variables in Azure:**
   - In Azure Portal, go to your Static Web App
   - Navigate to **Configuration** → **Application settings**
   - Add: `GEMINI_API_KEY` with your API key value

3. **Push to main branch:**
   ```bash
   git push origin main
   ```
   The GitHub Action will automatically build and deploy your app!

### Build Configuration

The Azure deployment uses these settings (configured in `.github/workflows/azure-static-web-apps.yml`):
- **App Location:** `/` (project root)
- **Output Location:** `dist` (Vite build output)
- **Triggers:** Pushes to `main` branch and pull requests

### Static Web App Configuration

Security and routing settings are in `staticwebapp.config.json`:
- Client-side routing fallback to `index.html`
- Enhanced security headers (CSP, X-Frame-Options, etc.)
- Route-based authentication (ready for `/api/*` and `/admin/*` routes)

## 📁 Project Structure

```
vendor-mdm-portal/
├── .github/workflows/       # CI/CD pipelines
├── src/                     # Source code
│   ├── components/          # Reusable React components
│   ├── pages/              # Page components
│   ├── services/           # API services
│   ├── context/            # React context providers
│   ├── App.tsx             # Main app component
│   ├── main.tsx            # Entry point
│   └── types.ts            # TypeScript type definitions
├── index.html              # HTML template
├── package.json            # Dependencies
├── vite.config.ts          # Vite configuration
├── staticwebapp.config.json # Azure SWA configuration
└── tsconfig.json           # TypeScript configuration
```

## 🧠 Domain Model

The application utilizes a **Hybrid Data Architecture**, combining **Azure SQL** for structured relational data and **Azure Cosmos DB** for flexible, high-volume document storage.

### 1. Relational Domain Entities (Azure SQL)
*Managed via Entity Framework Core*

| Entity | Description | Key Properties |
| :--- | :--- | :--- |
| **ChangeRequest** | Central entity for vendor modification or onboarding requests. | `Id` (PK), `Status`, `SapVendorId`, `RequesterId` |
| **VendorApplication** | Initial data for new vendor onboarding requests. | `Id` (PK), `CompanyName`, `ContactEmail`, `Status` |
| **Attachment** | Metadata for uploaded files (e.g., tax docs). | `Id` (PK), `LinkedEntityId`, `BlobUrl` |
| **UserRole** | Manages user permissions and roles. | `Id` (PK), `Username`, `Role` (Admin, Requester, Approver) |
| **WorkflowState** | Reference data for valid request states. | `StateName` (PK), `Description` |
| **SapEnvironment** | Reference data for target SAP environments. | `EnvironmentCode` (PK), `Description` |

### 2. Document Domain Entities (Azure Cosmos DB)
*Managed via Cosmos SDK*

| Entity | Description | Key Properties |
| :--- | :--- | :--- |
| **ChangeRequestData** | Stores the complex JSON payload for a `ChangeRequest`. | `id` (Link to SQL), `requestId` (Partition), `payload` (JSON), `oldValue`/`newValue` |
| **DomainEvent** | Immutable record of system events for auditing/sourcing. | `id`, `eventType` (Partition), `entityId`, `timestamp`, `data` |

### Architecture Pattern
- **Aggregate Root:** `ChangeRequest` (SQL) + `ChangeRequestData` (Cosmos)
- **Entities:** `VendorApplication`, `Attachment`
- **Value Objects:** `WorkflowState`, `SapEnvironment`
- **Domain Events:** `DomainEvent`

## 🔒 Security Features

- Content Security Policy (CSP) headers
- X-Frame-Options protection
- No unsafe-eval in production
- Route-based authentication ready
- Environment variable protection

## 📝 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally

## 🔗 Links

- [Azure Static Web Apps Documentation](https://docs.microsoft.com/en-us/azure/static-web-apps/)
- [Vite Documentation](https://vitejs.dev/)
- [React Documentation](https://react.dev/)

---

Built with ❤️ for vendor master data management
