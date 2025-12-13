// API Base URL configuration - use window.location for production, localhost for dev
const API_BASE_URL = window.location.hostname === 'localhost'
    ? 'http://localhost:5001'
    : window.location.origin;

export interface DataSourceConfiguration {
    mode: string;
    database: DatabaseConfig;
    cosmos: CosmosConfig;
    serviceBus: ServiceBusConfig;
    email: EmailConfig;
    lastChecked: string;
}

export interface DatabaseConfig {
    type: string;
    connectionString: string;
    isConnected: boolean;
    mode: string;
    statusMessage?: string;
}

export interface CosmosConfig {
    endpoint: string;
    isConnected: boolean;
    mode: string;
    databaseName?: string;
    statusMessage?: string;
}

export interface ServiceBusConfig {
    namespace: string;
    isConnected: boolean;
    mode: string;
    statusMessage?: string;
}

export interface EmailConfig {
    mode: string;
    isConfigured: boolean;
    smtpHost?: string;
    fromEmail?: string;
    statusMessage?: string;
}

export const SystemService = {
    /**
     * Get current data source configuration and connection status
     */
    getDataSourceStatus: async (): Promise<DataSourceConfiguration> => {
        const response = await fetch(`${API_BASE_URL}/api/system/data-sources`);
        if (!response.ok) {
            throw new Error('Failed to fetch data source status');
        }
        return response.json();
    },
};
