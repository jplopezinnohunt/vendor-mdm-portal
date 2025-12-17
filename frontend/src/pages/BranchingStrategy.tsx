/**
 * CODE VERSION: v2.17 (React Pure)
 * NAME: SAP + Azure Landscape Standard (Actors & Roles)
 * DATE: December 17, 2025
 */

import React, { useState, useEffect } from 'react';
import {
    GitBranch,
    GitCommit,
    GitMerge,
    Shield,
    Server,
    Terminal,
    AlertTriangle,
    CheckCircle,
    Clock,
    BookOpen,
    ChevronDown,
    ChevronUp,
    Zap,
    Layout,
    Globe,
    Award,
    Database,
    Cloud,
    Flame,
    Snowflake,
    FileText,
    Code,
    Laptop,
    TestTube,
    Truck,
    Rocket,
    Users,
    AlertOctagon, // For Hotfix
    RefreshCw,    // For Sync/Cascade
    History,      // New: For History
    UserCog       // New: For Tech Lead
} from 'lucide-react';

// --- DATA & CONTENT ---

const strategyData = {
    intro: {
        title: "Azure & SAP Alignment",
        subtitle: "Official Standard + Full Landscape v2.17",
        description: "Comprehensive branching strategy. Visualizes the full deployment flow: from code in Git, through Azure environments (App Service/Functions), to data connection with the SAP landscape.",
        principles: [
            "Azure PROD + SAP P01 always synchronized",
            "Azure DEV consumes data from SAP D01",
            "Staging (QA) validates against SAP Q01",
            "Full Cycle: Code -> Deploy Azure -> Connect SAP",
            "Hotfixes with fast-track to production"
        ]
    },
    // --- VERSION HISTORY ---
    versions: [
        { v: "v2.7", date: "17-Dec", desc: "Base: Azure Top + Git Mid + SAP Bottom" },
        { v: "v2.8", date: "17-Dec", desc: "Storytelling: Release and Cascade Concepts" },
        { v: "v2.15", date: "17-Dec", desc: "Feature Journey: Visual Timeline" },
        { v: "v2.16", date: "17-Dec", desc: "Dual Tabs: Feature vs Hotfix Separation" },
        { v: "v2.17", date: "17-Dec", desc: "Actors Section: Roles and Responsibilities Definition" }
    ],
    // --- ACTORS DATA ---
    actors: [
        {
            id: "dev",
            role: "Developer",
            icon: <Code className="w-6 h-6 text-purple-600" />,
            color: "bg-purple-50 border-purple-100",
            desc: "Responsible for technical implementation. Creates `feature/*` branches, writes code, runs local unit tests, and resolves merge conflicts before requesting review."
        },
        {
            id: "lead",
            role: "Tech Lead / Peer",
            icon: <UserCog className="w-6 h-6 text-blue-600" />,
            color: "bg-blue-50 border-blue-100",
            desc: "Quality guardian. Performs Code Reviews, approves Pull Requests (PRs) to `develop`, and ensures architecture and security standards are met."
        },
        {
            id: "release",
            role: "Release Manager",
            icon: <Truck className="w-6 h-6 text-yellow-600" />,
            color: "bg-yellow-50 border-yellow-100",
            desc: "Schedule manager. Decides when a version (`release/*`) is cut, freezes code for QA, and coordinates deployments to non-production environments."
        },
        {
            id: "user",
            role: "Key User",
            icon: <Users className="w-6 h-6 text-orange-600" />,
            color: "bg-orange-50 border-orange-100",
            desc: "Business validator. Executes User Acceptance Tests (UAT) in the Staging environment (connected to SAP Q01) and gives the 'Go/No-Go' for production."
        },
        {
            id: "devops",
            role: "DevOps Engineer",
            icon: <Rocket className="w-6 h-6 text-green-600" />,
            color: "bg-green-50 border-green-100",
            desc: "Infrastructure architect. Manages CI/CD pipelines, monitors Azure and SAP health, and executes final production releases."
        }
    ],
    branches: [
        {
            id: 'main',
            name: 'main',
            type: 'production',
            color: 'bg-green-100 text-green-800 border-green-200',
            iconColor: 'text-green-600',
            description: 'Production code. Must be 100% compatible with current SAP P01 configuration.',
            environment: 'PRODUCTION (Azure)',
            sap_env: 'SAP P01',
            protection: ['No direct commits', 'PR required (1 approval)', 'CI/CD 100% passing', 'Only admins can force-push'],
            lifecycle: 'Infinite. Receives merges from release/* and hotfix/*.',
            tags: ['v1.0.0', 'v1.1.0']
        },
        {
            id: 'develop',
            name: 'develop',
            type: 'integration',
            color: 'bg-blue-100 text-blue-800 border-blue-200',
            iconColor: 'text-blue-600',
            description: 'Continuous integration. Place to test integrations with BAPIs/IDOCs in development.',
            environment: 'DEV (Azure)',
            sap_env: 'SAP D01',
            protection: ['No direct commits', 'PR required from feature/*', 'Auto-deploy to DEV'],
            lifecycle: 'Infinite. Base for new features and releases.'
        },
        {
            id: 'feature',
            name: 'feature/*',
            type: 'development',
            color: 'bg-purple-100 text-purple-800 border-purple-200',
            iconColor: 'text-purple-600',
            description: 'Feature development. Can use SAP Mocks if D01 is unstable.',
            environment: 'Local / DEV',
            sap_env: 'SAP D01 / Mocks',
            protection: ['Ephemeral', 'Naming: feature/TASK-ID-description'],
            lifecycle: 'Born from develop -> PR -> Merge to develop -> Delete.'
        },
        {
            id: 'release',
            name: 'release/*',
            type: 'staging',
            color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
            iconColor: 'text-yellow-600',
            description: 'User Acceptance Testing (UAT). Validates custom logic works with real Q01 data.',
            environment: 'STAGING (Azure)',
            sap_env: 'SAP Q01',
            protection: ['Only bugfixes allowed', 'Naming: release/v1.x.x'],
            lifecycle: 'Born from develop -> QA in Staging -> Merge to main & develop.'
        },
        {
            id: 'hotfix',
            name: 'hotfix/*',
            type: 'emergency',
            color: 'bg-red-100 text-red-800 border-red-200',
            iconColor: 'text-red-600',
            description: 'Urgent fix. Requires rapid validation of non-regression in SAP P01.',
            environment: 'PRODUCTION (Azure)',
            sap_env: 'SAP P01',
            protection: ['High priority', 'Naming: hotfix/issue-description'],
            lifecycle: 'Born from main -> Fix -> Merge to main & develop.'
        }
    ],
    // --- STORYBOARD DATA ---
    stories: {
        feature: [
            {
                step: 1,
                title: "The Requirement",
                role: "Product Owner",
                icon: <FileText className="w-5 h-5 text-slate-600" />,
                color: "bg-slate-100 border-slate-200",
                desc: "A user story arrives: 'Create endpoint to query vendors by region'. Validates with SAP team if the BAPI exists."
            },
            {
                step: 2,
                title: "Branch Creation",
                role: "Developer",
                icon: <GitBranch className="w-5 h-5 text-purple-600" />,
                color: "bg-purple-50 border-purple-200",
                desc: "Dev creates `feature/VEN-101-get-regions` off `develop`. This is their safe workspace."
            },
            {
                step: 3,
                title: "Development & Local Test",
                role: "Developer",
                icon: <Laptop className="w-5 h-5 text-indigo-600" />,
                color: "bg-indigo-50 border-indigo-200",
                desc: "Codes the solution. Uses local mocks if SAP D01 is slow. Runs unit tests (`npm test`)."
            },
            {
                step: 4,
                title: "Merge to Develop",
                role: "Tech Lead / Peer",
                icon: <GitMerge className="w-5 h-5 text-blue-600" />,
                color: "bg-blue-50 border-blue-200",
                desc: "Pull Request (PR) approved. Merged into `develop`. CI/CD automatically deploys to **Azure DEV**."
            },
            {
                step: 5,
                title: "Validation in DEV",
                role: "Developer / QA",
                icon: <TestTube className="w-5 h-5 text-teal-600" />,
                color: "bg-teal-50 border-teal-200",
                desc: "Real integration tested against **SAP D01**. If everything works, feature is ready for next release."
            },
            {
                step: 6,
                title: "Release Cut",
                role: "Release Manager",
                icon: <Truck className="w-5 h-5 text-yellow-600" />,
                color: "bg-yellow-50 border-yellow-200",
                desc: "Branch `release/v1.2.0` cut from develop. Code frozen. Deployed to **Azure Staging** (connected to **SAP Q01**)."
            },
            {
                step: 7,
                title: "UAT (Acceptance)",
                role: "Key Users",
                icon: <Users className="w-5 h-5 text-orange-600" />,
                color: "bg-orange-50 border-orange-200",
                desc: "Business users test in Staging. Confirm SAP Q01 data looks correct."
            },
            {
                step: 8,
                title: "Go Live (Production)",
                role: "DevOps",
                icon: <Rocket className="w-5 h-5 text-green-600" />,
                color: "bg-green-50 border-green-200",
                desc: "Merge release to `main`. Deploy to **Azure PROD**. SAP team transports changes to **SAP P01** simultaneously."
            }
        ],
        hotfix: [
            {
                step: 1,
                title: "Critical Incident",
                role: "Support / Ops",
                icon: <AlertOctagon className="w-5 h-5 text-red-600" />,
                color: "bg-red-50 border-red-200",
                desc: "Alert! Blocking bug in Production prevents creating purchase orders. Immediate action required."
            },
            {
                step: "1. Create Hotfix",
                cmd: "git checkout -b hotfix/VEN-404-urgent-fix main",
                desc: "Branch directly from 'main'. Use 'VEN' prefix for traceability."
            },
            {
                step: 2,
                title: "Hotfix Branch",
                role: "Tech Lead",
                icon: <GitBranch className="w-5 h-5 text-red-500" />,
                color: "bg-red-50 border-red-200",
                desc: "Branch `hotfix/v1.2.1-fix-orders` created directly from `main` (current stable version). NOT from develop."
            },
            {
                step: 3,
                title: "Rapid Fix",
                role: "Senior Dev",
                icon: <Code className="w-5 h-5 text-red-400" />,
                color: "bg-white border-red-100",
                desc: "Minimal necessary solution coded. Avoids refactoring unrelated code to minimize risk."
            },
            {
                step: 4,
                title: "Deploy to Production",
                role: "DevOps",
                icon: <Rocket className="w-5 h-5 text-green-600" />,
                color: "bg-green-50 border-green-200",
                desc: "Merge to `main`. Tag v1.2.1. Immediate deploy to Azure PROD. System operational again."
            },
            {
                step: 5,
                title: "The CASCADE (Sync)",
                role: "Automated / Lead",
                icon: <RefreshCw className="w-5 h-5 text-pink-600" />,
                color: "bg-pink-50 border-pink-200",
                desc: "CRITICAL: Hotfix merged downwards: to `develop` and active `release` branches. Prevents regression in future."
            }
        ]
    },
    workflows: [
        {
            title: "Feature Development",
            steps: [
                {
                    title: "Create Branch",
                    cmd: "git checkout -b feature/VEN-123-functionality develop",
                    desc: "Every feature starts from 'develop'. Use the 'VEN' prefix for Vendor Portal tasks."
                },
                { title: "Develop", cmd: "git commit -m 'feat(sap): consume BAPI_VENDOR_GET'", desc: "Implement logic against SAP D01." },
                { title: "Pull Request", cmd: "GH UI: Compare feature -> develop", desc: "Validate it breaks no integrations." },
                { title: "Merge", cmd: "Squash & Merge", desc: "Auto deploy to Azure DEV." }
            ]
        },
        {
            title: "Release Process",
            steps: [
                { title: "Sync", cmd: "Check SAP Transports", desc: "Confirm SAP transports are in Q01." },
                { title: "Freeze", cmd: "git checkout -b release/v1.2.0 develop", desc: "Create release branch." },
                { title: "Deploy Staging", cmd: "Manual Deploy -> Azure Staging", desc: "App points to SAP Q01." },
                { title: "UAT", cmd: "User Acceptance Test", desc: "Users validate integration in Q01." },
                { title: "Release PROD", cmd: "Merge release -> main", desc: "Deploy to PROD same time SAP moves to P01." }
            ]
        },
        {
            title: "Emergency Hotfix",
            steps: [
                { title: "Start", cmd: "git checkout -b hotfix/sap-error main", desc: "Create from main." },
                { title: "Fix", cmd: "git commit -m 'fix: adjust payload for SAP'", desc: "Point fix (e.g., new mandatory field)." },
                { title: "Deploy", cmd: "Merge hotfix -> main", desc: "Urgent deploy to Azure PROD." },
                { title: "Sync", cmd: "Merge hotfix -> develop", desc: "Replicate fix to development." }
            ]
        }
    ],
    commits: [
        { type: 'feat', desc: 'New feature (Minor)', release: 'v1.X.0' },
        { type: 'fix', desc: 'Bug fix (Patch)', release: 'v1.0.X' },
        { type: 'docs', desc: 'Documentation', release: '-' },
        { type: 'refactor', desc: 'Code change without logic change', release: '-' },
        { type: 'test', desc: 'Add or correct tests', release: '-' },
        { type: 'chore', desc: 'Maintenance, dependencies', release: '-' },
    ],
    envs: [
        { branch: 'develop', env: 'DEV', sap: 'SAP D01', rg: 'rg-vendor-mdm-dev', trigger: 'Automatic' },
        { branch: 'release/*', env: 'STAGING', sap: 'SAP Q01', rg: 'rg-vendor-mdm-staging', trigger: 'Manual (QA)' },
        { branch: 'main', env: 'PRODUCTION', sap: 'SAP P01', rg: 'rg-vendor-mdm-prod', trigger: 'Manual (Lead)' },
        { branch: 'hotfix/*', env: 'PRODUCTION', sap: 'SAP P01', rg: 'rg-vendor-mdm-prod', trigger: 'Manual (Urgent)' },
    ],
    faq: [
        { q: "What if SAP D01 is down?", a: "Recommend using local Mocks in 'feature' branch to not stop development, but real integration is validated in 'develop'." },
        { q: "How to coordinate a release with SAP?", a: "'release branch' must not go to PROD until SAP team confirms transports are ready for P01." },
        { q: "Can I point develop to SAP Q01?", a: "Not recommended. 'develop' is unstable and could pollute Q01 data used for formal testing." }
    ]
};

// --- COMPONENTS ---

const Header = () => {
    return (
        <header className="bg-slate-900 text-white sticky top-0 z-50 shadow-lg border-b border-slate-700">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
                <div className="flex items-center space-x-3">
                    <GitBranch className="h-8 w-8 text-blue-400" />
                    <div>
                        <h1 className="text-xl font-bold tracking-tight">Vendor MDM Platform</h1>
                        <p className="text-xs text-green-400 font-mono font-bold tracking-wide flex items-center">
                            <Award className="w-3 h-3 mr-1" />
                            STANDARD WITH SAP LANDSCAPE
                        </p>
                    </div>
                </div>
                <nav className="hidden md:flex items-center space-x-6 text-sm font-medium text-slate-300">
                    <a href="#visual" className="hover:text-white transition-colors">Visualization</a>
                    <a href="#storyboard" className="hover:text-white transition-colors text-yellow-300">Feature Journey</a>
                    <a href="#actors" className="hover:text-white transition-colors">Roles</a>
                    <a href="#branches" className="hover:text-white transition-colors">Branches</a>
                </nav>
            </div>
        </header>
    );
};

const Hero = () => (
    <div className="bg-slate-900 text-white py-16 px-4 border-b border-slate-800">
        <div className="max-w-4xl mx-auto text-center">
            <div className="inline-flex items-center px-4 py-1.5 rounded-full bg-blue-900/50 border border-blue-700 text-blue-200 text-sm font-medium mb-6">
                <Award className="w-4 h-4 mr-2" />
                Approved Standard - 2025
            </div>
            <h2 className="text-4xl md:text-5xl font-extrabold mb-4 text-white drop-shadow-lg tracking-tight">
                {strategyData.intro.title}
            </h2>
            <p className="text-xl text-blue-200 font-medium mb-6">
                {strategyData.intro.subtitle}
            </p>
            <p className="text-lg md:text-xl text-slate-300 mb-8 max-w-2xl mx-auto">
                {strategyData.intro.description}
            </p>
            <div className="flex flex-wrap justify-center gap-4">
                {strategyData.intro.principles.map((p, i) => (
                    <span key={i} className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-slate-800 text-blue-300 border border-slate-700">
                        <CheckCircle className="w-4 h-4 mr-2 text-green-500" />
                        {p}
                    </span>
                ))}
            </div>
        </div>
    </div>
);

// --- COMPONENT: FEATURE STORYBOARD (WITH TABS) ---
const FeatureStoryBoard = () => {
    const [activeTab, setActiveTab] = useState<'feature' | 'hotfix'>('feature');

    const currentStory = strategyData.stories[activeTab];

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
            <div className="p-8 border-b border-slate-100 bg-slate-50/50">
                <div className="text-center max-w-3xl mx-auto mb-8">
                    <h3 className="text-2xl font-bold text-slate-800 mb-2">End-to-End Story</h3>
                    <p className="text-slate-500 mb-6">
                        Choose a scenario to see how code travels through our environments.
                    </p>

                    {/* NAMING CONVENTION ALERT */}
                    <div className="inline-flex items-center gap-2 px-4 py-2 bg-slate-100 border border-slate-300 rounded-lg text-sm text-slate-700 font-mono mb-6">
                        <Terminal className="w-4 h-4 text-slate-500" />
                        <span>Naming Convention:</span>
                        <span className="font-bold text-blue-600">type/VEN-ID-description</span>
                        <span className="text-xs text-slate-400 ml-2">(e.g., feature/VEN-200-add-button)</span>
                    </div>
                </div>

                {/* SELECTION TABS */}
                <div className="flex justify-center space-x-4">
                    <button
                        onClick={() => setActiveTab('feature')}
                        className={`px-6 py-3 rounded-full font-bold text-sm transition-all flex items-center shadow-sm
              ${activeTab === 'feature'
                                ? 'bg-blue-600 text-white shadow-blue-200'
                                : 'bg-white text-slate-500 border border-slate-200 hover:bg-slate-50'}`}
                    >
                        <GitBranch className="w-4 h-4 mr-2" />
                        Feature Journey
                    </button>
                    <button
                        onClick={() => setActiveTab('hotfix')}
                        className={`px-6 py-3 rounded-full font-bold text-sm transition-all flex items-center shadow-sm
              ${activeTab === 'hotfix'
                                ? 'bg-red-600 text-white shadow-red-200'
                                : 'bg-white text-slate-500 border border-slate-200 hover:bg-slate-50'}`}
                    >
                        <Flame className="w-4 h-4 mr-2" />
                        Hotfix Journey
                    </button>
                </div>
            </div>

            <div className="p-8 relative min-h-[500px]">
                {/* Central connecting line */}
                <div className={`absolute left-8 top-8 bottom-8 w-0.5 md:left-1/2 md:-ml-0.5 transition-colors duration-500
            ${activeTab === 'hotfix' ? 'bg-red-100' : 'bg-slate-100'}
        `}></div>

                <div className="space-y-8">
                    {currentStory.map((step, index) => {
                        const isEven = index % 2 === 0;
                        return (
                            <div key={index} className={`relative flex items-center md:justify-between ${isEven ? 'md:flex-row' : 'md:flex-row-reverse'} animate-fade-in`}>

                                {/* Content (Card) */}
                                <div className="ml-16 md:ml-0 md:w-[45%]">
                                    <div className={`p-5 rounded-xl border shadow-sm hover:shadow-md transition-all ${step.color}`}>
                                        <div className="flex justify-between items-start mb-2">
                                            <h4 className="font-bold text-slate-800 text-lg">{step.title}</h4>
                                            <span className="text-xs font-bold uppercase tracking-wider text-slate-500 bg-white/50 px-2 py-1 rounded">
                                                {step.role}
                                            </span>
                                        </div>
                                        <p className="text-slate-600 text-sm leading-relaxed">
                                            {step.desc}
                                        </p>
                                    </div>
                                </div>

                                {/* Central Bubble */}
                                <div className={`absolute left-0 md:left-1/2 md:-ml-6 w-12 h-12 rounded-full border-4 shadow-sm flex items-center justify-center z-10 transition-colors
                    ${activeTab === 'hotfix' ? 'bg-red-50 border-red-100' : 'bg-white border-slate-100'}
                `}>
                                    {step.icon}
                                </div>

                                {/* Empty space to balance layout on desktop */}
                                <div className="hidden md:block md:w-[45%]"></div>
                            </div>
                        );
                    })}
                </div>

                {/* Final Label */}
                <div className="flex justify-center mt-12 relative z-10">
                    <div className={`px-6 py-2 rounded-full font-bold text-sm border shadow-sm flex items-center
            ${activeTab === 'hotfix'
                            ? 'bg-red-100 text-red-800 border-red-200'
                            : 'bg-green-100 text-green-800 border-green-200'}
          `}>
                        <CheckCircle className="w-4 h-4 mr-2" />
                        {activeTab === 'hotfix' ? 'Service Restored' : 'Value Delivered to User'}
                    </div>
                </div>
            </div>
        </div>
    );
};

// --- COMPONENT: ACTORS SECTION ---
const ActorsSection = () => {
    return (
        <div className="bg-slate-50 rounded-xl p-8 border border-slate-200">
            <div className="text-center mb-8">
                <h3 className="text-2xl font-bold text-slate-800">Process Roles & Actors</h3>
                <p className="text-slate-500">Who is responsible for what in the software lifecycle?</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {strategyData.actors.map((actor) => (
                    <div key={actor.id} className={`bg-white p-6 rounded-lg border shadow-sm hover:shadow-md transition-all ${actor.color.replace('bg-', 'hover:bg-').split(' ')[0]}-50`}>
                        <div className="flex items-center gap-3 mb-3">
                            <div className={`p-2 rounded-lg ${actor.color} bg-opacity-30`}>
                                {actor.icon}
                            </div>
                            <h4 className="font-bold text-slate-800">{actor.role}</h4>
                        </div>
                        <p className="text-sm text-slate-600 leading-relaxed">
                            {actor.desc}
                        </p>
                    </div>
                ))}
            </div>
        </div>
    );
};

// --- INTERACTIVE VISUALIZER COMPONENT (v2.15) ---
const GitGraphVisualizer = () => {
    const [activeTooltip, setActiveTooltip] = useState<string | null>(null);

    const Node = ({ cx, cy, color, type, label, onClick }: { cx: number, cy: number, color: string, type: string, label: string, onClick?: (l: string) => void }) => (
        <g
            className="cursor-pointer group"
            onClick={() => onClick && onClick(label)}
            onMouseEnter={() => setActiveTooltip(label)}
            onMouseLeave={() => setActiveTooltip(null)}
        >
            <circle cx={cx} cy={cy} r="6" className={`${color} stroke-white stroke-2 transition-all duration-300 group-hover:r-8`} />
            <text x={cx} y={cy + 20} textAnchor="middle" className="text-[10px] fill-slate-500 font-mono opacity-0 group-hover:opacity-100 transition-opacity">
                {type}
            </text>
        </g>
    );

    // Environment Box Component
    const EnvBox = ({ x, y, width, label, color, alignNodeX, type }: { x: number, y: number, width: number, label: string, color: string, alignNodeX?: number, type?: 'top' | 'bottom' }) => (
        <g>
            <rect x={x} y={y} width={width} height="30" rx="6" className="fill-white stroke-slate-200 stroke-1" />
            <rect x={x} y={y} width={width} height="30" rx="6" className={`fill-${color}-50 bg-opacity-30`} />
            <text x={x + width / 2} y={y + 20} textAnchor="middle" className={`text-xs font-bold ${color === 'blue' ? 'fill-blue-600' : color === 'yellow' ? 'fill-yellow-600' : 'fill-green-600'}`}>
                {label}
            </text>
            {alignNodeX && (
                type === 'top' ? (
                    <path d={`M${alignNodeX},${y + 30} L${alignNodeX},${y + 40}`} className="stroke-slate-300 stroke-1" />
                ) : (
                    <path d={`M${alignNodeX},${y} L${alignNodeX},${y - 10}`} className="stroke-slate-300 stroke-1" />
                )
            )}
        </g>
    );

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6 overflow-hidden">
            <div className="flex justify-between items-center mb-4">
                <h3 className="text-lg font-bold text-slate-800 flex items-center">
                    <Layout className="w-5 h-5 mr-2 text-blue-600" />
                    Full Landscape View
                </h3>
                <div className="flex space-x-3 text-xs items-center">
                    <span className="flex items-center text-slate-500 mr-2"><Cloud className="w-3 h-3 mr-1" /> Azure</span>
                    <span className="flex items-center text-slate-500 mr-2"><GitBranch className="w-3 h-3 mr-1" /> Git</span>
                    <span className="flex items-center text-slate-500"><Database className="w-3 h-3 mr-1" /> SAP</span>
                </div>
            </div>

            <div className="relative overflow-x-auto">
                <svg viewBox="0 0 800 380" className="w-full min-w-[600px] h-auto">

                    {/* --- DEFS (Arrows) --- */}
                    <defs>
                        <marker id="arrow-cascade" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
                            <path d="M0,0 L0,6 L9,3 z" fill="#ec4899" />
                        </marker>
                    </defs>

                    {/* --- AZURE LAYER (TOP) --- */}
                    <text x="50" y="25" className="text-xs font-bold fill-slate-400 font-mono">AZURE CLOUD:</text>

                    <EnvBox x={150} y={10} width={220} label="Azure DEV (App Service)" color="blue" alignNodeX={260} type="top" />
                    <EnvBox x={480} y={10} width={140} label="Azure STAGING" color="yellow" alignNodeX={550} type="top" />
                    <EnvBox x={640} y={10} width={140} label="Azure PROD" color="green" alignNodeX={710} type="top" />

                    {/* Connectors Azure -> Git */}
                    <line x1="260" y1="40" x2="260" y2="140" className="stroke-blue-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />
                    <line x1="550" y1="40" x2="550" y2="220" className="stroke-yellow-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />
                    <line x1="710" y1="40" x2="710" y2="220" className="stroke-green-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />


                    {/* --- GIT GRAPH (MIDDLE) --- */}

                    {/* Main Line */}
                    <line x1="50" y1="220" x2="750" y2="220" className="stroke-green-500 stroke-2" />
                    <text x="20" y="225" className="text-xs font-bold fill-green-700">Main</text>

                    {/* Develop Line */}
                    <line x1="50" y1="140" x2="750" y2="140" className="stroke-blue-500 stroke-2" />
                    <text x="10" y="145" className="text-xs font-bold fill-blue-700">Dev</text>

                    {/* Feature Arc (Updated: Solid Line) */}
                    <path d="M150,140 C150,100 180,80 200,80 L300,80 C320,80 350,100 350,140" className="stroke-purple-400 stroke-2 fill-none" />
                    <text x="250" y="70" textAnchor="middle" className="text-[10px] fill-purple-600 font-mono">feature</text>

                    {/* Release Arc */}
                    <path d="M450,140 C450,180 470,180 490,180 L550,180 C570,180 590,220 600,220" className="stroke-yellow-500 stroke-2 fill-none" />
                    <line x1="600" y1="220" x2="600" y2="140" className="stroke-yellow-500 stroke-2 fill-none marker-end" />
                    <text x="520" y="200" textAnchor="middle" className="text-[10px] fill-yellow-600 font-mono">release</text>

                    {/* Hotfix Arc (Original) */}
                    <path d="M650,220 C650,260 670,260 690,260 L710,260 C730,260 730,220 730,220" className="stroke-red-500 stroke-2 fill-none" />

                    {/* --- CASCADE LINES (HOTFIX MERGE) - ORTOGONALES & PUNTEADAS --- */}

                    {/* 1. Hotfix (690,260) -> Feature Node (250,80) */}
                    <path
                        d="M 690 260 L 270 260 Q 250 260 250 240 L 250 86"
                        className="stroke-pink-500 stroke-2 fill-none"
                        style={{ strokeDasharray: "5, 5" }}
                        markerEnd="url(#arrow-cascade)"
                    />

                    {/* 2. Hotfix (690,260) -> Release Node (520,180) */}
                    <path
                        d="M 690 260 L 540 260 Q 520 260 520 240 L 520 186"
                        className="stroke-pink-500 stroke-2 fill-none"
                        style={{ strokeDasharray: "5, 5" }}
                        markerEnd="url(#arrow-cascade)"
                    />

                    <text x="530" y="275" className="text-[9px] fill-pink-600 font-bold bg-white" textAnchor="middle">HOTFIX CASCADE</text>

                    {/* Backmerge Hotfix -> Develop (Reference line) */}
                    <path d="M730,220 C730,180 730,140 730,140" className="stroke-red-500 stroke-2 fill-none opacity-50" style={{ strokeDasharray: "4, 4" }} />


                    {/* Nodes */}
                    <Node cx={100} cy={220} color="fill-green-500" type="v1.0" label="Stable Prod" onClick={() => { }} />
                    <Node cx={600} cy={220} color="fill-green-500" type="v1.2" label="Deploy Release v1.2" onClick={() => { }} />
                    <Node cx={730} cy={220} color="fill-green-500" type="v1.2.1" label="Deploy Hotfix v1.2.1" onClick={() => { }} />

                    <Node cx={150} cy={140} color="fill-blue-500" type="start" label="Start Feature" onClick={() => { }} />
                    <Node cx={350} cy={140} color="fill-blue-500" type="merge" label="Merge Feature" onClick={() => { }} />
                    <Node cx={450} cy={140} color="fill-blue-500" type="cut" label="Cut Release" onClick={() => { }} />
                    <Node cx={600} cy={140} color="fill-blue-500" type="sync" label="Backmerge Release" onClick={() => { }} />
                    <Node cx={730} cy={140} color="fill-blue-500" type="sync" label="Backmerge Hotfix" onClick={() => { }} />

                    <Node cx={250} cy={80} color="fill-purple-500" type="wip" label="Coding..." onClick={() => { }} />
                    <Node cx={520} cy={180} color="fill-yellow-500" type="qa" label="QA Testing" onClick={() => { }} />
                    <Node cx={690} cy={260} color="fill-red-500" type="fix" label="Hotfix Coding" onClick={() => { }} />


                    {/* --- SAP LAYER (BOTTOM) --- */}
                    <text x="50" y="340" className="text-xs font-bold fill-slate-400 font-mono">SAP LANDSCAPE:</text>

                    <EnvBox x={150} y={320} width={220} label="SAP D01" color="blue" alignNodeX={260} type="bottom" />
                    <EnvBox x={480} y={320} width={140} label="SAP Q01" color="yellow" alignNodeX={550} type="bottom" />
                    <EnvBox x={640} y={320} width={140} label="SAP P01" color="green" alignNodeX={710} type="bottom" />

                    {/* Connectors Git -> SAP */}
                    <line x1="260" y1="140" x2="260" y2="320" className="stroke-blue-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />
                    <line x1="550" y1="220" x2="550" y2="320" className="stroke-yellow-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />
                    <line x1="710" y1="220" x2="710" y2="320" className="stroke-green-200 stroke-1" style={{ strokeDasharray: "4, 2" }} />

                </svg>

                {activeTooltip && (
                    <div className="absolute top-4 right-4 bg-slate-800 text-white text-xs px-3 py-2 rounded shadow-lg animate-fade-in z-10">
                        {activeTooltip}
                    </div>
                )}
            </div>
            <p className="text-center text-slate-400 text-sm mt-4 italic">
                3-Tier Flow: Azure (Deployment) ← Git (Code) → SAP (Data)
            </p>
        </div>
    );
};

const BranchCard = ({ branch }: { branch: any }) => (
    <div className={`rounded-xl border ${branch.color} p-6 transition-all hover:shadow-md h-full flex flex-col`}>
        <div className="flex items-center justify-between mb-4">
            <div className="flex items-center space-x-2">
                <GitBranch className={`w-6 h-6 ${branch.iconColor}`} />
                <h3 className="font-bold text-lg">{branch.name}</h3>
            </div>
            <span className={`px-2 py-1 rounded text-xs font-bold uppercase tracking-wide border ${branch.color.replace('bg-', 'bg-opacity-50 ')}`}>
                {branch.type}
            </span>
        </div>

        <p className="text-sm mb-4 font-medium opacity-90">{branch.description}</p>

        <div className="space-y-3 text-sm mt-auto">
            <div className="flex items-start space-x-2">
                <Server className="w-4 h-4 mt-0.5 opacity-70" />
                <span><strong className="opacity-80">Azure Env:</strong> {branch.environment}</span>
            </div>
            <div className="flex items-start space-x-2">
                <Database className="w-4 h-4 mt-0.5 opacity-70" />
                <span><strong className="opacity-80">SAP Connection:</strong> {branch.sap_env}</span>
            </div>
            <div className="flex items-start space-x-2">
                <Shield className="w-4 h-4 mt-0.5 opacity-70" />
                <div className="flex-1">
                    <strong className="opacity-80 block mb-1">Protection:</strong>
                    <ul className="list-disc list-inside space-y-1 opacity-80 text-xs">
                        {branch.protection.map((rule: string, i: number) => <li key={i}>{rule}</li>)}
                    </ul>
                </div>
            </div>
        </div>
    </div>
);

const WorkflowSection = () => {
    const [activeTab, setActiveTab] = useState(0);

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
            <div className="flex border-b border-slate-200 overflow-x-auto">
                {strategyData.workflows.map((wf, index) => (
                    <button
                        key={index}
                        onClick={() => setActiveTab(index)}
                        className={`px-6 py-4 text-sm font-medium whitespace-nowrap transition-colors focus:outline-none ${activeTab === index
                            ? 'bg-blue-50 text-blue-700 border-b-2 border-blue-600'
                            : 'text-slate-600 hover:bg-slate-50'
                            }`}
                    >
                        {wf.title}
                    </button>
                ))}
            </div>
            <div className="p-6 bg-slate-50 min-h-[300px]">
                <div className="space-y-4">
                    {strategyData.workflows[activeTab].steps.map((step, i) => (
                        <div key={i} className="flex items-start space-x-4 bg-white p-4 rounded-lg border border-slate-200 shadow-sm relative overflow-hidden group">
                            <div className="flex-shrink-0 w-8 h-8 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center font-bold text-sm">
                                {i + 1}
                            </div>
                            <div className="flex-1">
                                <h4 className="font-bold text-slate-800">{step.title}</h4>
                                <p className="text-sm text-slate-600 mb-2">{step.desc}</p>
                                <div className="bg-slate-900 rounded p-2 font-mono text-xs text-green-400 overflow-x-auto">
                                    {step.cmd.startsWith('GH UI') ? <span className="text-yellow-400">{step.cmd}</span> : step.cmd}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
};

const FAQItem: React.FC<{ q: string; a: string }> = ({ q, a }) => {
    const [isOpen, setIsOpen] = useState(false);
    return (
        <div className="border border-slate-200 rounded-lg bg-white overflow-hidden">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="w-full flex items-center justify-between p-4 text-left hover:bg-slate-50 transition-colors"
            >
                <span className="font-semibold text-slate-800">{q}</span>
                {isOpen ? <ChevronUp className="w-5 h-5 text-slate-400" /> : <ChevronDown className="w-5 h-5 text-slate-400" />}
            </button>
            {isOpen && (
                <div className="p-4 pt-0 text-slate-600 text-sm border-t border-slate-100 bg-slate-50">
                    {a}
                </div>
            )}
        </div>
    );
};

// --- MAIN APP COMPONENT ---

export default function BranchingStrategy() {
    const [scrolled, setScrolled] = useState(false);

    useEffect(() => {
        const handleScroll = () => setScrolled(window.scrollY > 20);
        window.addEventListener('scroll', handleScroll);
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    return (
        <div className="min-h-screen bg-slate-50 font-sans text-slate-900">
            <style>{`
        .stroke-dasharray-2 { stroke-dasharray: 4 2; }
        .stroke-dasharray-4 { stroke-dasharray: 4 4; }
        @keyframes fade-in {
            from { opacity: 0; transform: translateY(-5px); }
            to { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-in { animation: fade-in 0.2s ease-out; }
      `}</style>
            <Header />
            <Hero />

            <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-16">

                {/* Visualizer Section */}
                <section id="visual" className="scroll-mt-24">
                    <div className="mb-6">
                        <h2 className="text-2xl font-bold text-slate-900">Strategy Visual Map</h2>
                        <p className="text-slate-600">Interaction between branches and SAP Landscape.</p>
                    </div>
                    <GitGraphVisualizer />
                </section>

                {/* Feature Storyboard Section */}
                <section id="storyboard" className="scroll-mt-24">
                    <FeatureStoryBoard />
                </section>

                {/* ACTORS SECTION */}
                <section id="actors" className="scroll-mt-24">
                    <ActorsSection />
                </section>

                {/* Branch Definitions Grid */}
                <section id="branches" className="scroll-mt-24">
                    <div className="mb-6">
                        <h2 className="text-2xl font-bold text-slate-900">Branch Definitions</h2>
                        <p className="text-slate-600">
                            Roles, responsibilities and SAP connection.
                        </p>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {strategyData.branches.map((branch) => (
                            <div key={branch.id} className={branch.id === 'main' || branch.id === 'develop' ? 'md:col-span-1 lg:col-span-1' : ''}>
                                <BranchCard branch={branch} />
                            </div>
                        ))}
                    </div>
                </section>

                {/* Workflows & Operations */}
                <section id="workflows" className="grid lg:grid-cols-2 gap-12 scroll-mt-24">
                    <div>
                        <div className="mb-6">
                            <h2 className="text-2xl font-bold text-slate-900 flex items-center">
                                <Terminal className="mr-2 text-blue-600" />
                                Operational Workflows
                            </h2>
                            <p className="text-slate-600">Step-by-step guides for daily operations.</p>
                        </div>
                        <WorkflowSection />
                    </div>

                    <div className="space-y-8">
                        {/* CICD Table */}
                        <div id="cicd" className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
                            <h3 className="text-lg font-bold mb-4 flex items-center text-slate-800">
                                <Globe className="mr-2 text-indigo-600 w-5 h-5" />
                                Azure CI/CD & SAP Integration
                            </h3>
                            <div className="overflow-x-auto">
                                <table className="w-full text-sm text-left">
                                    <thead className="bg-slate-50 text-slate-500 uppercase font-bold text-xs">
                                        <tr>
                                            <th className="px-3 py-2">Branch</th>
                                            <th className="px-3 py-2">Environment</th>
                                            <th className="px-3 py-2 text-blue-700">SAP Connection</th>
                                            <th className="px-3 py-2">Trigger</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-slate-100">
                                        {strategyData.envs.map((env, i) => (
                                            <tr key={i} className="hover:bg-slate-50">
                                                <td className="px-3 py-3 font-mono text-blue-600">{env.branch}</td>
                                                <td className="px-3 py-3 font-medium">{env.env}</td>
                                                <td className="px-3 py-3 font-medium text-blue-800 bg-blue-50/50 rounded">{env.sap}</td>
                                                <td className="px-3 py-3">
                                                    <span className={`px-2 py-1 rounded-full text-xs ${env.trigger.includes('Auto') ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                                                        {env.trigger}
                                                    </span>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>

                        {/* Commits Table */}
                        <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-6">
                            <h3 className="text-lg font-bold mb-4 flex items-center text-slate-800">
                                <GitCommit className="mr-2 text-pink-600 w-5 h-5" />
                                Conventional Commits
                            </h3>
                            <div className="grid grid-cols-1 gap-3">
                                {strategyData.commits.map((commit, i) => (
                                    <div key={i} className="flex items-center text-sm p-2 rounded hover:bg-slate-50 border border-transparent hover:border-slate-100">
                                        <span className="font-mono font-bold text-slate-700 w-20">{commit.type}</span>
                                        <span className="text-slate-600 flex-1">{commit.desc}</span>
                                        {commit.release !== '-' && (
                                            <span className="text-xs bg-slate-100 text-slate-500 px-2 py-0.5 rounded ml-2">
                                                {commit.release}
                                            </span>
                                        )}
                                    </div>
                                ))}
                            </div>
                            <p className="text-xs text-slate-400 mt-4 text-center">
                                Format: <code>&lt;type&gt;(&lt;scope&gt;): &lt;subject&gt;</code>
                            </p>
                        </div>
                    </div>
                </section>

                {/* FAQ Section */}
                <section className="max-w-3xl mx-auto scroll-mt-24">
                    <div className="text-center mb-8">
                        <h2 className="text-2xl font-bold text-slate-900">Frequently Asked Questions</h2>
                        <p className="text-slate-600">Resolving common questions about the strategy.</p>
                    </div>
                    <div className="space-y-4">
                        {strategyData.faq.map((item, i) => (
                            <FAQItem key={i} q={item.q} a={item.a} />
                        ))}
                    </div>
                </section>

                {/* Version History Footer */}
                <section className="max-w-3xl mx-auto mt-12 mb-12">
                    <div className="bg-slate-50 rounded-lg p-6 border border-slate-200">
                        <div className="flex items-center gap-2 mb-4 text-slate-700 font-bold">
                            <History className="w-5 h-5" />
                            <h3>Change History</h3>
                        </div>
                        <div className="space-y-3">
                            {strategyData.versions.map((ver, i) => (
                                <div key={i} className="flex items-center text-sm">
                                    <span className="font-mono font-bold bg-slate-200 text-slate-700 px-2 py-0.5 rounded mr-3 w-16 text-center">{ver.v}</span>
                                    <span className="text-slate-400 mr-4 text-xs">{ver.date}</span>
                                    <span className="text-slate-600">{ver.desc}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                </section>

            </main>

            <footer className="bg-slate-900 text-slate-400 py-12 mt-12 border-t border-slate-800">
                <div className="max-w-7xl mx-auto px-4 text-center">
                    <p className="mb-4 text-white font-bold tracking-wider">VENDOR MDM PLATFORM</p>
                    <div className="flex justify-center space-x-6 text-sm mb-8">
                        <span className="hover:text-white cursor-pointer">Documentation v2.17</span>
                        <span className="hover:text-white cursor-pointer">Azure DevOps</span>
                        <span className="hover:text-white cursor-pointer">GitHub Repo</span>
                    </div>
                    <p className="text-xs text-slate-600">
                        © 2025 Vendor MDM Dev Team. All rights reserved.
                        <br />Internal Implementation Proposal. App Version: v2.17
                    </p>
                </div>
            </footer>
        </div>
    );
}
