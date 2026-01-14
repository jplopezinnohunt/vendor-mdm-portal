import { API_BASE_URL } from './api';

export interface DataSourceConfiguration {
    mode: string | number;
    database: DatabaseConfig;
    cosmos: CosmosConfig;
    serviceBus: ServiceBusConfig;
    email: EmailConfig;
    sap: BaseServiceConfig;
    fileStorage: BaseServiceConfig;
    sanctions: BaseServiceConfig;
    lastChecked: string;
}

export interface BaseServiceConfig {
    mode: string;
    isConnected: boolean;
    statusMessage?: string;
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
    isConnected: boolean;
    smtpHost?: string;
    fromEmail?: string;
    statusMessage?: string;
}


export const SystemService = {
    /**
     * Get current data source configuration and connection status
     */
    getDataSourceStatus: async (): Promise<DataSourceConfiguration> => {
        const response = await fetch(`${API_BASE_URL}/system/data-sources`);
        if (!response.ok) {
            throw new Error('Failed to fetch data source status');
        }
        return response.json();
    },
};
