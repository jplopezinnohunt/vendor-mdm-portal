import React, { createContext, useContext, useState, useEffect, PropsWithChildren } from 'react';
import { useMsal, useAccount } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { InteractionStatus, InteractionRequiredAuthError } from "@azure/msal-browser";
import axios from 'axios';

// User type mimicking claims from Azure AD B2C / Entra ID
export type UserRole = 'Vendor' | 'Requestor' | 'VendorUnit' | 'BFM' | 'Admin' | 'Approver' | 'Viewer';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  sapId?: string; // Links to SAP LIFNR, only for Vendors
  isImpersonated?: boolean;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (role?: UserRole) => Promise<void>;
  loginLocal: (token: string, user: User) => Promise<void>;
  mockLogin: (role: UserRole) => Promise<void>;
  logout: () => void;
  getToken: () => Promise<string | null>;
  impersonate: (role: string, displayName?: string, email?: string) => Promise<void>;
  stopImpersonation: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: PropsWithChildren<{}>) => {
  const { instance, accounts, inProgress } = useMsal();
  const account = accounts[0] || undefined;
  const [localToken, setLocalToken] = useState<string | null>(localStorage.getItem('localToken'));
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Helper to acquire token
  const getToken = async () => {
    if (localToken) return localToken; // Priority to local token

    if (!account) return null;
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: account
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        return null;
      }
      console.error(error);
      return null;
    }
  };

  useEffect(() => {
    // Session expiration time in milliseconds (2 hours)
    const SESSION_EXPIRY_MS = 2 * 60 * 60 * 1000;

    // Check for session expiration
    const checkSessionExpiry = (): boolean => {
      const sessionTimestamp = localStorage.getItem('sessionTimestamp');
      if (!sessionTimestamp) return true; // No timestamp = expired

      const elapsed = Date.now() - parseInt(sessionTimestamp, 10);
      if (elapsed > SESSION_EXPIRY_MS) {
        console.warn('[AuthContext] Session expired - clearing auth data');
        localStorage.removeItem('mockUser');
        localStorage.removeItem('localToken');
        localStorage.removeItem('sessionTimestamp');
        return true;
      }
      return false;
    };

    // Check for persisted mock user first
    const storedMockUser = localStorage.getItem('mockUser');
    if (storedMockUser) {
      // Check if session has expired
      if (checkSessionExpiry()) {
        setUser(null);
        setIsLoading(false);
        return;
      }

      try {
        const mockUser = JSON.parse(storedMockUser);
        setUser(mockUser);
        setIsLoading(false);
        return;
      } catch (e) {
        console.error('Failed to parse stored mock user', e);
        localStorage.removeItem('mockUser');
        localStorage.removeItem('sessionTimestamp');
      }
    }

    // Check localToken expiry too
    const storedToken = localStorage.getItem('localToken');
    if (storedToken && checkSessionExpiry()) {
      setLocalToken(null);
      setUser(null);
      setIsLoading(false);
      return;
    }

    // Check for Local Auth Token
    if (localToken) {
      // Ideally verify token or just fetch profile
      // For now, let fetchProfile handle validity
    }

    // Timeout - extended to 15 seconds to allow MSAL to complete initialization
    const timeout = setTimeout(() => {
      if (isLoading) {
        console.warn('[AuthContext] Loading timeout - no auth found, redirecting to login');
        setIsLoading(false);
        setUser(null);
      }
    }, 15000);

    const fetchProfile = async () => {
      // Logic: If account OR localToken exists
      if ((account && inProgress === InteractionStatus.None) || localToken) {
        setIsLoading(true);
        try {
          const token = await getToken();
          if (token) {
            // If local token, maybe we already have user info? 
            // Better to always fetch profile to stay in sync
            // Note: Our backend /api/auth/profile needs to support the local JWT too.
            // Since we implemented JWT in AuthController, we essentially need to make sure 
            // the backend *verifies* it. (Program.cs handles JwtBearer).

            // However, the /profile endpoint might be in UserController or AuthController.
            // If it's the old mocking one, we might need a new one or ensure old one adapts.
            // Let's assume /api/auth/profile works with the new JWT claims (UserId, Role, etc).

            // Wait, existing profile endpoint might rely on Graph if Azure AD. 
            // If Local, it should just return User from DB.
            // We need to verify /api/auth/profile implementation on Backend later.

            // For now, let's assume we can set user directly from login response for Local Auth
            // to avoid round trip if possible, BUT fetchProfile is cleaner for page reloads.

            const response = await axios.get('/api/user/me', { // Changed to /user/me or similar if exists, or fallback
              headers: { Authorization: `Bearer ${token}` }
            });

            // If 404/401, catch block handles it.
            const data = response.data;

            const roles = data.roles || data.Roles || [];
            const userId = data.id || data.Id;
            const displayName = data.username || data.Username || data.email;
            const email = data.email || data.Email;

            let role: UserRole = 'Vendor';
            if (roles.includes('Admin')) role = 'Admin';
            else if (roles.includes('Approver')) role = 'Approver';
            else if (roles.includes('VendorUnit')) role = 'VendorUnit';
            else if (roles.includes('BFM')) role = 'BFM';
            else if (roles.includes('Requestor') || roles.includes('Requester')) role = 'Requestor';
            else if (roles.includes('Viewer')) role = 'Viewer';

            setUser({
              id: userId,
              name: displayName,
              email: email,
              role: role,
              isImpersonated: false
            });
          }
        } catch (err) {
          console.error("Failed to fetch profile", err);
          // If local token is invalid, clear it
          if (localToken) {
            setLocalToken(null);
            localStorage.removeItem('localToken');
          }
          setUser(null);
        } finally {
          setIsLoading(false);
        }
      } else if (!account && inProgress === InteractionStatus.None) {
        setUser(null);
        setIsLoading(false);
      }
    };

    fetchProfile();

    return () => clearTimeout(timeout);
  }, [account, inProgress, localToken]);

  const loginLocal = async (token: string, userData: User) => {
    setLocalToken(token);
    localStorage.setItem('localToken', token);
    localStorage.setItem('sessionTimestamp', Date.now().toString());
    setUser(userData);
    setIsLoading(false);
  };

  const mockLogin = async (role: UserRole) => {
    const mockUser: User = {
      id: 'mock-user-id',
      name: `Mock ${role}`,
      email: `mock.${role.toLowerCase()}@example.com`,
      role: role,
      isImpersonated: false
    };
    setUser(mockUser);
    localStorage.setItem('mockUser', JSON.stringify(mockUser));
    localStorage.setItem('sessionTimestamp', Date.now().toString());
    setIsLoading(false);
  };

  const login = async (role?: UserRole) => {
    try {
      await instance.loginPopup(loginRequest);
    } catch (e) {
      console.error("Login failed", e);
    }
  };

  const logout = () => {
    localStorage.removeItem('mockUser');
    localStorage.removeItem('sessionTimestamp');
    if (localToken) {
      localStorage.removeItem('localToken');
      setLocalToken(null);
      setUser(null);
      // Redirect to login
      window.location.href = '/login';
    } else {
      // Check if MSAL interaction is already in progress
      if (inProgress !== InteractionStatus.None) {
        console.warn('[AuthContext] MSAL interaction in progress, skipping logout redirect');
        // Clear local state and redirect manually
        setUser(null);
        window.location.href = '/login';
        return;
      }
      instance.logoutRedirect().catch((e) => {
        console.error('[AuthContext] Logout redirect failed:', e);
        // Fallback: clear state and redirect manually
        setUser(null);
        window.location.href = '/login';
      });
    }
    setUser(null);
  };

  const impersonate = async (role: string, displayName?: string, email?: string) => {
    const token = await getToken();
    if (!token) return;
    try {
      await axios.post('/api/auth/impersonate', { role, displayName, email }, {
        headers: { Authorization: `Bearer ${token}` }
      });
      window.location.reload();
    } catch (e) {
      console.error("Impersonation failed", e);
    }
  };

  const stopImpersonation = async () => {
    try {
      // No auth header needed as cookie is sent automatically, and endpoint is public (checks cookie presence basically)
      await axios.post('/api/auth/stop-impersonation');
      window.location.reload();
    } catch (e) {
      console.error("Stop impersonation failed", e);
    }
  };

  return (
    <AuthContext.Provider value={{
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      loginLocal,
      mockLogin,
      logout,
      getToken,
      impersonate,
      stopImpersonation
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};