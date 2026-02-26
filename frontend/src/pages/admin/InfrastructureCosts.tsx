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
    Cpu,
    Activity,
    DollarSign,
    ArrowRight,
    CheckCircle,
    AlertTriangle,
    XCircle,
    Layers,
    Shield,
    Zap,
    TrendingUp,
    ExternalLink,
} from 'lucide-react';

// --- Types ---

type TabId = 'overview' | 'environments' | 'comparison' | 'roadmap';

interface ServiceCost {
    name: string;
    icon: React.ElementType;
    color: string;
    pocTier: string;
    pocCost: string;
    devTier: string;
    devCost: string;
    qaTier: string;
    qaCost: string;
    prodTier: string;
    prodCost: string;
    changeNotes: string;
}

interface EnvironmentSummary {
    name: string;
    label: string;
    sapTarget: string;
    resourceGroup: string;
    color: string;
    monthlyCost: string;
    services: number;
    purpose: string;
    status: 'active' | 'planned' | 'future';
}

// --- Data ---

const SERVICE_COSTS: ServiceCost[] = [
    {
        name: 'App Service Plan',
        icon: Cpu,
        color: 'blue',
        pocTier: 'F1 Free',
        pocCost: '$0',
        devTier: 'F1 Free',
        devCost: '$0',
        qaTier: 'B1 Basic',
        qaCost: '~$13',
        prodTier: 'S1 Standard',
        prodCost: '~$70',
        changeNotes: 'AlwaysOn, auto-scale, staging slots, SLA',
    },
    {
        name: 'App Service (API)',
        icon: Server,
        color: 'blue',
        pocTier: 'F1 Free (.NET 8)',
        pocCost: '$0',
        devTier: 'F1 Free (.NET 8)',
        devCost: '$0',
        qaTier: 'B1 Basic (.NET 8)',
        qaCost: 'incl.',
        prodTier: 'S1 Standard (.NET 8)',
        prodCost: 'incl.',
        changeNotes: 'Included in App Service Plan cost',
    },
    {
        name: 'Static Web App',
        icon: Globe,
        color: 'cyan',
        pocTier: 'Free',
        pocCost: '$0',
        devTier: 'Free',
        devCost: '$0',
        qaTier: 'Free',
        qaCost: '$0',
        prodTier: 'Standard',
        prodCost: '~$9',
        changeNotes: 'SLA, custom auth, 100GB bandwidth, staging envs',
    },
    {
        name: 'Cosmos DB',
        icon: Cloud,
        color: 'purple',
        pocTier: 'Serverless',
        pocCost: '~$1-5',
        devTier: 'Serverless',
        devCost: '~$1-5',
        qaTier: 'Serverless',
        qaCost: '~$5-15',
        prodTier: 'Serverless / 400 RU/s',
        prodCost: '~$24-50',
        changeNotes: 'Multi-region option, backup policies, reserved capacity savings',
    },
    {
        name: 'SQL Database',
        icon: Database,
        color: 'green',
        pocTier: 'Basic (5 DTU)',
        pocCost: '~$5',
        devTier: 'Basic (5 DTU)',
        devCost: '~$5',
        qaTier: 'Standard S0 (10 DTU)',
        qaCost: '~$15',
        prodTier: 'Standard S1 (20 DTU)',
        prodCost: '~$30',
        changeNotes: 'More DTUs, 250GB capacity, geo-replication option',
    },
    {
        name: 'Service Bus',
        icon: Radio,
        color: 'orange',
        pocTier: 'Basic',
        pocCost: '<$1',
        devTier: 'Basic',
        devCost: '<$1',
        qaTier: 'Standard',
        qaCost: '~$10',
        prodTier: 'Standard',
        prodCost: '~$10',
        changeNotes: 'Topics/Subscriptions for SAP integration events',
    },
    {
        name: 'Storage Account',
        icon: HardDrive,
        color: 'indigo',
        pocTier: 'Standard_LRS',
        pocCost: '<$1',
        devTier: 'Standard_LRS',
        devCost: '<$1',
        qaTier: 'Standard_LRS',
        qaCost: '~$1',
        prodTier: 'Standard_GRS',
        prodCost: '~$3-5',
        changeNotes: 'Geo-redundancy for disaster recovery',
    },
    {
        name: 'Key Vault',
        icon: KeyRound,
        color: 'yellow',
        pocTier: 'Standard',
        pocCost: '<$1',
        devTier: 'Standard',
        devCost: '<$1',
        qaTier: 'Standard',
        qaCost: '<$1',
        prodTier: 'Standard + RBAC',
        prodCost: '<$1',
        changeNotes: 'RBAC authorization, purge protection, 90-day retention',
    },
    {
        name: 'Application Insights',
        icon: Activity,
        color: 'blue',
        pocTier: 'Pay-as-you-go',
        pocCost: '$0',
        devTier: 'Pay-as-you-go',
        devCost: '$0',
        qaTier: 'Pay-as-you-go',
        qaCost: '~$5-10',
        prodTier: 'Commitment Tier',
        prodCost: '~$20-50',
        changeNotes: 'Daily cap, alerting, availability tests, workbooks',
    },
    {
        name: 'Function App',
        icon: Zap,
        color: 'orange',
        pocTier: 'Not deployed',
        pocCost: '$0',
        devTier: 'Y1 Consumption',
        devCost: '$0',
        qaTier: 'Y1 Consumption',
        qaCost: '~$0-5',
        prodTier: 'EP1 Elastic Premium',
        prodCost: '~$220',
        changeNotes: 'Always warm, VNET integration. Y1 alternative: ~$0-5/mo',
    },
];

const ENVIRONMENTS: EnvironmentSummary[] = [
    {
        name: 'POC',
        label: 'Current POC',
        sapTarget: 'SAP D01 (Mock)',
        resourceGroup: 'rg-vendor-mdm-dev-v3',
        color: 'gray',
        monthlyCost: '$5-15',
        services: 9,
        purpose: 'Proof of Concept validation. Single environment, all free tiers. Mock SAP integration.',
        status: 'active',
    },
    {
        name: 'DEV',
        label: 'Development',
        sapTarget: 'SAP D01',
        resourceGroup: 'rg-vendor-mdm-dev-v3',
        color: 'blue',
        monthlyCost: '$10-15',
        services: 10,
        purpose: 'Active development and integration testing. Free/low tiers. Full SAP D01 connectivity with R/W/Create permissions.',
        status: 'active',
    },
    {
        name: 'QA',
        label: 'Quality Assurance',
        sapTarget: 'SAP Q01',
        resourceGroup: 'rg-vendor-mdm-qa',
        color: 'amber',
        monthlyCost: '$50-75',
        services: 10,
        purpose: 'Pre-production UAT and performance testing. Mid-tier resources. SAP Q01 with realistic data. Business user validation.',
        status: 'planned',
    },
    {
        name: 'PROD',
        label: 'Production',
        sapTarget: 'SAP P01 (ONLY)',
        resourceGroup: 'rg-vendor-mdm-prod',
        color: 'green',
        monthlyCost: '$170-450',
        services: 10,
        purpose: 'Live operations with SLA. Production-grade SKUs. ONLY connects to SAP P01. No environment switching.',
        status: 'future',
    },
];

const SCENARIO_COSTS = [
    { label: 'Current (POC)', dev: '$10-15', qa: '-', prod: '-', total: '$10-15', highlight: true },
    { label: 'Production Lite (Y1 Functions)', dev: '$10-15', qa: '$50-75', prod: '$170-230', total: '$230-320', highlight: false },
    { label: 'Production Standard (EP1)', dev: '$10-15', qa: '$50-75', prod: '$390-450', total: '$450-540', highlight: false },
    { label: 'Production HA (P1v3 + Multi-region)', dev: '$10-15', qa: '$80-120', prod: '$800-1,200', total: '$890-1,335', highlight: false },
];

const FREE_TIER_LIMITATIONS = [
    { limitation: 'No AlwaysOn', impact: '20-min idle cold starts (10-30s delays)', icon: XCircle, severity: 'critical' as const },
    { limitation: 'No SLA', impact: 'No uptime guarantee for business', icon: XCircle, severity: 'critical' as const },
    { limitation: 'No auto-scale', impact: 'Single instance, cannot handle traffic spikes', icon: XCircle, severity: 'critical' as const },
    { limitation: 'No staging slots', impact: 'Downtime during deployments', icon: AlertTriangle, severity: 'warning' as const },
    { limitation: 'No Topics (SB Basic)', impact: 'Cannot implement pub/sub for SAP events', icon: AlertTriangle, severity: 'warning' as const },
    { limitation: '5 DTU / 2GB (SQL)', impact: 'Concurrent queries will timeout', icon: AlertTriangle, severity: 'warning' as const },
    { limitation: 'Single region', impact: 'No disaster recovery, single point of failure', icon: AlertTriangle, severity: 'warning' as const },
    { limitation: 'No custom auth (SWA)', impact: 'Cannot enforce corporate SSO', icon: AlertTriangle, severity: 'warning' as const },
];

const ROADMAP_PHASES = [
    {
        phase: '1',
        title: 'Minimum Viable Production',
        cost: '$230-320/mo',
        color: 'blue',
        items: [
            'Upgrade App Service: DEV=F1, QA=B1, PROD=S1',
            'Service Bus to Standard (QA + PROD) for Topics',
            'Re-enable Azure AD authentication',
            'Key Vault RBAC + purge protection (PROD)',
            'Keep Cosmos DB Serverless (variable traffic)',
            'Deploy Function App on Consumption (Y1)',
        ],
    },
    {
        phase: '2',
        title: 'Production Hardened',
        cost: '$450-540/mo',
        color: 'purple',
        items: [
            'SWA to Standard (PROD) for SLA',
            'Function App to EP1 (PROD) for always-warm',
            'SQL to S1 / 20 DTU (PROD)',
            'Storage to GRS (PROD) for geo-redundancy',
            'App Insights alerting + availability tests',
        ],
    },
    {
        phase: '3',
        title: 'High Availability',
        cost: '$890-1,335/mo',
        color: 'red',
        items: [
            'App Service to P1v3 with auto-scale rules',
            'Cosmos DB multi-region replication',
            'SQL geo-replication',
            'Premium Service Bus tier',
            'Full VNET isolation + private endpoints',
        ],
    },
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
    gray: { bg: 'bg-gray-500', text: 'text-gray-700', border: 'border-gray-200', light: 'bg-gray-50' },
};

const statusBadge: Record<string, { label: string; classes: string }> = {
    active: { label: 'Active', classes: 'bg-green-100 text-green-800' },
    planned: { label: 'Planned', classes: 'bg-amber-100 text-amber-800' },
    future: { label: 'Future', classes: 'bg-gray-100 text-gray-700' },
};

// --- Sub-components ---

const EnvironmentCard: React.FC<{ env: EnvironmentSummary }> = ({ env }) => {
    const colors = colorMap[env.color];
    const badge = statusBadge[env.status];

    return (
        <div className={`rounded-lg border-2 ${colors.border} overflow-hidden hover:shadow-lg transition-shadow`}>
            <div className={`${colors.bg} px-4 py-3`}>
                <div className="flex items-center justify-between">
                    <h4 className="text-sm font-bold text-white">{env.name}</h4>
                    <span className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${badge.classes}`}>
                        {badge.label}
                    </span>
                </div>
                <p className="text-xs text-white/80 mt-0.5">{env.label}</p>
            </div>
            <div className={`${colors.light} px-4 py-4 space-y-3`}>
                <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-500">Monthly Cost</span>
                    <span className="text-lg font-bold text-gray-900">{env.monthlyCost}</span>
                </div>
                <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-500">SAP Target</span>
                    <span className="text-xs font-mono font-medium text-gray-700">{env.sapTarget}</span>
                </div>
                <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-500">Resource Group</span>
                    <span className="text-xs font-mono text-gray-600 truncate ml-2">{env.resourceGroup}</span>
                </div>
                <div className="flex items-center justify-between">
                    <span className="text-xs text-gray-500">Services</span>
                    <span className="text-sm font-medium text-gray-700">{env.services}</span>
                </div>
                <p className="text-xs text-gray-600 leading-relaxed pt-1 border-t border-gray-200">{env.purpose}</p>
            </div>
        </div>
    );
};

const SapLandscapeDiagram: React.FC = () => (
    <div className="overflow-x-auto">
        <pre className="text-xs leading-relaxed text-gray-700 font-mono whitespace-pre">
{`  Platform Environments          SAP Landscape          Relationship
  ══════════════════          ═════════════          ════════════

  ┌──────────────┐           ┌──────────────┐
  │     DEV      │──default──│   SAP D01    │  Development & Integration
  │  (Free/Low)  │           │ (Desarrollo) │  R/W/Create permissions
  │  F1, Basic   │──switch──→│   SAP Q01    │  Read/Write (ad-hoc testing)
  │              │──debug───→│   SAP P01    │  Read-only (debugging)
  └──────────────┘           └──────────────┘

  ┌──────────────┐           ┌──────────────┐
  │      QA      │──default──│   SAP Q01    │  Pre-production validation
  │  (Mid-tier)  │           │  (Quality)   │  UAT with realistic data
  │  B1, S0      │──fallback→│   SAP D01    │  Fallback if Q01 down
  └──────────────┘           └──────────────┘

  ┌──────────────┐           ┌──────────────┐
  │    PROD      │══ ONLY ══>│   SAP P01    │  Live operations
  │ (Prod-grade) │           │ (Producción) │  No switching. No exceptions.
  │  S1, S1, GRS │           │              │  Minimum-privilege credentials
  └──────────────┘           └──────────────┘

  Key: ── default connection   ─→ optional switch   ══ locked connection`}
        </pre>
    </div>
);

// --- Main Component ---

export const InfrastructureCosts: React.FC = () => {
    const [activeTab, setActiveTab] = useState<TabId>('overview');

    const tabs: { id: TabId; label: string; icon: React.ElementType }[] = [
        { id: 'overview', label: 'Cost Overview', icon: DollarSign },
        { id: 'environments', label: 'Environments', icon: Layers },
        { id: 'comparison', label: 'Service Comparison', icon: TrendingUp },
        { id: 'roadmap', label: 'Migration Roadmap', icon: ArrowRight },
    ];

    const pocTotal = '$5-15';
    const prodLiteTotal = '$230-320';
    const prodStdTotal = '$450-540';

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Infrastructure Costs</h1>
                <p className="mt-1 text-sm text-gray-500">
                    Azure cost comparison: POC (Free Tier) vs Production-ready deployment across DEV / QA / PROD environments.
                </p>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                <div className="rounded-lg border border-gray-200 bg-gray-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <DollarSign className="h-5 w-5 text-gray-600" />
                        <span className="text-xs font-medium text-gray-600 uppercase tracking-wider">POC (Now)</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-gray-900">{pocTotal}</p>
                    <p className="text-xs text-gray-500">per month</p>
                </div>
                <div className="rounded-lg border border-blue-200 bg-blue-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <TrendingUp className="h-5 w-5 text-blue-600" />
                        <span className="text-xs font-medium text-blue-700 uppercase tracking-wider">Prod Lite</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-blue-900">{prodLiteTotal}</p>
                    <p className="text-xs text-blue-600">3 environments</p>
                </div>
                <div className="rounded-lg border border-purple-200 bg-purple-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <Shield className="h-5 w-5 text-purple-600" />
                        <span className="text-xs font-medium text-purple-700 uppercase tracking-wider">Prod Standard</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-purple-900">{prodStdTotal}</p>
                    <p className="text-xs text-purple-600">3 environments</p>
                </div>
                <div className="rounded-lg border border-green-200 bg-green-50 p-4 hover:shadow-md transition-shadow">
                    <div className="flex items-center gap-2">
                        <Layers className="h-5 w-5 text-green-600" />
                        <span className="text-xs font-medium text-green-700 uppercase tracking-wider">Environments</span>
                    </div>
                    <p className="mt-2 text-2xl font-bold text-green-900">3</p>
                    <p className="text-xs text-green-600">DEV + QA + PROD</p>
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

            {/* ===================== TAB: OVERVIEW ===================== */}
            {activeTab === 'overview' && (
                <div className="space-y-6">
                    {/* Scenario Cost Table */}
                    <Card title="Monthly Cost by Scenario">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Scenario</th>
                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">DEV</th>
                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">QA</th>
                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">PROD</th>
                                        <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Total /mo</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {SCENARIO_COSTS.map((s, i) => (
                                        <tr key={i} className={s.highlight ? 'bg-green-50' : i % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                            <td className="px-4 py-3 text-sm font-medium text-gray-900">
                                                {s.highlight && <CheckCircle className="inline h-4 w-4 text-green-600 mr-1.5 -mt-0.5" />}
                                                {s.label}
                                            </td>
                                            <td className="px-4 py-3 text-sm text-right text-gray-600 font-mono">{s.dev}</td>
                                            <td className="px-4 py-3 text-sm text-right text-gray-600 font-mono">{s.qa}</td>
                                            <td className="px-4 py-3 text-sm text-right text-gray-600 font-mono">{s.prod}</td>
                                            <td className="px-4 py-3 text-sm text-right font-bold text-gray-900 font-mono">{s.total}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                        <p className="mt-3 text-xs text-gray-500">
                            Estimates based on US Central region. Use the{' '}
                            <a href="https://azure.microsoft.com/en-us/pricing/calculator/" target="_blank" rel="noopener noreferrer" className="text-brand-600 hover:underline inline-flex items-center gap-0.5">
                                Azure Pricing Calculator <ExternalLink className="h-3 w-3" />
                            </a>{' '}
                            for exact figures.
                        </p>
                    </Card>

                    {/* Free Tier Limitations */}
                    <Card title="Free Tier Limitations Blocking Production">
                        <div className="grid gap-3 sm:grid-cols-2">
                            {FREE_TIER_LIMITATIONS.map((item, i) => {
                                const Icon = item.icon;
                                const isCritical = item.severity === 'critical';
                                return (
                                    <div
                                        key={i}
                                        className={`flex items-start gap-3 rounded-lg border p-3 ${
                                            isCritical ? 'border-red-200 bg-red-50' : 'border-yellow-200 bg-yellow-50'
                                        }`}
                                    >
                                        <Icon className={`h-5 w-5 flex-shrink-0 mt-0.5 ${isCritical ? 'text-red-600' : 'text-yellow-600'}`} />
                                        <div>
                                            <p className="text-sm font-medium text-gray-900">{item.limitation}</p>
                                            <p className="text-xs text-gray-600">{item.impact}</p>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </Card>

                    {/* Cost Driver Insight */}
                    <Card>
                        <div className="flex items-start gap-4">
                            <div className="flex-shrink-0 rounded-lg bg-amber-100 p-3">
                                <Zap className="h-6 w-6 text-amber-700" />
                            </div>
                            <div>
                                <h4 className="text-sm font-semibold text-gray-900">Biggest Cost Driver: Function App (EP1)</h4>
                                <p className="mt-1 text-sm text-gray-600">
                                    The Elastic Premium plan (~$220/mo) is the largest line item. If cold starts are acceptable
                                    for email processing, stay on <span className="font-mono text-xs bg-gray-100 px-1 rounded">Y1 Consumption</span> ($0-5/mo)
                                    and reduce PROD cost by ~$215/month. Consider upgrading to EP1 only when email SLA is critical.
                                </p>
                            </div>
                        </div>
                    </Card>
                </div>
            )}

            {/* ===================== TAB: ENVIRONMENTS ===================== */}
            {activeTab === 'environments' && (
                <div className="space-y-6">
                    {/* Environment Cards */}
                    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                        {ENVIRONMENTS.map((env) => (
                            <EnvironmentCard key={env.name} env={env} />
                        ))}
                    </div>

                    {/* SAP Landscape Diagram */}
                    <Card title="SAP Landscape Mirroring Strategy">
                        <div className="mb-4">
                            <p className="text-sm text-gray-600">
                                Platform environments are <strong>NOT</strong> 1:1 with SAP landscapes.
                                Each environment uses a <em>flexible, configuration-based</em> connection model.
                                PROD is the only exception: it connects <strong>exclusively</strong> to SAP P01.
                            </p>
                        </div>
                        <SapLandscapeDiagram />
                    </Card>

                    {/* Environment Principles */}
                    <Card title="Key Principles">
                        <div className="space-y-4">
                            {[
                                {
                                    title: 'No 1:1 Automatic Mapping',
                                    detail: 'Platform DEV can connect to D01, Q01, or P01 (read-only). The relationship is configurable, not hardcoded.',
                                    icon: Shield,
                                    color: 'blue',
                                },
                                {
                                    title: 'PROD Locks to SAP P01',
                                    detail: 'Production has NO environment switch capability. Only P01 credentials exist in the PROD Key Vault. No fallbacks.',
                                    icon: KeyRound,
                                    color: 'red',
                                },
                                {
                                    title: 'Isolated Credentials Per Environment',
                                    detail: 'Each platform environment uses separate SAP users (PORTALDEV, PORTALQA, PORTALPRD) with different permission levels.',
                                    icon: Shield,
                                    color: 'green',
                                },
                                {
                                    title: 'Independent Azure Deployments',
                                    detail: 'Each environment is a full, independent set of Azure resources in its own resource group. No shared databases or queues.',
                                    icon: Cloud,
                                    color: 'purple',
                                },
                            ].map((principle, i) => {
                                const Icon = principle.icon;
                                const colors = colorMap[principle.color];
                                return (
                                    <div key={i} className="flex items-start gap-3">
                                        <div className={`flex-shrink-0 rounded-lg ${colors.bg} p-2`}>
                                            <Icon className="h-4 w-4 text-white" />
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-semibold text-gray-900">{principle.title}</h4>
                                            <p className="text-xs text-gray-600 mt-0.5">{principle.detail}</p>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </Card>
                </div>
            )}

            {/* ===================== TAB: COMPARISON ===================== */}
            {activeTab === 'comparison' && (
                <div className="space-y-6">
                    <Card title="Service-by-Service: POC vs Production Tiers">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Service</th>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            <span className="inline-flex items-center gap-1">
                                                POC<span className="text-gray-400 normal-case font-normal">(current)</span>
                                            </span>
                                        </th>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-blue-600 uppercase tracking-wider">DEV</th>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-amber-600 uppercase tracking-wider">QA</th>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-green-600 uppercase tracking-wider">PROD</th>
                                        <th className="px-3 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Key Changes</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {SERVICE_COSTS.map((service, i) => {
                                        const Icon = service.icon;
                                        const colors = colorMap[service.color];
                                        return (
                                            <tr key={i} className={i % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                                <td className="px-3 py-3 text-sm whitespace-nowrap">
                                                    <div className="flex items-center gap-2">
                                                        <div className={`flex-shrink-0 rounded ${colors.bg} p-1`}>
                                                            <Icon className="h-3.5 w-3.5 text-white" />
                                                        </div>
                                                        <span className="font-medium text-gray-900">{service.name}</span>
                                                    </div>
                                                </td>
                                                <td className="px-3 py-3">
                                                    <div className="text-xs text-gray-600">{service.pocTier}</div>
                                                    <div className="text-xs font-mono font-medium text-gray-900">{service.pocCost}</div>
                                                </td>
                                                <td className="px-3 py-3">
                                                    <div className="text-xs text-blue-600">{service.devTier}</div>
                                                    <div className="text-xs font-mono font-medium text-gray-900">{service.devCost}</div>
                                                </td>
                                                <td className="px-3 py-3">
                                                    <div className="text-xs text-amber-600">{service.qaTier}</div>
                                                    <div className="text-xs font-mono font-medium text-gray-900">{service.qaCost}</div>
                                                </td>
                                                <td className="px-3 py-3">
                                                    <div className="text-xs text-green-600">{service.prodTier}</div>
                                                    <div className="text-xs font-mono font-medium text-gray-900">{service.prodCost}</div>
                                                </td>
                                                <td className="px-3 py-3 text-xs text-gray-500 max-w-[200px]">{service.changeNotes}</td>
                                            </tr>
                                        );
                                    })}
                                </tbody>
                                <tfoot className="bg-gray-100">
                                    <tr>
                                        <td className="px-3 py-3 text-sm font-bold text-gray-900">Total</td>
                                        <td className="px-3 py-3 text-sm font-bold font-mono text-gray-900">~$5-15</td>
                                        <td className="px-3 py-3 text-sm font-bold font-mono text-blue-700">~$10-15</td>
                                        <td className="px-3 py-3 text-sm font-bold font-mono text-amber-700">~$50-75</td>
                                        <td className="px-3 py-3 text-sm font-bold font-mono text-green-700">~$170-450</td>
                                        <td className="px-3 py-3 text-xs text-gray-500">Range depends on Function App tier</td>
                                    </tr>
                                </tfoot>
                            </table>
                        </div>
                    </Card>

                    {/* Security Upgrades */}
                    <Card title="Security Upgrades Required for Production">
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Area</th>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">POC (Current)</th>
                                        <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Production (Required)</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {[
                                        { area: 'Authentication', poc: 'Disabled (testing)', prod: 'Azure AD enforced' },
                                        { area: 'Key Vault', poc: 'Access Policies, no purge protection', prod: 'RBAC, purge protection, 90-day retention' },
                                        { area: 'Network', poc: 'Public endpoints', prod: 'Private endpoints + VNET integration' },
                                        { area: 'Storage', poc: 'Public blob access disabled', prod: '+ Private endpoints' },
                                        { area: 'SQL Database', poc: 'Public network access', prod: '+ Firewall rules, VNET service endpoints' },
                                        { area: 'Service Bus', poc: 'Basic (no Topics)', prod: 'Standard (Topics for SAP events)' },
                                    ].map((row, i) => (
                                        <tr key={i} className={i % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                            <td className="px-4 py-3 text-sm font-medium text-gray-900">{row.area}</td>
                                            <td className="px-4 py-3 text-sm text-gray-600">
                                                <span className="inline-flex items-center gap-1">
                                                    <XCircle className="h-3.5 w-3.5 text-red-400" />
                                                    {row.poc}
                                                </span>
                                            </td>
                                            <td className="px-4 py-3 text-sm text-gray-600">
                                                <span className="inline-flex items-center gap-1">
                                                    <CheckCircle className="h-3.5 w-3.5 text-green-500" />
                                                    {row.prod}
                                                </span>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </Card>
                </div>
            )}

            {/* ===================== TAB: ROADMAP ===================== */}
            {activeTab === 'roadmap' && (
                <div className="space-y-6">
                    {ROADMAP_PHASES.map((phase) => {
                        const colors = colorMap[phase.color];
                        return (
                            <Card key={phase.phase}>
                                <div className="flex items-start gap-4">
                                    <div className={`flex-shrink-0 flex h-12 w-12 items-center justify-center rounded-full ${colors.bg} text-white text-lg font-bold`}>
                                        {phase.phase}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center gap-3">
                                            <h3 className="text-lg font-semibold text-gray-900">{phase.title}</h3>
                                            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${colors.light} ${colors.text} border ${colors.border}`}>
                                                {phase.cost}
                                            </span>
                                        </div>
                                        <ul className="mt-3 space-y-2">
                                            {phase.items.map((item, i) => (
                                                <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                                                    <ArrowRight className={`h-4 w-4 flex-shrink-0 mt-0.5 ${colors.text}`} />
                                                    {item}
                                                </li>
                                            ))}
                                        </ul>
                                    </div>
                                </div>
                            </Card>
                        );
                    })}

                    {/* Recommendation */}
                    <Card>
                        <div className="rounded-lg border-2 border-blue-300 bg-blue-50 p-5">
                            <div className="flex items-start gap-3">
                                <CheckCircle className="h-6 w-6 text-blue-600 flex-shrink-0 mt-0.5" />
                                <div>
                                    <h4 className="text-sm font-bold text-blue-900">Recommended First Step: Phase 1 (Production Lite)</h4>
                                    <p className="mt-1 text-sm text-blue-800">
                                        For an estimated <strong>$230-320/month</strong> across 3 environments, you get SLA-backed services,
                                        AlwaysOn for the API, Topics support for SAP integration events, and Azure AD authentication.
                                        This is the minimum viable production configuration that supports the full SAP landscape
                                        (D01 / Q01 / P01) mirroring strategy.
                                    </p>
                                    <div className="mt-3 flex flex-wrap gap-2">
                                        <a
                                            href="https://azure.microsoft.com/en-us/pricing/calculator/"
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="inline-flex items-center gap-1 rounded-md bg-blue-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-blue-700 transition-colors"
                                        >
                                            <ExternalLink className="h-3 w-3" />
                                            Azure Pricing Calculator
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </Card>
                </div>
            )}
        </div>
    );
};
