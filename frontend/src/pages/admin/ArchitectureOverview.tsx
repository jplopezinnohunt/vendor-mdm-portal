import React, { useState } from 'react';
import { Card } from '../../components/ui/Elements';
import {
    Server,
    Database,
    Cloud,
    Radio,
    KeyRound,
    HardDrive,
    Globe,
    Layers,
    ArrowRight,
    ArrowDown,
    CheckCircle,
    AlertTriangle,
    XCircle,
    Hexagon,
    Monitor,
    Cpu,
    Shield,
    Activity,
    Zap,
    Box,
} from 'lucide-react';

// --- Types ---

interface AzureResource {
    name: string;
    type: string;
    tier: string;
    icon: React.ElementType;
    color: string;
    purpose: string;
    hexagonalRole: string;
}

type TabId = 'resources' | 'hexagonal' | 'flow' | 'gaps';

// --- Data ---

const AZURE_RESOURCES: AzureResource[] = [
    {
        name: 'app-vendor-mdm-api-dev',
        type: 'App Service',
        tier: 'F1 Free (Linux, .NET 8.0)',
        icon: Server,
        color: 'blue',
        purpose: 'Backend API — 22 REST controllers handling vendor onboarding, change requests, approval workflows, sanctions screening, and SAP integration. Orchestrates the SQL \u2192 Cosmos \u2192 Service Bus flow.',
        hexagonalRole: 'Inbound Port (API Layer)',
    },
    {
        name: 'asp-vendor-mdm-dev',
        type: 'App Service Plan',
        tier: 'F1 Free',
        icon: Cpu,
        color: 'blue',
        purpose: 'Compute plan — defines CPU/memory allocation. F1 Free tier for development (no cost, no always-on). Production moves to paid tier or Container Apps.',
        hexagonalRole: 'Infrastructure',
    },
    {
        name: 'cosmos-vendor-mdm-dev',
        type: 'Azure Cosmos DB',
        tier: 'Serverless (SQL API)',
        icon: Cloud,
        color: 'purple',
        purpose: 'Document store & event sourcing — stores immutable domain events (DomainEvents) and full request payloads (InvitationArtifacts). Enables audit trail, compliance, and data reconstruction. Pay-per-request.',
        hexagonalRole: 'Persistence Port (Functional Log)',
    },
    {
        name: 'sql-vendor-mdm-dev',
        type: 'SQL Server',
        tier: 'Basic (5 DTU, 2 GB)',
        icon: Database,
        color: 'green',
        purpose: 'Transactional state store — 27 tables for structured data: applications, change requests, invitations, workflows, canonical entities, audit logs, and the Outbox for guaranteed event delivery.',
        hexagonalRole: 'Persistence Port (State Store)',
    },
    {
        name: 'VendorMdmDb',
        type: 'SQL Database',
        tier: 'Basic (5 DTU)',
        icon: Database,
        color: 'green',
        purpose: 'Relational database with hybrid model: indexed SQL columns for fast queries + JSON Attributes column for schema flexibility without migrations.',
        hexagonalRole: 'Persistence Port (State Store)',
    },
    {
        name: 'sb-vendor-mdm-dev',
        type: 'Service Bus',
        tier: 'Basic',
        icon: Radio,
        color: 'orange',
        purpose: 'Async message bus — decouples event producers (API) from consumers (Azure Functions). Currently 1 queue (invitation-created) for invitation email processing. Enables retry and dead-letter handling.',
        hexagonalRole: 'Outbound Port (Event Bus)',
    },
    {
        name: 'kv-vendor-mdm-dev',
        type: 'Key Vault',
        tier: 'Standard',
        icon: KeyRound,
        color: 'yellow',
        purpose: 'Secrets management — stores all connection strings (SQL, Cosmos, Service Bus, Blob) and SMTP credentials. App Service reads secrets at startup via Managed Identity.',
        hexagonalRole: 'Infrastructure (Security)',
    },
    {
        name: 'stvendormdmdev',
        type: 'Storage Account',
        tier: 'Standard_LRS',
        icon: HardDrive,
        color: 'indigo',
        purpose: 'Blob storage — vendor-attachments container for uploaded documents (contracts, tax certificates, bank letters). Public access disabled, 30-day soft delete.',
        hexagonalRole: 'Outbound Port (File Storage)',
    },
    {
        name: 'swa-vendor-mdm-dev',
        type: 'Static Web App',
        tier: 'Free',
        icon: Globe,
        color: 'cyan',
        purpose: 'Frontend hosting — React 19 SPA with global CDN, automatic SSL, GitHub CI/CD. Handles SPA routing, security headers (CSP, HSTS, X-Frame-Options), and proxies /api/* to backend.',
        hexagonalRole: 'Presentation Layer',
    },
];

const HEXAGONAL_LAYERS = [
    {
        id: 'core',
        name: 'Core Domain (Golden Kernel)',
        location: 'backend/VendorMdm.Shared/',
        color: 'amber',
        description: 'Pure business entities, DTOs, domain events, and JSON schema definitions. MUST NOT reference SAP, SQL, or HTTP libraries.',
        items: ['Canonical Entities', 'Domain Events', 'DTOs & Contracts', 'Business Rules', 'Constants'],
    },
    {
        id: 'inbound',
        name: 'Inbound Port (API Layer)',
        location: 'backend/VendorMdm.Api/Controllers/',
        color: 'blue',
        description: 'Contract-first REST APIs. Accepts requests, validates payloads, assigns Correlation IDs, delegates to Service Layer.',
        items: ['22 REST Controllers', 'Input Sanitization', 'API Versioning (v1.0)', 'Rate Limiting', 'Swagger/OpenAPI'],
    },
    {
        id: 'persistence',
        name: 'Persistence Port (Hybrid SQL/JSONB)',
        location: 'backend/VendorMdm.Api/Data/ & Services/',
        color: 'green',
        description: 'State management and audit logging. SQL for transactional state, Cosmos DB for immutable event log.',
        items: ['SQL (27 tables + JSON Attributes)', 'Cosmos DB (Event Sourcing)', 'Entity Framework Core 8', 'Outbox Pattern'],
    },
    {
        id: 'outbound',
        name: 'Outbound Port (Event Bus)',
        location: 'backend/VendorMdm.Api/Services/',
        color: 'orange',
        description: 'Publishing domain events. Services emit events to Cosmos DB (Event Store) and Service Bus for async processing.',
        items: ['Domain Event Dispatcher', 'Service Bus Publisher', 'SignalR Real-time Hub', 'Outbox Processor'],
    },
    {
        id: 'acl',
        name: 'Anti-Corruption Layer (ACL)',
        location: 'MigrationRunner / Function Apps',
        color: 'red',
        description: 'Translation between Canonical and External formats via Hub-and-Spoke pattern. ExternalSystemMapping is the ONLY bridge.',
        items: ['ExternalSystemMapping Table', 'SAP Adapter (LIFNR \u2194 GUID)', 'SapMapperService', 'Canonical Entity Decoupling'],
    },
];

const GAP_ITEMS = [
    { severity: 'critical', label: 'Cosmos MdmCore DB not deployed', detail: 'Code references MdmCore database but only VendorMdm exists in Azure. Entity services will fail in Connected mode.' },
    { severity: 'critical', label: '4 of 5 Service Bus queues missing', detail: 'Code sends to invitation-emails, vendor-applications, vendor-status, approvals \u2014 only invitation-created is deployed.' },
    { severity: 'critical', label: 'Health endpoint mismatch', detail: 'CI/CD tests /api/health but code maps /health/live, /health/ready, /health/startup.' },
    { severity: 'warning', label: 'Azure Functions not deployed', detail: '3 functions exist in code (Email, Artifacts, Metadata) but none are deployed in Azure.' },
    { severity: 'warning', label: 'Cosmos partition key casing', detail: 'Azure has /EventType (PascalCase), code may write /eventType (camelCase). Could cause partition mismatches.' },
    { severity: 'warning', label: 'Bicep naming inconsistency', detail: 'Modules use mdmportal-* prefix, main.bicep and deployed resources use vendor-mdm-*.' },
    { severity: 'warning', label: 'Frontend/Backend role mismatch', detail: 'Frontend defines 7 roles but backend only enforces 4 in authorization policies.' },
    { severity: 'info', label: 'Serverless First rule gap', detail: 'Golden rule mandates serverless compute (Functions/Container Apps). Primary API runs on App Service F1.' },
    { severity: 'info', label: 'Schema Evolution not implemented', detail: 'No SchemaVersion property on entities or domain events. Required by hexagonal architecture standard.' },
    { severity: 'info', label: 'Domain Ontology layer missing', detail: 'IOntologyConcept and Ontology layer mandated by architecture standard but not yet implemented.' },
];

// --- Helpers ---

const colorMap: Record<string, { bg: string; text: string; border: string; light: string }> = {
    blue: { bg: 'bg-blue-500', text: 'text-blue-700', border: 'border-blue-200', light: 'bg-blue-50' },
    purple: { bg: 'bg-purple-500', text: 'text-purple-700', border: 'border-purple-200', light: 'bg-purple-50' },
    green: { bg: 'bg-green-500', text: 'text-green-700', border: 'border-green-200', light: 'bg-green-50' },
    orange: { bg: 'bg-orange-500', text: 'text-orange-700', border: 'border-orange-200', light: 'bg-orange-50' },
    yellow: { bg: 'bg-yellow-500', text: 'text-yellow-700', border: 'border-yellow-200', light: 'bg-yellow-50' },
    indigo: { bg: 'bg-indigo-500', text: 'text-indigo-700', border: 'border-indigo-200', light: 'bg-indigo-50' },
    cyan: { bg: 'bg-cyan-500', text: 'text-cyan-700', border: 'border-cyan-200', light: 'bg-cyan-50' },
    amber: { bg: 'bg-amber-500', text: 'text-amber-700', border: 'border-amber-200', light: 'bg-amber-50' },
    red: { bg: 'bg-red-500', text: 'text-red-700', border: 'border-red-200', light: 'bg-red-50' },
};

const severityConfig: Record<string, { icon: React.ElementType; color: string; badge: string }> = {
    critical: { icon: XCircle, color: 'text-red-600', badge: 'bg-red-100 text-red-800' },
    warning: { icon: AlertTriangle, color: 'text-yellow-600', badge: 'bg-yellow-100 text-yellow-800' },
    info: { icon: Activity, color: 'text-blue-600', badge: 'bg-blue-100 text-blue-800' },
};

// --- Sub-components ---

const ResourceCard: React.FC<{ resource: AzureResource }> = ({ resource }) => {
    const colors = colorMap[resource.color] || colorMap.blue;
    const Icon = resource.icon;

    return (
        <div className={`rounded-lg border ${colors.border} ${colors.light} p-4 hover:shadow-md transition-shadow`}>
            <div className="flex items-start gap-3">
                <div className={`flex-shrink-0 rounded-lg ${colors.bg} p-2`}>
                    <Icon className="h-5 w-5 text-white" />
                </div>
                <div className="min-w-0 flex-1">
                    <h4 className="text-sm font-semibold text-gray-900 truncate">{resource.name}</h4>
                    <div className="mt-1 flex flex-wrap items-center gap-2">
                        <span className="inline-flex items-center rounded-full bg-white px-2 py-0.5 text-xs font-medium text-gray-700 border border-gray-200">
                            {resource.type}
                        </span>
                        <span className="text-xs text-gray-500">{resource.tier}</span>
                    </div>
                    <p className="mt-2 text-xs text-gray-600 leading-relaxed">{resource.purpose}</p>
                    <div className="mt-2">
                        <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${colors.border} ${colors.text} bg-white`}>
                            <Hexagon className="mr-1 h-3 w-3" />
                            {resource.hexagonalRole}
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
};

const HexagonalLayer: React.FC<{ layer: typeof HEXAGONAL_LAYERS[0]; isCenter?: boolean }> = ({ layer, isCenter }) => {
    const colors = colorMap[layer.color] || colorMap.blue;

    return (
        <div className={`rounded-lg border-2 ${colors.border} ${isCenter ? 'ring-2 ring-amber-300 ring-offset-2' : ''}`}>
            <div className={`${colors.bg} px-4 py-3 rounded-t-md`}>
                <h4 className="text-sm font-bold text-white">{layer.name}</h4>
                <p className="text-xs text-white/80 mt-0.5 font-mono">{layer.location}</p>
            </div>
            <div className={`${colors.light} px-4 py-3`}>
                <p className="text-xs text-gray-700 leading-relaxed">{layer.description}</p>
                <div className="mt-3 flex flex-wrap gap-1.5">
                    {layer.items.map((item) => (
                        <span
                            key={item}
                            className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-white ${colors.text} border ${colors.border}`}
                        >
                            {item}
                        </span>
                    ))}
                </div>
            </div>
        </div>
    );
};

const FlowStep: React.FC<{ step: string; label: string; detail: string; color: string; icon: React.ElementType; isLast?: boolean }> = ({
    step, label, detail, color, icon: Icon, isLast,
}) => {
    const colors = colorMap[color] || colorMap.blue;

    return (
        <div className="flex items-start gap-3">
            <div className="flex flex-col items-center">
                <div className={`flex h-10 w-10 items-center justify-center rounded-full ${colors.bg} text-white text-sm font-bold`}>
                    {step}
                </div>
                {!isLast && (
                    <div className="mt-1 h-12 w-0.5 bg-gray-200" />
                )}
            </div>
            <div className="pb-8">
                <div className="flex items-center gap-2">
                    <Icon className={`h-4 w-4 ${colors.text}`} />
                    <h4 className="text-sm font-semibold text-gray-900">{label}</h4>
                </div>
                <p className="mt-1 text-xs text-gray-600 leading-relaxed">{detail}</p>
            </div>
        </div>
    );
};

// --- Main Component ---

export const ArchitectureOverview: React.FC = () => {
    const [activeTab, setActiveTab] = useState<TabId>('resources');

    const tabs: { id: TabId; label: string; icon: React.ElementType }[] = [
        { id: 'resources', label: 'Azure Resources', icon: Cloud },
        { id: 'hexagonal', label: 'Hexagonal Architecture', icon: Hexagon },
        { id: 'flow', label: 'Data Flow', icon: Zap },
        { id: 'gaps', label: 'Gap Analysis', icon: AlertTriangle },
    ];

    const criticalCount = GAP_ITEMS.filter(g => g.severity === 'critical').length;
    const warningCount = GAP_ITEMS.filter(g => g.severity === 'warning').length;

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Architecture Overview</h1>
                <p className="mt-1 text-sm text-gray-500">
                    Azure resources, Hexagonal Architecture, and serverless design for the Vendor MDM Portal.
                </p>
            </div>

            {/* Stats Row */}
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div className="rounded-lg border border-blue-200 bg-blue-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <Box className="h-5 w-5 text-blue-600" />
                        <span className="text-xs font-medium text-blue-700 uppercase tracking-wider">Resources</span>
                    </div>
                    <p className="mt-2 text-3xl font-bold text-blue-900">9</p>
                    <p className="text-xs text-blue-600">Azure deployed</p>
                </div>
                <div className="rounded-lg border border-purple-200 bg-purple-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <Hexagon className="h-5 w-5 text-purple-600" />
                        <span className="text-xs font-medium text-purple-700 uppercase tracking-wider">Layers</span>
                    </div>
                    <p className="mt-2 text-3xl font-bold text-purple-900">5</p>
                    <p className="text-xs text-purple-600">Hexagonal ports</p>
                </div>
                <div className="rounded-lg border border-red-200 bg-red-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <XCircle className="h-5 w-5 text-red-600" />
                        <span className="text-xs font-medium text-red-700 uppercase tracking-wider">Critical</span>
                    </div>
                    <p className="mt-2 text-3xl font-bold text-red-900">{criticalCount}</p>
                    <p className="text-xs text-red-600">Gaps found</p>
                </div>
                <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <AlertTriangle className="h-5 w-5 text-yellow-600" />
                        <span className="text-xs font-medium text-yellow-700 uppercase tracking-wider">Warnings</span>
                    </div>
                    <p className="mt-2 text-3xl font-bold text-yellow-900">{warningCount}</p>
                    <p className="text-xs text-yellow-600">Needs attention</p>
                </div>
            </div>

            {/* Tab Navigation */}
            <div className="border-b border-gray-200">
                <nav className="-mb-px flex space-x-6">
                    {tabs.map((tab) => {
                        const Icon = tab.icon;
                        return (
                            <button
                                key={tab.id}
                                onClick={() => setActiveTab(tab.id)}
                                className={`flex items-center gap-2 border-b-2 px-1 py-3 text-sm font-medium transition-colors ${
                                    activeTab === tab.id
                                        ? 'border-brand-500 text-brand-600'
                                        : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
                                }`}
                            >
                                <Icon className="h-4 w-4" />
                                {tab.label}
                            </button>
                        );
                    })}
                </nav>
            </div>

            {/* Tab Content */}
            {activeTab === 'resources' && (
                <div className="space-y-6">
                    <Card>
                        <div className="mb-4">
                            <h3 className="text-lg font-semibold text-gray-900">Deployed Azure Resources</h3>
                            <p className="text-sm text-gray-500 mt-1">
                                Resource Group: <span className="font-mono text-xs bg-gray-100 px-1.5 py-0.5 rounded">rg-vendor-mdm-dev</span>
                                {' '} &middot; Region: Central US &middot; Subscription: <span className="font-mono text-xs bg-gray-100 px-1.5 py-0.5 rounded">8c89e199-...</span>
                            </p>
                        </div>
                        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                            {AZURE_RESOURCES.map((resource) => (
                                <ResourceCard key={resource.name} resource={resource} />
                            ))}
                        </div>
                    </Card>

                    {/* Connection Diagram */}
                    <Card title="How They Connect">
                        <div className="overflow-x-auto">
                            <pre className="text-xs leading-relaxed text-gray-700 font-mono whitespace-pre">
{`  User Browser
       |
       v
  +----------+   static assets    +------------+
  | Browser  |<-------------------| SWA  (9)   |
  |          |                    +------------+
  |          |   /api/* calls     +------------+   secrets    +------------+
  |          |------------------>| API  (1)   |<------------|  KV  (7)   |
  +----------+                    | Plan (2)   |              +------------+
                                  +--+-+-+--+--+
                         +-----------+ | +----------+
                         v             v            v
                    +---------+  +----------+  +----------+
                    | SQL (5) |  |Cosmos (3)|  |  SB (6)  |
                    |Server(4)|  | Events + |  |  Queues  |
                    | 27 tbls |  |Artifacts |  |  Async   |
                    +---------+  +----------+  +----------+
                                                     |
                         +----------+          (Future: Azure
                         |Blob  (8) |           Functions)
                         | Vendor   |
                         | Files    |
                         +----------+`}
                            </pre>
                        </div>
                    </Card>
                </div>
            )}

            {activeTab === 'hexagonal' && (
                <div className="space-y-6">
                    {/* Hexagonal Diagram */}
                    <Card>
                        <div className="mb-4">
                            <h3 className="text-lg font-semibold text-gray-900">Hexagonal Architecture (Ports & Adapters)</h3>
                            <p className="text-sm text-gray-500 mt-1">
                                The platform follows a strict Hexagonal Architecture hosted on serverless infrastructure.
                                Core domain logic is isolated from external systems.
                            </p>
                        </div>

                        {/* Visual Hex Layout */}
                        <div className="space-y-4">
                            {/* Presentation */}
                            <div className="rounded-lg border-2 border-cyan-200 bg-cyan-50 p-3">
                                <div className="flex items-center gap-2">
                                    <Monitor className="h-4 w-4 text-cyan-700" />
                                    <span className="text-sm font-semibold text-cyan-800">Presentation Layer</span>
                                    <span className="text-xs text-cyan-600 font-mono">Static Web App (React 19 + MSAL + SignalR)</span>
                                </div>
                            </div>

                            <div className="flex justify-center">
                                <ArrowDown className="h-5 w-5 text-gray-400" />
                            </div>

                            {/* Inbound */}
                            <HexagonalLayer layer={HEXAGONAL_LAYERS[1]} />

                            <div className="flex justify-center">
                                <ArrowDown className="h-5 w-5 text-gray-400" />
                            </div>

                            {/* Core (center, highlighted) */}
                            <HexagonalLayer layer={HEXAGONAL_LAYERS[0]} isCenter />

                            <div className="flex justify-center">
                                <ArrowDown className="h-5 w-5 text-gray-400" />
                            </div>

                            {/* Persistence + Outbound side by side */}
                            <div className="grid gap-4 md:grid-cols-2">
                                <HexagonalLayer layer={HEXAGONAL_LAYERS[2]} />
                                <HexagonalLayer layer={HEXAGONAL_LAYERS[3]} />
                            </div>

                            <div className="flex justify-center">
                                <ArrowDown className="h-5 w-5 text-gray-400" />
                            </div>

                            {/* ACL */}
                            <HexagonalLayer layer={HEXAGONAL_LAYERS[4]} />
                        </div>
                    </Card>

                    {/* Serverless Mapping */}
                    <Card title="Azure \u2194 Hexagonal Mapping">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Hexagonal Layer</th>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Azure Resource</th>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Serverless?</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    <tr>
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Presentation</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Static Web App (Free)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"><CheckCircle className="mr-1 h-3 w-3" />Yes</span></td>
                                    </tr>
                                    <tr className="bg-gray-50">
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Inbound Port</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">App Service (F1 Free)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800"><AlertTriangle className="mr-1 h-3 w-3" />No (Fixed)</span></td>
                                    </tr>
                                    <tr>
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Persistence (SQL)</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Azure SQL (Basic 5 DTU)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-yellow-100 px-2 py-0.5 text-xs font-medium text-yellow-800"><AlertTriangle className="mr-1 h-3 w-3" />No (Fixed DTU)</span></td>
                                    </tr>
                                    <tr className="bg-gray-50">
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Persistence (Events)</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Cosmos DB (Serverless)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"><CheckCircle className="mr-1 h-3 w-3" />Yes</span></td>
                                    </tr>
                                    <tr>
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Outbound (Bus)</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Service Bus (Basic)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"><CheckCircle className="mr-1 h-3 w-3" />Yes</span></td>
                                    </tr>
                                    <tr className="bg-gray-50">
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Outbound (Files)</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Blob Storage (Standard_LRS)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"><CheckCircle className="mr-1 h-3 w-3" />Yes</span></td>
                                    </tr>
                                    <tr>
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">ACL (Async)</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Azure Functions (Consumption)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-red-100 px-2 py-0.5 text-xs font-medium text-red-800"><XCircle className="mr-1 h-3 w-3" />Not deployed</span></td>
                                    </tr>
                                    <tr className="bg-gray-50">
                                        <td className="px-4 py-3 text-sm font-medium text-gray-900">Security</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">Key Vault (Standard)</td>
                                        <td className="px-4 py-3"><span className="inline-flex items-center rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-800"><CheckCircle className="mr-1 h-3 w-3" />Yes</span></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </Card>
                </div>
            )}

            {activeTab === 'flow' && (
                <div className="space-y-6">
                    <Card>
                        <div className="mb-6">
                            <h3 className="text-lg font-semibold text-gray-900">Mandatory Data Flow (Golden Rule)</h3>
                            <p className="text-sm text-gray-500 mt-1">
                                Every feature MUST follow this flow: <span className="font-mono text-xs bg-gray-100 px-1.5 py-0.5 rounded">SQL &rarr; Cosmos Artifact &rarr; Cosmos Event &rarr; Service Bus</span>
                            </p>
                        </div>

                        <div className="pl-2">
                            <FlowStep
                                step="A"
                                label="SQL Database (State & Metadata)"
                                detail="Create/update entity in SQL with transactional consistency. Set status, timestamps, relationships. Commit transaction. Uses hybrid model: indexed columns + JSON Attributes."
                                color="green"
                                icon={Database}
                            />
                            <FlowStep
                                step="B"
                                label="Cosmos DB \u2014 Artifacts (Full Payload)"
                                detail="Store complete request object as immutable artifact. Enables future data reconstruction, compliance audits, and schema evolution without migrations. Non-blocking (catch exceptions)."
                                color="purple"
                                icon={Cloud}
                            />
                            <FlowStep
                                step="C"
                                label="Cosmos DB \u2014 Events (Domain Event)"
                                detail="Emit domain event (e.g., VendorCreated, InvitationSent) partitioned by EventType. Powers event sourcing, analytics, and integration. Non-blocking."
                                color="purple"
                                icon={Layers}
                            />
                            <FlowStep
                                step="D"
                                label="Service Bus (Async Integration)"
                                detail="Publish message for async processing: email notifications, SAP integration triggers, cross-system events. Uses Outbox Pattern for guaranteed delivery with retry and dead-letter."
                                color="orange"
                                icon={Radio}
                                isLast
                            />
                        </div>
                    </Card>

                    {/* Outbox Pattern Detail */}
                    <Card title="Outbox Pattern (Guaranteed Delivery)">
                        <div className="space-y-3">
                            <div className="flex items-center gap-3 text-sm">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-green-100 text-green-700">
                                    <Database className="h-4 w-4" />
                                </div>
                                <span className="text-gray-700">Service writes event to <span className="font-mono text-xs bg-gray-100 px-1 rounded">OutboxEvents</span> table within same SQL transaction</span>
                            </div>
                            <div className="ml-4 h-6 w-0.5 bg-gray-200" />
                            <div className="flex items-center gap-3 text-sm">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100 text-blue-700">
                                    <Activity className="h-4 w-4" />
                                </div>
                                <span className="text-gray-700"><span className="font-mono text-xs bg-gray-100 px-1 rounded">OutboxProcessor</span> background service polls every 5s, batches of 100</span>
                            </div>
                            <div className="ml-4 h-6 w-0.5 bg-gray-200" />
                            <div className="flex items-center gap-3 text-sm">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-orange-100 text-orange-700">
                                    <Radio className="h-4 w-4" />
                                </div>
                                <span className="text-gray-700">Publishes to Service Bus queue. Retries up to 5x with exponential backoff (5s, 25s, 125s...)</span>
                            </div>
                            <div className="ml-4 h-6 w-0.5 bg-gray-200" />
                            <div className="flex items-center gap-3 text-sm">
                                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-cyan-100 text-cyan-700">
                                    <Monitor className="h-4 w-4" />
                                </div>
                                <span className="text-gray-700">SignalR Hub broadcasts real-time updates to connected browser clients</span>
                            </div>
                        </div>
                    </Card>

                    {/* Code Pattern */}
                    <Card title="Service Code Pattern (Mandatory)">
                        <pre className="text-xs leading-relaxed text-gray-700 font-mono whitespace-pre overflow-x-auto bg-gray-50 rounded-lg p-4">
{`public async Task<Response> CreateAsync(Request request)
{
    // STEP A: SQL - Create entity (State & Metadata)
    var entity = new Entity { Id = Guid.NewGuid(), Status = "Pending" };
    _context.Entities.Add(entity);
    await _context.SaveChangesAsync();

    // STEP B: COSMOS - Store artifact (Full Payload)
    try {
        await SaveArtifactAsync(entity.Id, request);
    } catch (Exception ex) {
        _logger.LogError(ex, "Artifact failed for {Id}", entity.Id);
        // CONTINUE - don't block on artifact failure
    }

    // STEP C: COSMOS - Emit event (Event Sourcing)
    try {
        await EmitDomainEventAsync("EntityCreated", entity.Id, eventData);
    } catch (Exception ex) {
        _logger.LogError(ex, "Event failed for {Id}", entity.Id);
    }

    // STEP D: SERVICE BUS (Optional - Integration)
    await _serviceBusService?.PublishEventAsync("entity-created", message);

    return new Response { /* ... */ };
}`}
                        </pre>
                    </Card>
                </div>
            )}

            {activeTab === 'gaps' && (
                <div className="space-y-6">
                    <Card>
                        <div className="mb-4">
                            <h3 className="text-lg font-semibold text-gray-900">3-Way Cross-Validation Results</h3>
                            <p className="text-sm text-gray-500 mt-1">
                                Discrepancies found between Azure deployed resources, codebase, and Golden Rules.
                            </p>
                        </div>

                        <div className="space-y-3">
                            {GAP_ITEMS.map((gap, index) => {
                                const config = severityConfig[gap.severity];
                                const Icon = config.icon;

                                return (
                                    <div key={index} className="flex items-start gap-3 rounded-lg border border-gray-200 p-4 hover:bg-gray-50 transition-colors">
                                        <Icon className={`h-5 w-5 flex-shrink-0 mt-0.5 ${config.color}`} />
                                        <div className="min-w-0 flex-1">
                                            <div className="flex items-center gap-2">
                                                <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${config.badge}`}>
                                                    {gap.severity.toUpperCase()}
                                                </span>
                                                <h4 className="text-sm font-semibold text-gray-900">{gap.label}</h4>
                                            </div>
                                            <p className="mt-1 text-xs text-gray-600">{gap.detail}</p>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </Card>

                    {/* Serverless Scorecard */}
                    <Card title="Serverless Compliance Scorecard">
                        <div className="grid gap-3 sm:grid-cols-2">
                            {[
                                { label: 'Static Web App', status: 'serverless', detail: 'Free tier, CDN, auto-SSL' },
                                { label: 'Cosmos DB', status: 'serverless', detail: 'Pay-per-RU, no provisioning' },
                                { label: 'Service Bus', status: 'serverless', detail: 'Pay-per-message' },
                                { label: 'Key Vault', status: 'serverless', detail: 'Pay-per-transaction' },
                                { label: 'Blob Storage', status: 'serverless', detail: 'Pay-per-GB + ops' },
                                { label: 'App Service', status: 'fixed', detail: 'F1 Free \u2192 migrate to Container Apps' },
                                { label: 'SQL Database', status: 'fixed', detail: 'Basic 5 DTU \u2192 upgrade to Serverless tier' },
                                { label: 'Azure Functions', status: 'missing', detail: '3 in code, 0 deployed' },
                            ].map((item) => (
                                <div
                                    key={item.label}
                                    className={`flex items-center gap-3 rounded-lg border p-3 ${
                                        item.status === 'serverless' ? 'border-green-200 bg-green-50' :
                                        item.status === 'fixed' ? 'border-yellow-200 bg-yellow-50' :
                                        'border-red-200 bg-red-50'
                                    }`}
                                >
                                    {item.status === 'serverless' ? (
                                        <CheckCircle className="h-5 w-5 text-green-600 flex-shrink-0" />
                                    ) : item.status === 'fixed' ? (
                                        <AlertTriangle className="h-5 w-5 text-yellow-600 flex-shrink-0" />
                                    ) : (
                                        <XCircle className="h-5 w-5 text-red-600 flex-shrink-0" />
                                    )}
                                    <div>
                                        <p className="text-sm font-medium text-gray-900">{item.label}</p>
                                        <p className="text-xs text-gray-600">{item.detail}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </Card>
                </div>
            )}
        </div>
    );
};
