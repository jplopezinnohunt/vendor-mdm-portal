import React, { createContext, useContext, useState, useEffect, PropsWithChildren } from 'react';
import { useMsal, useAccount } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { InteractionStatus, InteractionRequiredAuthError } from "@azure/msal-browser";
import axios from 'axios';

// User type mimicking claims from Azure AD B2C / Entra ID
export type UserRole = 'Vendor' | 'Admin' | 'Approver';

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
  logout: () => void;
  getToken: () => Promise<string | null>;
  impersonate: (role: string, displayName?: string, email?: string) => Promise<void>;
  stopImpersonation: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: PropsWithChildren<{}>) => {
  const { instance, accounts, inProgress } = useMsal();
  const account = accounts[0] || undefined;
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Helper to acquire token
  const getToken = async () => {
    if (!account) return null;
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: account
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        // Should handle by asking user to login again, but for getToken just return null
        return null;
      }
      console.error(error);
      return null;
    }
  };

  useEffect(() => {
    // Timeout to prevent infinite loading
    const timeout = setTimeout(() => {
      if (isLoading) {
        console.warn('[AuthContext] Loading timeout - setting isLoading to false');
        setIsLoading(false);
      }
    }, 3000);

    const fetchProfile = async () => {
      if (account && inProgress === InteractionStatus.None) {
        setIsLoading(true);
        try {
          const token = await getToken();
          if (token) {
            const response = await axios.get('/api/auth/profile', {
              headers: { Authorization: `Bearer ${token}` }
            });

            const data = response.data;

            // Helper key mapping to handle case-sensitivity from backend JSON
            const roles = data.roles || data.Roles || [];
            const userId = data.userId || data.UserId;
            const displayName = data.displayName || data.DisplayName;
            const email = data.email || data.Email;
            const isImpersonated = data.isImpersonated || data.IsImpersonated;

            let role: UserRole = 'Vendor';
            if (roles.includes('Admin')) role = 'Admin';
            else if (roles.includes('Approver')) role = 'Approver';

            setUser({
              id: userId,
              name: displayName,
              email: email,
              role: role,
              isImpersonated: isImpersonated
            });
          }
        } catch (err) {
          console.error("Failed to fetch profile", err);
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
  }, [account, inProgress]);

  const login = async (role?: UserRole) => {
    // We ignore role for Real Auth, effectively.
    await instance.loginPopup(loginRequest);
  };

  const logout = () => {
    instance.logoutRedirect();
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