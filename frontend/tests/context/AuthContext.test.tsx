import React from 'react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act, waitFor } from '@testing-library/react';
import { renderHook } from '@testing-library/react';
import { AuthProvider, useAuth, UserRole } from '../../src/context/AuthContext';
import axios from 'axios';

// Mock axios
vi.mock('axios');
const mockedAxios = vi.mocked(axios, true);

// Mock authConfig
vi.mock('../../src/authConfig', () => ({
  loginRequest: { scopes: ['user.read'] },
}));

// Re-mock @azure/msal-react with stable mock implementations that survive vi.clearAllMocks()
const stableMsalInstance = {
  loginPopup: vi.fn().mockResolvedValue({ account: null }),
  logout: vi.fn().mockResolvedValue(undefined),
  logoutRedirect: vi.fn().mockResolvedValue(undefined),
  acquireTokenSilent: vi.fn().mockResolvedValue({ accessToken: 'mock-token' }),
  getAllAccounts: vi.fn().mockReturnValue([]),
  getActiveAccount: vi.fn().mockReturnValue(null),
  setActiveAccount: vi.fn(),
};

vi.mock('@azure/msal-react', () => ({
  MsalProvider: ({ children }: { children: React.ReactNode }) => children,
  useMsal: () => ({
    instance: stableMsalInstance,
    accounts: [],
    inProgress: 'none',
  }),
  useIsAuthenticated: () => false,
  useAccount: () => null,
}));

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    // Re-establish MSAL mock return values after clearAllMocks resets them
    stableMsalInstance.logoutRedirect.mockResolvedValue(undefined);
    stableMsalInstance.loginPopup.mockResolvedValue({ account: null });
    stableMsalInstance.acquireTokenSilent.mockResolvedValue({ accessToken: 'mock-token' });
    stableMsalInstance.getAllAccounts.mockReturnValue([]);
    stableMsalInstance.getActiveAccount.mockReturnValue(null);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    localStorage.clear();
  });

  describe('useAuth hook', () => {
    it('throws error when used outside AuthProvider', () => {
      // Suppress console.error for this test since we expect an error
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      expect(() => {
        renderHook(() => useAuth());
      }).toThrow('useAuth must be used within an AuthProvider');

      consoleSpy.mockRestore();
    });

    it('provides context when used within AuthProvider', () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current).toBeDefined();
      expect(result.current.user).toBeNull();
      expect(result.current.isAuthenticated).toBe(false);
      expect(typeof result.current.login).toBe('function');
      expect(typeof result.current.logout).toBe('function');
    });
  });

  describe('Initial state', () => {
    it('starts with isLoading true', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      // Initially loading is true, eventually becomes false (after timeout or fetch)
      await waitFor(
        () => {
          expect(result.current.isLoading).toBe(false);
        },
        { timeout: 4000 }
      );
    });

    it('starts with user as null', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.user).toBeNull();
      });
    });
  });

  describe('Mock login', () => {
    it('sets user with mock login', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.mockLogin('Admin');
      });

      expect(result.current.user).toBeDefined();
      expect(result.current.user?.role).toBe('Admin');
      expect(result.current.user?.name).toBe('Mock Admin');
      expect(result.current.isAuthenticated).toBe(true);
    });

    it('persists mock user in localStorage', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.mockLogin('Vendor');
      });

      const storedUser = localStorage.getItem('mockUser');
      expect(storedUser).toBeDefined();
      const parsed = JSON.parse(storedUser!);
      expect(parsed.role).toBe('Vendor');
    });

    it('supports all user roles', async () => {
      const roles: UserRole[] = ['Vendor', 'Requestor', 'VendorUnit', 'BFM', 'Admin', 'Approver', 'Viewer'];
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      for (const role of roles) {
        localStorage.clear();
        const { result } = renderHook(() => useAuth(), { wrapper });

        await act(async () => {
          await result.current.mockLogin(role);
        });

        expect(result.current.user?.role).toBe(role);
      }
    });
  });

  describe('Logout', () => {
    it('clears user on logout', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      // First login
      await act(async () => {
        await result.current.mockLogin('Admin');
      });

      expect(result.current.isAuthenticated).toBe(true);

      // Then logout
      await act(async () => {
        result.current.logout();
      });

      expect(result.current.user).toBeNull();
      expect(result.current.isAuthenticated).toBe(false);
    });

    it('clears mockUser from localStorage on logout', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await act(async () => {
        await result.current.mockLogin('Admin');
      });

      expect(localStorage.getItem('mockUser')).toBeDefined();

      await act(async () => {
        result.current.logout();
      });

      expect(localStorage.getItem('mockUser')).toBeNull();
    });
  });

  describe('Local authentication', () => {
    it('sets user with loginLocal', async () => {
      // Mock axios.get for the fetchProfile call
      mockedAxios.get.mockResolvedValue({
        data: {
          id: 'test-id',
          username: 'Test User',
          email: 'test@example.com',
          roles: ['Admin'],
        },
      });

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });
      const testUser = {
        id: 'test-id',
        name: 'Test User',
        email: 'test@example.com',
        role: 'Admin' as UserRole,
      };

      await act(async () => {
        await result.current.loginLocal('test-token', testUser);
      });

      // Wait for the effect to complete
      await waitFor(() => {
        expect(result.current.user).toBeDefined();
      });

      expect(result.current.isAuthenticated).toBe(true);
    });

    it('stores token in localStorage', async () => {
      mockedAxios.get.mockResolvedValue({
        data: {
          id: 'test-id',
          username: 'Test User',
          email: 'test@example.com',
          roles: ['Admin'],
        },
      });

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });
      const testUser = {
        id: 'test-id',
        name: 'Test User',
        email: 'test@example.com',
        role: 'Admin' as UserRole,
      };

      await act(async () => {
        await result.current.loginLocal('my-secure-token', testUser);
      });

      // The token is stored immediately before the profile fetch
      expect(localStorage.getItem('localToken')).toBe('my-secure-token');
    });
  });

  describe('getToken', () => {
    it('returns localToken when available', async () => {
      // Mock axios.get for fetchProfile
      mockedAxios.get.mockResolvedValue({
        data: {
          id: 'test-id',
          username: 'Test User',
          email: 'test@example.com',
          roles: ['Vendor'],
        },
      });

      // Need sessionTimestamp to prevent session expiry clearing the token
      localStorage.setItem('localToken', 'stored-token');
      localStorage.setItem('sessionTimestamp', Date.now().toString());
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => expect(result.current.isLoading).toBe(false));

      let token: string | null = null;
      await act(async () => {
        token = await result.current.getToken();
      });

      expect(token).toBe('stored-token');
    });

    it('returns null when no token or account', async () => {
      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => expect(result.current.isLoading).toBe(false));

      let token: string | null = null;
      await act(async () => {
        token = await result.current.getToken();
      });

      expect(token).toBeNull();
    });
  });

  describe('Persisted mock user', () => {
    it('restores mock user from localStorage on mount', async () => {
      const mockUser = {
        id: 'stored-id',
        name: 'Stored User',
        email: 'stored@example.com',
        role: 'Vendor',
        isImpersonated: false,
      };
      // Need sessionTimestamp to pass expiry check in AuthContext
      localStorage.setItem('mockUser', JSON.stringify(mockUser));
      localStorage.setItem('sessionTimestamp', Date.now().toString());

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.user).toEqual(mockUser);
      });
      expect(result.current.isAuthenticated).toBe(true);
    });

    it('handles corrupted localStorage data gracefully', async () => {
      // Need sessionTimestamp so the code reaches the JSON.parse block
      // (otherwise checkSessionExpiry returns early before parsing)
      localStorage.setItem('mockUser', 'not-valid-json');
      localStorage.setItem('sessionTimestamp', Date.now().toString());

      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.user).toBeNull();
      expect(localStorage.getItem('mockUser')).toBeNull();

      consoleSpy.mockRestore();
    });
  });

  describe('Impersonation', () => {
    it('calls impersonate endpoint with correct params', async () => {
      // IMPORTANT: Set localStorage BEFORE mounting component
      // because localToken state is initialized from localStorage at mount time
      localStorage.setItem('localToken', 'admin-token');
      localStorage.setItem('sessionTimestamp', Date.now().toString());

      // Mock axios for both fetchProfile and impersonate
      mockedAxios.get.mockResolvedValue({
        data: {
          id: 'admin-id',
          username: 'Admin User',
          email: 'admin@example.com',
          roles: ['Admin'],
        },
      });
      mockedAxios.post.mockResolvedValue({ data: {} });

      // Mock window.location.reload before mounting
      const reloadMock = vi.fn();
      Object.defineProperty(window, 'location', {
        value: { reload: reloadMock, href: '/' },
        writable: true,
        configurable: true,
      });

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => expect(result.current.isLoading).toBe(false));

      await act(async () => {
        await result.current.impersonate('Vendor', 'Test Vendor', 'vendor@test.com');
      });

      expect(mockedAxios.post).toHaveBeenCalledWith(
        '/api/auth/impersonate',
        { role: 'Vendor', displayName: 'Test Vendor', email: 'vendor@test.com' },
        { headers: { Authorization: 'Bearer admin-token' } }
      );
    });

    it('calls stop impersonation endpoint', async () => {
      mockedAxios.post.mockResolvedValue({ data: {} });

      // Mock window.location.reload before mounting
      const reloadMock = vi.fn();
      Object.defineProperty(window, 'location', {
        value: { reload: reloadMock, href: '/' },
        writable: true,
        configurable: true,
      });

      const wrapper = ({ children }: { children: React.ReactNode }) => (
        <AuthProvider>{children}</AuthProvider>
      );

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => expect(result.current.isLoading).toBe(false));

      await act(async () => {
        await result.current.stopImpersonation();
      });

      expect(mockedAxios.post).toHaveBeenCalledWith('/api/auth/stop-impersonation');
    });
  });
});
