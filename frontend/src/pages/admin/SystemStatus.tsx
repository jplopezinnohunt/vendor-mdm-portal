import React, { useEffect, useState } from 'react';
import { Card } from '../../components/ui/Elements';
import { SystemService, DataSourceConfiguration } from '../../services/systemService';
import {
    Database,
    Cloud,
    Radio,
    Mail,
    CheckCircle,
    AlertCircle,
    Circle,
    RefreshCw
} from 'lucide-react';

export const SystemStatus: React.FC = () => {
    const [config, setConfig] = useState<DataSourceConfiguration | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadStatus = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await SystemService.getDataSourceStatus();
            setConfig(data);
        } catch (err) {
            setError('Failed to load system status');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadStatus();
    }, []);

    const resolveMode = (mode: string | number): string => {
        if (typeof mode === 'number') {
            switch (mode) {
                case 1: return 'Local';
                case 2: return 'Mock';
                case 3: return 'Connected';
                default: return 'Auto';
            }
        }
        return mode || 'Unknown';
    };

    const getModeColor = (rawMode: string | number): string => {
        const mode = resolveMode(rawMode);
        switch (mode.toLowerCase()) {
            case 'connected':
                return 'bg-green-100 text-green-800';
            case 'local':
            case 'local emulator':
            case 'local development':
            case 'auto':
                return 'bg-gray-100 text-gray-800';
            case 'mock':
                return 'bg-yellow-100 text-yellow-800';
            case 'console logging':
            case 'not configured':
                return 'bg-orange-100 text-orange-800';
            default:
                return 'bg-blue-100 text-blue-800';
        }
    };

    const getStatusIcon = (isConnected: boolean, mode: string) => {
        if (mode.toLowerCase().includes('mock') || mode.toLowerCase().includes('console')) {
            return <Circle className="h-5 w-5 text-yellow-500" />;
        }
        if (isConnected) {
            return <CheckCircle className="h-5 w-5 text-green-500" />;
        }
        return <AlertCircle className="h-5 w-5 text-red-500" />;
    };

    if (loading) {
        return (
            <div className="flex h-64 items-center justify-center">
                <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-600 border-t-transparent"></div>
            </div>
        );
    }

    if (error || !config) {
        return (
            <div className="rounded-md bg-red-50 p-4">
                <div className="flex">
                    <AlertCircle className="h-5 w-5 text-red-400" />
                    <div className="ml-3">
                        <h3 className="text-sm font-medium text-red-800">Error loading system status</h3>
                        <p className="mt-2 text-sm text-red-700">{error || 'Unknown error'}</p>
                        <button
                            onClick={loadStatus}
                            className="mt-3 inline-flex items-center rounded-md bg-red-100 px-3 py-2 text-sm font-medium text-red-800 hover:bg-red-200"
                        >
                            <RefreshCw className="mr-2 h-4 w-4" />
                            Retry
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">System Status</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        Monitor data source connections and configuration
                    </p>
                </div>
                <button
                    onClick={loadStatus}
                    className="inline-flex items-center rounded-md bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm border border-gray-300 hover:bg-gray-50"
                >
                    <RefreshCw className="mr-2 h-4 w-4" />
                    Refresh
                </button>
            </div>

            {/* Overall Mode Card */}
            <Card className="p-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h2 className="text-lg font-semibold text-gray-900">Data Source Mode</h2>
                        <p className="mt-1 text-sm text-gray-500">
                            Current operating mode for all data sources
                        </p>
                    </div>
                    <div>
                        <span className={`inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold ${getModeColor(config.mode)}`}>
                            {resolveMode(config.mode)}
                        </span>
                    </div>
                </div>
                <div className="mt-4 text-xs text-gray-500">
                    Last checked: {new Date(config.lastChecked).toLocaleString()}
                </div>
            </Card>

            {/* Data Source Status Cards */}
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
                {/* Database */}
                <Card className="p-6">
                    <div className="flex items-start">
                        <div className="rounded-lg bg-blue-100 p-3">
                            <Database className="h-6 w-6 text-blue-600" />
                        </div>
                        <div className="ml-4 flex-1">
                            <div className="flex items-center justify-between">
                                <h3 className="text-lg font-semibold text-gray-900">Database</h3>
                                {getStatusIcon(config.database.isConnected, config.database.mode)}
                            </div>
                            <div className="mt-3 space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Type:</span>
                                    <span className="font-medium text-gray-900">{config.database.type}</span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Mode:</span>
                                    <span className={`rounded-full px-2 py-1 text-xs font-medium ${getModeColor(config.database.mode)}`}>
                                        {config.database.mode}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Status:</span>
                                    <span className="font-medium text-gray-900">
                                        {config.database.statusMessage || (config.database.isConnected ? 'Connected' : 'Not connected')}
                                    </span>
                                </div>
                                {config.database.connectionString && (
                                    <div className="mt-2 rounded-md bg-gray-50 p-2">
                                        <p className="text-xs font-mono text-gray-600 break-all">
                                            {config.database.connectionString}
                                        </p>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>
                </Card>

                {/* Cosmos DB */}
                <Card className="p-6">
                    <div className="flex items-start">
                        <div className="rounded-lg bg-purple-100 p-3">
                            <Cloud className="h-6 w-6 text-purple-600" />
                        </div>
                        <div className="ml-4 flex-1">
                            <div className="flex items-center justify-between">
                                <h3 className="text-lg font-semibold text-gray-900">Cosmos DB</h3>
                                {getStatusIcon(config.cosmos.isConnected, config.cosmos.mode)}
                            </div>
                            <div className="mt-3 space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Endpoint:</span>
                                    <span className="font-medium text-gray-900 truncate ml-2">
                                        {config.cosmos.endpoint || 'Not configured'}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Mode:</span>
                                    <span className={`rounded-full px-2 py-1 text-xs font-medium ${getModeColor(config.cosmos.mode)}`}>
                                        {config.cosmos.mode}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Status:</span>
                                    <span className="font-medium text-gray-900">
                                        {config.cosmos.statusMessage || (config.cosmos.isConnected ? 'Connected' : 'Not connected')}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </Card>

                {/* Service Bus */}
                <Card className="p-6">
                    <div className="flex items-start">
                        <div className="rounded-lg bg-indigo-100 p-3">
                            <Radio className="h-6 w-6 text-indigo-600" />
                        </div>
                        <div className="ml-4 flex-1">
                            <div className="flex items-center justify-between">
                                <h3 className="text-lg font-semibold text-gray-900">Service Bus</h3>
                                {getStatusIcon(config.serviceBus.isConnected, config.serviceBus.mode)}
                            </div>
                            <div className="mt-3 space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Namespace:</span>
                                    <span className="font-medium text-gray-900 truncate ml-2">
                                        {config.serviceBus.namespace || 'Not configured'}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Mode:</span>
                                    <span className={`rounded-full px-2 py-1 text-xs font-medium ${getModeColor(config.serviceBus.mode)}`}>
                                        {config.serviceBus.mode}
                                    </span>
                                </div>
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Status:</span>
                                    <span className="font-medium text-gray-900">
                                        {config.serviceBus.statusMessage || (config.serviceBus.isConnected ? 'Connected' : 'Not connected')}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </Card>

                {/* Email Service */}
                <Card className="p-6">
                    <div className="flex items-start">
                        <div className="rounded-lg bg-green-100 p-3">
                            <Mail className="h-6 w-6 text-green-600" />
                        </div>
                        <div className="ml-4 flex-1">
                            <div className="flex items-center justify-between">
                                <h3 className="text-lg font-semibold text-gray-900">Email Service</h3>
                                {getStatusIcon(config.email.isConnected, config.email.mode)}
                            </div>
                            <div className="mt-3 space-y-2">
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Mode:</span>
                                    <span className={`rounded-full px-2 py-1 text-xs font-medium ${getModeColor(config.email.mode)}`}>
                                        {config.email.mode}
                                    </span>
                                </div>
                                {config.email.smtpHost && (
                                    <div className="flex justify-between text-sm">
                                        <span className="text-gray-500">SMTP Host:</span>
                                        <span className="font-medium text-gray-900">{config.email.smtpHost}</span>
                                    </div>
                                )}
                                {config.email.fromEmail && (
                                    <div className="flex justify-between text-sm">
                                        <span className="text-gray-500">From Email:</span>
                                        <span className="font-medium text-gray-900">{config.email.fromEmail}</span>
                                    </div>
                                )}
                                <div className="flex justify-between text-sm">
                                    <span className="text-gray-500">Status:</span>
                                    <span className="font-medium text-gray-900">
                                        {config.email.statusMessage || (config.email.isConfigured ? 'Configured' : 'Not configured')}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                </Card>
            </div>

            {/* Service Integration Strategy */}
            <Card className="p-6">
                <div className="mb-4">
                    <h2 className="text-lg font-semibold text-gray-900">Service Integration Strategy</h2>
                    <p className="mt-1 text-sm text-gray-500">
                        Overview of service simulations and real system connections. Azure Mocks mimic real system behavior to enable robust scenario testing without hard dependencies.
                    </p>
                </div>
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Service Name</th>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Azure Mock (Simulation)</th>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Real System (Connected)</th>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description / Role</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            <tr>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">Vendor Service</td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-medium text-purple-800">
                                        Available
                                    </span>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                                        Planned
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-sm text-gray-500">
                                    Manages vendor master data. Mock simulates CRUD and search.
                                </td>
                            </tr>
                            <tr>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">SAP ECC Gateway</td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-medium text-purple-800">
                                        Available
                                    </span>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">
                                        Pending
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-sm text-gray-500">
                                    Simulates SAP responses (e.g., Block status, Payment terms). Triggers based on vendor ID.
                                </td>
                            </tr>
                            <tr>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">Sanctions Screening</td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-medium text-purple-800">
                                        Available
                                    </span>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">
                                        Pending
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-sm text-gray-500">
                                    Mock returns "Pass", "Fail", or "Review" based on applicant name patterns.
                                </td>
                            </tr>
                            <tr>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">Email Service</td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-purple-100 px-2.5 py-0.5 text-xs font-medium text-purple-800">
                                        Available
                                    </span>
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap">
                                    <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                                        Active
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-sm text-gray-500">
                                    Sends invitation and notification emails. Mock logs to console/DB.
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </Card>

            {/* Mode Information - moved to bottom */}
            <Card className="p-6 bg-blue-50 border-blue-200">
                <h3 className="text-sm font-semibold text-blue-900 mb-3">Data Source Modes</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
                    <div>
                        <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-1 text-xs font-medium text-gray-800 mb-1">
                            Local
                        </span>
                        <p className="text-blue-800">SQLite database + local emulators for development</p>
                    </div>
                    <div>
                        <span className="inline-flex items-center rounded-full bg-yellow-100 px-2 py-1 text-xs font-medium text-yellow-800 mb-1">
                            Mock
                        </span>
                        <p className="text-blue-800">No real connections, returns mock/empty data for testing</p>
                    </div>
                    <div>
                        <span className="inline-flex items-center rounded-full bg-green-100 px-2 py-1 text-xs font-medium text-green-800 mb-1">
                            Connected
                        </span>
                        <p className="text-blue-800">Full Azure resources (SQL Server, Cosmos DB, Service Bus)</p>
                    </div>
                </div>
            </Card>
        </div>
    );
};
