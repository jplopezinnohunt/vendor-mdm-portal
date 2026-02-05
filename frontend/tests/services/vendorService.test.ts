import { describe, it, expect, vi, beforeEach } from 'vitest';
import { VendorService } from '../../src/services/vendorService';
import { ChangeRequestStatus, ApplicationStatus } from '../../src/types';

// Mock the api module
vi.mock('../../src/services/api', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

import { api } from '../../src/services/api';

describe('VendorService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Suppress console.warn for tests
    vi.spyOn(console, 'warn').mockImplementation(() => {});
  });

  describe('getCurrentVendor', () => {
    it('returns vendor data from API when successful', async () => {
      const mockResponse = {
        data: {
          vendorId: '100450',
          name: 'Test Corp',
          address: '123 Test St',
        },
      };
      vi.mocked(api.get).mockResolvedValueOnce(mockResponse);

      const result = await VendorService.getCurrentVendor('100450');

      expect(api.get).toHaveBeenCalledWith('/changerequest/100450');
      expect(result.sapVendorId).toBe('100450');
      expect(result.name).toBe('Test Corp');
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await VendorService.getCurrentVendor('100450');

      // Should return mock data
      expect(result.sapVendorId).toBe('100450');
      expect(result.name).toBe('Acme Corp Global');
    });

    it('returns mock with requested vendorId when not found in mock map', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await VendorService.getCurrentVendor('999999');

      expect(result.sapVendorId).toBe('999999');
    });
  });

  describe('searchVendors', () => {
    it('returns search results from API when successful', async () => {
      const mockResponse = {
        data: [
          { sapVendorId: '100450', name: 'Acme Corp' },
          { sapVendorId: '100451', name: 'Acme Logistics' },
        ],
      };
      vi.mocked(api.get).mockResolvedValueOnce(mockResponse);

      const result = await VendorService.searchVendors('Acme');

      expect(api.get).toHaveBeenCalledWith('/vendor/search?query=Acme');
      expect(result).toHaveLength(2);
    });

    it('falls back to mock search when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await VendorService.searchVendors('Acme');

      // Should filter mock vendors by name
      expect(result.length).toBeGreaterThan(0);
      result.forEach((v) => {
        expect(v.name.toLowerCase()).toContain('acme');
      });
    });
  });

  describe('getChangeRequests', () => {
    it('returns change requests from API when successful', async () => {
      const mockResponse = {
        data: [
          { id: 'cr-001', status: 'New' },
          { id: 'cr-002', status: 'InReview' },
        ],
      };
      vi.mocked(api.get).mockResolvedValueOnce(mockResponse);

      const result = await VendorService.getChangeRequests();

      expect(api.get).toHaveBeenCalledWith('/changerequest/vendor/100450');
      expect(result).toHaveLength(2);
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await VendorService.getChangeRequests();

      // Should return mock requests for vendor 100450
      expect(Array.isArray(result)).toBe(true);
    });
  });

  describe('submitChangeRequest', () => {
    it('submits change request to API when successful', async () => {
      const mockResponse = {
        data: {
          id: 'new-cr-123',
          sapVendorId: '100450',
          createdAt: '2024-01-01T00:00:00Z',
        },
      };
      vi.mocked(api.post).mockResolvedValueOnce(mockResponse);

      const deltaItems = [{ field: 'name', oldValue: 'Old', newValue: 'New' }];
      const result = await VendorService.submitChangeRequest(deltaItems, [], '100450');

      expect(api.post).toHaveBeenCalledWith('/changerequest', expect.objectContaining({
        sapVendorId: '100450',
        payload: { items: deltaItems },
      }));
      expect(result.id).toBe('new-cr-123');
      expect(result.status).toBe(ChangeRequestStatus.New);
    });

    it('falls back to mock submission when API fails', async () => {
      vi.mocked(api.post).mockRejectedValueOnce(new Error('Network error'));

      const deltaItems = [{ field: 'name', oldValue: 'Old', newValue: 'New' }];
      const result = await VendorService.submitChangeRequest(deltaItems, [], '100450');

      expect(result.id).toMatch(/^cr-/);
      expect(result.status).toBe(ChangeRequestStatus.New);
      expect(result.items).toEqual(deltaItems);
    });
  });

  describe('getOnboardingRequests', () => {
    it('returns onboarding requests from API when successful', async () => {
      const mockResponse = {
        data: [
          { id: 'app-001', companyName: 'Test Inc', contactEmail: 'test@test.com', createdAt: '2024-01-01' },
        ],
      };
      vi.mocked(api.get).mockResolvedValueOnce(mockResponse);

      const result = await VendorService.getOnboardingRequests();

      expect(api.get).toHaveBeenCalledWith('/review/pending');
      expect(result[0].companyName).toBe('Test Inc');
      expect(result[0].status).toBe(ApplicationStatus.Submitted);
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await VendorService.getOnboardingRequests();

      expect(result.length).toBeGreaterThan(0);
      expect(result[0].companyName).toBeTruthy();
    });
  });

  describe('processChangeRequest', () => {
    it('approves change request via API when successful', async () => {
      vi.mocked(api.post).mockResolvedValueOnce({ data: {} });

      await VendorService.processChangeRequest('cr-001', ChangeRequestStatus.Approved);

      expect(api.post).toHaveBeenCalledWith('/changerequest/cr-001/approve', {});
    });

    it('handles approval when API fails', async () => {
      vi.mocked(api.post).mockRejectedValueOnce(new Error('Network error'));

      // Should not throw
      await expect(
        VendorService.processChangeRequest('cr-001', ChangeRequestStatus.Approved)
      ).resolves.not.toThrow();
    });
  });

  describe('getWorkflowRules', () => {
    it('returns workflow rules JSON', async () => {
      const result = await VendorService.getWorkflowRules();
      const parsed = JSON.parse(result);

      expect(parsed.rules).toBeDefined();
      expect(Array.isArray(parsed.rules)).toBe(true);
      expect(parsed.rules[0]).toHaveProperty('field');
      expect(parsed.rules[0]).toHaveProperty('risk');
      expect(parsed.rules[0]).toHaveProperty('approvers');
    });
  });
});
