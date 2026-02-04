import { useEffect, useCallback, useState } from 'react';
import {
  useSignalRContext,
  StatusChangedEvent,
  VendorCreatedEvent,
  NotificationEvent,
  TaskAssignedEvent,
  SapSyncResultEvent,
  ConnectionState
} from '../context/SignalRContext';

/**
 * Hook to subscribe to SignalR events.
 * Automatically handles subscription and cleanup.
 *
 * @example
 * // Subscribe to status changes
 * useSignalREvent<StatusChangedEvent>('StatusChanged', (event) => {
 *   console.log('Status changed:', event);
 * });
 */
export function useSignalREvent<T>(
  eventName: string,
  callback: (data: T) => void,
  deps: React.DependencyList = []
): void {
  const { subscribe } = useSignalRContext();

  useEffect(() => {
    const unsubscribe = subscribe<T>(eventName, callback);
    return () => {
      unsubscribe();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [eventName, subscribe, ...deps]);
}

/**
 * Hook to get the SignalR connection state.
 *
 * @example
 * const connectionState = useSignalRConnectionState();
 * // 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting'
 */
export function useSignalRConnectionState(): ConnectionState {
  const { connectionState } = useSignalRContext();
  return connectionState;
}

/**
 * Hook to subscribe to status change events.
 *
 * @example
 * useStatusChanged((event) => {
 *   toast.info(`${event.entityType} status: ${event.oldStatus} → ${event.newStatus}`);
 * });
 */
export function useStatusChanged(
  callback: (event: StatusChangedEvent) => void,
  deps: React.DependencyList = []
): void {
  useSignalREvent<StatusChangedEvent>('StatusChanged', callback, deps);
}

/**
 * Hook to subscribe to vendor created events.
 */
export function useVendorCreated(
  callback: (event: VendorCreatedEvent) => void,
  deps: React.DependencyList = []
): void {
  useSignalREvent<VendorCreatedEvent>('VendorCreated', callback, deps);
}

/**
 * Hook to subscribe to notification events for the current user.
 */
export function useNotifications(
  callback: (event: NotificationEvent) => void,
  deps: React.DependencyList = []
): void {
  useSignalREvent<NotificationEvent>('Notification', callback, deps);
}

/**
 * Hook to subscribe to task assignment events.
 */
export function useTaskAssigned(
  callback: (event: TaskAssignedEvent) => void,
  deps: React.DependencyList = []
): void {
  useSignalREvent<TaskAssignedEvent>('TaskAssigned', callback, deps);
}

/**
 * Hook to subscribe to SAP sync result events.
 */
export function useSapSyncResult(
  callback: (event: SapSyncResultEvent) => void,
  deps: React.DependencyList = []
): void {
  useSignalREvent<SapSyncResultEvent>('SapSyncResult', callback, deps);
}

/**
 * Hook to subscribe to updates for a specific vendor.
 * Automatically joins/leaves the vendor group.
 *
 * @example
 * useVendorUpdates(vendorId, (event) => {
 *   console.log('Vendor updated:', event);
 * });
 */
export function useVendorUpdates(
  vendorId: string | null,
  callback: (event: StatusChangedEvent) => void,
  deps: React.DependencyList = []
): void {
  const { subscribeToVendor, unsubscribeFromVendor, connectionState } = useSignalRContext();

  // Subscribe/unsubscribe to vendor group
  useEffect(() => {
    if (!vendorId || connectionState !== 'Connected') {
      return;
    }

    subscribeToVendor(vendorId);

    return () => {
      unsubscribeFromVendor(vendorId);
    };
  }, [vendorId, connectionState, subscribeToVendor, unsubscribeFromVendor]);

  // Listen for vendor-specific status changes
  useSignalREvent<StatusChangedEvent>('VendorStatusChanged', callback, deps);
}

/**
 * Hook that returns a list of recent notifications.
 * Automatically manages notification history.
 */
export function useNotificationHistory(maxItems: number = 10): {
  notifications: NotificationEvent[];
  clearNotifications: () => void;
  unreadCount: number;
  markAllRead: () => void;
} {
  const [notifications, setNotifications] = useState<NotificationEvent[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);

  useNotifications((event) => {
    setNotifications((prev) => [event, ...prev].slice(0, maxItems));
    setUnreadCount((prev) => prev + 1);
  });

  const clearNotifications = useCallback(() => {
    setNotifications([]);
    setUnreadCount(0);
  }, []);

  const markAllRead = useCallback(() => {
    setUnreadCount(0);
  }, []);

  return {
    notifications,
    clearNotifications,
    unreadCount,
    markAllRead
  };
}

/**
 * Hook that provides a connection status indicator component props.
 */
export function useConnectionIndicator(): {
  isConnected: boolean;
  isReconnecting: boolean;
  statusText: string;
  statusColor: string;
} {
  const connectionState = useSignalRConnectionState();

  const statusMap = {
    Connected: { text: 'Connected', color: 'green', isConnected: true, isReconnecting: false },
    Connecting: { text: 'Connecting...', color: 'yellow', isConnected: false, isReconnecting: false },
    Reconnecting: { text: 'Reconnecting...', color: 'orange', isConnected: false, isReconnecting: true },
    Disconnected: { text: 'Disconnected', color: 'red', isConnected: false, isReconnecting: false }
  };

  const status = statusMap[connectionState];

  return {
    isConnected: status.isConnected,
    isReconnecting: status.isReconnecting,
    statusText: status.text,
    statusColor: status.color
  };
}

export default useSignalREvent;
