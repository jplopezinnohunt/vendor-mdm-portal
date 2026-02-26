import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { SystemService, DataSourceConfiguration } from '../../src/services/systemService';

// Store original fetch
const originalFetch = global.fetch;

describe('SystemService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    global.fetch = originalFetch;
  });

  describe('getDataSourceStatus', () => {
    it('returns data source configuration when API responds successfully', async () => {
      const mockConfig: DataSourceConfiguration = {
        mode: 'Production',
        database: {
          type: 'AzureSQL',
          connectionString: 'Server=***',
          isConnected: true,
          mode: 'Connected',
          statusMessage: 'Connected to Azure SQL',
        },
        cosmos: {
          endpoint: 'https://cosmos.documents.azure.com',
          isConnected: true,
          mode: 'Connected',
          databaseName: 'VendorMdm',
        },
        serviceBus: {
          namespace: 'sb-vendor-mdm',
          isConnected: true,
          mode: 'Connected',
        },
        email: {
          mode: 'Production',
          isConfigured: true,
          isConnected: true,
          smtpHost: 'smtp.sendgrid.net',
          fromEmail: 'noreply@vendor-mdm.com',
        },
        sap: {
          mode: 'Mock',
          isConnected: false,
          statusMessage: 'Using mock SAP service',
        },
        fileStorage: {
          mode: 'Connected',
          isConnected: true,
        },
        sanctions: {
          mode: 'Mock',
          isConnected: false,
        },
        lastChecked: '2024-01-15T10:30:00Z',
      };

      global.fetch = vi.fn().mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockConfig),
      });

      const result = await SystemService.getDataSourceStatus();

      expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/system/data-sources'));
      expect(result.mode).toBe('Production');
      expect(result.database.isConnected).toBe(true);
      expect(result.cosmos.isConnected).toBe(true);
      expect(result.sap.mode).toBe('Mock');
    });

    it('throws error when API response is not ok', async () => {
      global.fetch = vi.fn().mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
      });

      await expect(SystemService.getDataSourceStatus()).rejects.toThrow(
        'Failed to fetch data source status'
      );
    });

    it('throws error when network fails', async () => {
      global.fetch = vi.fn().mockRejectedValueOnce(new Error('Network error'));

      await expect(SystemService.getDataSourceStatus()).rejects.toThrow('Network error');
    });

    it('correctly parses all service configurations', async () => {
      const mockConfig: DataSourceConfiguration = {
        mode: 'Development',
        database: {
          type: 'InMemory',
          connectionString: '',
          isConnected: true,
          mode: 'Mock',
        },
        cosmos: {
          endpoint: '',
          isConnected: false,
          mode: 'Disabled',
        },
        serviceBus: {
          namespace: '',
          isConnected: false,
          mode: 'Disabled',
        },
        email: {
          mode: 'Console',
          isConfigured: false,
          isConnected: false,
        },
        sap: {
          mode: 'Simulation',
          isConnected: true,
          statusMessage: 'Using in-memory simulation',
        },
        fileStorage: {
          mode: 'LocalFileSystem',
          isConnected: true,
        },
        sanctions: {
          mode: 'Disabled',
          isConnected: false,
        },
        lastChecked: '2024-01-15T10:30:00Z',
      };

      global.fetch = vi.fn().mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(mockConfig),
      });

      const result = await SystemService.getDataSourceStatus();

      expect(result.mode).toBe('Development');
      expect(result.database.type).toBe('InMemory');
      expect(result.cosmos.mode).toBe('Disabled');
      expect(result.serviceBus.mode).toBe('Disabled');
      expect(result.email.mode).toBe('Console');
      expect(result.sap.statusMessage).toBe('Using in-memory simulation');
      expect(result.fileStorage.mode).toBe('LocalFileSystem');
      expect(result.sanctions.mode).toBe('Disabled');
    });

    it('handles partial configuration response', async () => {
      const partialConfig = {
        mode: 'Test',
        database: {
          type: 'SQLite',
          connectionString: 'test.db',
          isConnected: true,
          mode: 'Connected',
        },
        cosmos: {
          endpoint: '',
          isConnected: false,
          mode: 'Disabled',
        },
        serviceBus: {
          namespace: '',
          isConnected: false,
          mode: 'Disabled',
        },
        email: {
          mode: 'Mock',
          isConfigured: false,
          isConnected: false,
        },
        sap: {
          mode: 'Mock',
          isConnected: false,
        },
        fileStorage: {
          mode: 'Mock',
          isConnected: false,
        },
        sanctions: {
          mode: 'Mock',
          isConnected: false,
        },
        lastChecked: '2024-01-15T10:30:00Z',
      };

      global.fetch = vi.fn().mockResolvedValueOnce({
        ok: true,
        json: () => Promise.resolve(partialConfig),
      });

      const result = await SystemService.getDataSourceStatus();

      expect(result.mode).toBe('Test');
      expect(result.database.type).toBe('SQLite');
    });
  });
});
