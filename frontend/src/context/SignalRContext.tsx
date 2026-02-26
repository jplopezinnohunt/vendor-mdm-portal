import React, { createContext, useContext, useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './AuthContext';

// Event types that can be received from the backend
export interface StatusChangedEvent {
  entityType: string;
  entityId: string;
  oldStatus: string;
  newStatus: string;
  timestamp: string;
}

export interface VendorCreatedEvent {
  vendorId: string;
  legalName: string;
  accountGroup: string;
  eventId: string;
  timestamp: string;
}

export interface NotificationEvent {
  title: string;
  message: string;
  link?: string;
  timestamp: string;
}

export interface TaskAssignedEvent {
  taskType: string;
  entityId: string;
  description: string;
  timestamp: string;
}

export interface SapSyncResultEvent {
  vendorId: string;
  success: boolean;
  sapVendorNumber?: string;
  errorMessage?: string;
  timestamp: string;
}

// Connection state
export type ConnectionState = 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting';

// Context type
interface SignalRContextType {
  connectionState: ConnectionState;
  subscribe: <T>(eventName: string, callback: (data: T) => void) => () => void;
  joinGroup: (groupName: string) => Promise<void>;
  leaveGroup: (groupName: string) => Promise<void>;
  subscribeToVendor: (vendorId: string) => Promise<void>;
  unsubscribeFromVendor: (vendorId: string) => Promise<void>;
}

const SignalRContext = createContext<SignalRContextType | null>(null);

interface SignalRProviderProps {
  children: React.ReactNode;
  hubUrl?: string;
}

export const SignalRProvider: React.FC<SignalRProviderProps> = ({
  children,
  hubUrl = '/hubs/events'
}) => {
  const [connectionState, setConnectionState] = useState<ConnectionState>('Disconnected');
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const subscribersRef = useRef<Map<string, Set<(data: unknown) => void>>>(new Map());
  const { isAuthenticated, isLoading, getToken, user } = useAuth();

  // Check if this is a mock user - derive from user object, not localStorage
  // Mock users have id='mock-user-id' (see AuthContext.mockLogin)
  const isMockUser = user?.id === 'mock-user-id' || user?.id?.startsWith('mock-');

  // Get the hub URL - use relative path (Vite proxy handles routing) or explicit URL
  const getFullUrl = useCallback(() => {
    const apiBaseUrl = import.meta.env.VITE_API_URL || '';
    let url = `${apiBaseUrl}${hubUrl}`;

    // For mock users, append mockUser query param for backend auth
    // Use the user from AuthContext (more reliable than re-reading localStorage)
    if (isMockUser && user?.role) {
      url += `?mockUser=${encodeURIComponent(user.role)}`;
      console.log('[SignalR] Building URL with mock user role:', user.role);
    }

    console.log('[SignalR] Full URL:', url);
    return url;
  }, [hubUrl, isMockUser, user?.role]);

  // Stable refs for functions that shouldn't trigger reconnection
  const getTokenRef = useRef(getToken);
  getTokenRef.current = getToken;
  const getFullUrlRef = useRef(getFullUrl);
  getFullUrlRef.current = getFullUrl;

  // Derive stable primitive values from user to avoid object-reference re-renders
  const userRole = user?.role;
  const userId = user?.id;

  // Initialize connection only when authenticated
  useEffect(() => {
    // Don't connect if still loading auth or not authenticated
    if (isLoading || !isAuthenticated) {
      console.log('[SignalR] Waiting for authentication... isLoading:', isLoading, 'isAuthenticated:', isAuthenticated);
      return;
    }

    // For mock users, validate we have a role
    if (isMockUser) {
      if (!userRole) {
        console.error('[SignalR] Mock user detected but no role available');
        return;
      }
      console.log('[SignalR] Connecting as mock user:', userRole);
    } else {
      console.log('[SignalR] Real user detected, will use token authentication');
    }

    const fullUrl = getFullUrlRef.current();

    // Verify the URL has proper auth for mock users
    if (isMockUser && !fullUrl.includes('mockUser=')) {
      console.error('[SignalR] Mock user detected but URL is missing mockUser parameter');
      console.error('[SignalR] URL:', fullUrl, 'user.role:', userRole);
      return;
    }

    let cancelled = false;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(fullUrl, {
        withCredentials: true,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        // Only use accessTokenFactory for real tokens (not mock users)
        accessTokenFactory: isMockUser ? undefined : async () => {
          const token = await getTokenRef.current();
          if (!token) {
            console.warn('[SignalR] No auth token available');
          }
          return token || '';
        }
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0, 2s, 4s, 8s, 16s, then cap at 30s
          if (retryContext.previousRetryCount < 5) {
            return Math.pow(2, retryContext.previousRetryCount) * 1000;
          }
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connectionRef.current = connection;

    // Connection state handlers
    connection.onreconnecting(() => {
      console.log('[SignalR] Reconnecting...');
      setConnectionState('Reconnecting');
    });

    connection.onreconnected(() => {
      console.log('[SignalR] Reconnected');
      setConnectionState('Connected');
    });

    connection.onclose(() => {
      console.log('[SignalR] Connection closed');
      setConnectionState('Disconnected');
    });

    // Start connection
    const startConnection = async () => {
      if (cancelled) return;
      try {
        setConnectionState('Connecting');
        await connection.start();
        if (cancelled) {
          connection.stop();
          return;
        }
        console.log('[SignalR] Connected to', fullUrl);
        setConnectionState('Connected');
      } catch (error) {
        if (cancelled) return;
        // Extract useful error information
        const errorMessage = error instanceof Error
          ? error.message
          : typeof error === 'string'
            ? error
            : JSON.stringify(error);
        console.error('[SignalR] Connection failed:', errorMessage);

        // Check for auth-related failures
        if (errorMessage.includes('401') || errorMessage.includes('Unauthorized')) {
          console.error('[SignalR] Authentication failed. Check that mock user is set correctly or token is valid.');
          console.error('[SignalR] Mock user in localStorage:', localStorage.getItem('mockUser'));
          // Don't retry on auth failures - user needs to re-login
          setConnectionState('Disconnected');
          return;
        }

        // "stopped during negotiation" is typically caused by cleanup, not a real error
        if (errorMessage.includes('negotiation')) {
          console.log('[SignalR] Connection stopped during negotiation (effect cleanup)');
          return;
        }

        setConnectionState('Disconnected');
        // Retry after 5 seconds for transient failures
        if (!cancelled) {
          setTimeout(startConnection, 5000);
        }
      }
    };

    startConnection();

    // Cleanup on unmount or dependency change
    return () => {
      cancelled = true;
      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }
    };
  }, [isAuthenticated, isLoading, isMockUser, userRole, userId]);

  // Subscribe to events
  const subscribe = useCallback(<T,>(eventName: string, callback: (data: T) => void) => {
    const connection = connectionRef.current;
    if (!connection) {
      console.warn('[SignalR] Cannot subscribe - no connection');
      return () => {};
    }

    // Add to local subscribers
    if (!subscribersRef.current.has(eventName)) {
      subscribersRef.current.set(eventName, new Set());

      // Register handler with SignalR
      connection.on(eventName, (data: T) => {
        console.log(`[SignalR] Received ${eventName}:`, data);
        const subscribers = subscribersRef.current.get(eventName);
        subscribers?.forEach((cb) => cb(data));
      });
    }

    const subscribers = subscribersRef.current.get(eventName)!;
    subscribers.add(callback as (data: unknown) => void);

    // Return unsubscribe function
    return () => {
      subscribers.delete(callback as (data: unknown) => void);
      if (subscribers.size === 0) {
        connection.off(eventName);
        subscribersRef.current.delete(eventName);
      }
    };
  }, []);

  // Join a group
  const joinGroup = useCallback(async (groupName: string) => {
    const connection = connectionRef.current;
    if (!connection || connectionState !== 'Connected') {
      console.warn('[SignalR] Cannot join group - not connected');
      return;
    }

    try {
      await connection.invoke('JoinGroup', groupName);
      console.log(`[SignalR] Joined group: ${groupName}`);
    } catch (error) {
      console.error(`[SignalR] Failed to join group ${groupName}:`, error);
    }
  }, [connectionState]);

  // Leave a group
  const leaveGroup = useCallback(async (groupName: string) => {
    const connection = connectionRef.current;
    if (!connection || connectionState !== 'Connected') {
      return;
    }

    try {
      await connection.invoke('LeaveGroup', groupName);
      console.log(`[SignalR] Left group: ${groupName}`);
    } catch (error) {
      console.error(`[SignalR] Failed to leave group ${groupName}:`, error);
    }
  }, [connectionState]);

  // Subscribe to vendor updates
  const subscribeToVendor = useCallback(async (vendorId: string) => {
    const connection = connectionRef.current;
    if (!connection || connectionState !== 'Connected') {
      console.warn('[SignalR] Cannot subscribe to vendor - not connected');
      return;
    }

    try {
      await connection.invoke('SubscribeToVendor', vendorId);
      console.log(`[SignalR] Subscribed to vendor: ${vendorId}`);
    } catch (error) {
      console.error(`[SignalR] Failed to subscribe to vendor ${vendorId}:`, error);
    }
  }, [connectionState]);

  // Unsubscribe from vendor updates
  const unsubscribeFromVendor = useCallback(async (vendorId: string) => {
    const connection = connectionRef.current;
    if (!connection || connectionState !== 'Connected') {
      return;
    }

    try {
      await connection.invoke('UnsubscribeFromVendor', vendorId);
      console.log(`[SignalR] Unsubscribed from vendor: ${vendorId}`);
    } catch (error) {
      console.error(`[SignalR] Failed to unsubscribe from vendor ${vendorId}:`, error);
    }
  }, [connectionState]);

  const value: SignalRContextType = {
    connectionState,
    subscribe,
    joinGroup,
    leaveGroup,
    subscribeToVendor,
    unsubscribeFromVendor
  };

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  );
};

// Hook to use SignalR context
export const useSignalRContext = (): SignalRContextType => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalRContext must be used within a SignalRProvider');
  }
  return context;
};

export default SignalRContext;
