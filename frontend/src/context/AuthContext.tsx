import React, { createContext, useContext, useState, useEffect, PropsWithChildren } from 'react';

// User type mimicking claims from Azure AD B2C / Entra ID
export type UserRole = 'Vendor' | 'Admin' | 'Approver';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  sapId?: string; // Links to SAP LIFNR, only for Vendors
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (role?: UserRole) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: PropsWithChildren<{}>) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Simulate checking for existing session / token
    const timer = setTimeout(() => {
      // Mock persistent login for demo purposes
      const storedUser = localStorage.getItem('mdm_user');
      if (storedUser) {
        setUser(JSON.parse(storedUser));
      }
      setIsLoading(false);
    }, 500);
    return () => clearTimeout(timer);
  }, []);

  const login = async (role: UserRole = 'Vendor') => {
    setIsLoading(true);
    // Mimic MSAL Popup Login
    return new Promise<void>((resolve) => {
      setTimeout(() => {
        let mockUser: User;

        switch (role) {
          case 'Admin':
            mockUser = {
              id: 'adm001',
              name: 'System Administrator',
              email: 'admin@company.com',
              role: 'Admin'
            };
            break;
          case 'Approver':
            mockUser = {
              id: 'apr001',
              name: 'Jane Doe (Finance)',
              email: 'jane.doe@company.com',
              role: 'Approver'
            };
            break;
          case 'Vendor':
          default:
            mockUser = {
              id: 'v123',
              name: 'Acme Corp Admin',
              email: 'admin@acme.com',
              role: 'Vendor',
              sapId: '100450',
            };
            break;
        }

        setUser(mockUser);
        localStorage.setItem('mdm_user', JSON.stringify(mockUser));
        setIsLoading(false);
        resolve();
      }, 1000);
    });
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('mdm_user');
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout }}>
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