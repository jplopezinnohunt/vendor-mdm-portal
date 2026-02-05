import '@testing-library/jest-dom';
import { vi } from 'vitest';

// Mock window.matchMedia for components that use media queries
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock IntersectionObserver
global.IntersectionObserver = vi.fn().mockImplementation(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

// Mock ResizeObserver
global.ResizeObserver = vi.fn().mockImplementation(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

// Mock MSAL Browser
vi.mock('@azure/msal-browser', () => ({
  PublicClientApplication: vi.fn().mockImplementation(() => ({
    initialize: vi.fn().mockResolvedValue(undefined),
    loginPopup: vi.fn().mockResolvedValue({ account: null }),
    loginRedirect: vi.fn().mockResolvedValue(undefined),
    logout: vi.fn().mockResolvedValue(undefined),
    logoutPopup: vi.fn().mockResolvedValue(undefined),
    acquireTokenSilent: vi.fn().mockResolvedValue({ accessToken: 'mock-token' }),
    acquireTokenPopup: vi.fn().mockResolvedValue({ accessToken: 'mock-token' }),
    getAllAccounts: vi.fn().mockReturnValue([]),
    getAccountByHomeId: vi.fn().mockReturnValue(null),
    setActiveAccount: vi.fn(),
    getActiveAccount: vi.fn().mockReturnValue(null),
    handleRedirectPromise: vi.fn().mockResolvedValue(null),
    addEventCallback: vi.fn().mockReturnValue('callback-id'),
    removeEventCallback: vi.fn(),
  })),
  InteractionStatus: {
    None: 'none',
    Login: 'login',
    Logout: 'logout',
    AcquireToken: 'acquireToken',
    SsoSilent: 'ssoSilent',
    HandleRedirect: 'handleRedirect',
    Startup: 'startup',
  },
  EventType: {
    LOGIN_SUCCESS: 'msal:loginSuccess',
    LOGIN_FAILURE: 'msal:loginFailure',
    LOGOUT_SUCCESS: 'msal:logoutSuccess',
    ACQUIRE_TOKEN_SUCCESS: 'msal:acquireTokenSuccess',
  },
  AccountInfo: {},
  AuthenticationResult: {},
}));

// Mock MSAL React
const mockMsalInstance = {
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
    instance: mockMsalInstance,
    accounts: [],
    inProgress: 'none',
  }),
  useIsAuthenticated: () => false,
  useAccount: () => null,
}));

// Mock SignalR - must be a class that returns chainable methods
const mockConnection = {
  start: vi.fn().mockResolvedValue(undefined),
  stop: vi.fn().mockResolvedValue(undefined),
  on: vi.fn(),
  off: vi.fn(),
  invoke: vi.fn().mockResolvedValue(undefined),
  state: 'Connected',
  onclose: vi.fn(),
  onreconnecting: vi.fn(),
  onreconnected: vi.fn(),
};

class MockHubConnectionBuilder {
  withUrl() { return this; }
  withAutomaticReconnect() { return this; }
  configureLogging() { return this; }
  build() { return mockConnection; }
}

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: MockHubConnectionBuilder,
  HubConnectionState: {
    Disconnected: 'Disconnected',
    Connecting: 'Connecting',
    Connected: 'Connected',
    Disconnecting: 'Disconnecting',
    Reconnecting: 'Reconnecting',
  },
  HttpTransportType: {
    None: 0,
    WebSockets: 1,
    ServerSentEvents: 2,
    LongPolling: 4,
  },
  LogLevel: {
    None: 0,
    Critical: 1,
    Error: 2,
    Warning: 3,
    Information: 4,
    Debug: 5,
    Trace: 6,
  },
}));

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: (key: string) => store[key] || null,
    setItem: (key: string, value: string) => {
      store[key] = value;
    },
    removeItem: (key: string) => {
      delete store[key];
    },
    clear: () => {
      store = {};
    },
    get length() {
      return Object.keys(store).length;
    },
    key: (index: number) => Object.keys(store)[index] || null,
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

// Mock sessionStorage
Object.defineProperty(window, 'sessionStorage', {
  value: localStorageMock,
});

// Suppress console output in tests to prevent memory issues from verbose logging
// Comment out these lines for debugging specific test failures
vi.spyOn(console, 'log').mockImplementation(() => {});
vi.spyOn(console, 'warn').mockImplementation(() => {});
vi.spyOn(console, 'info').mockImplementation(() => {});
