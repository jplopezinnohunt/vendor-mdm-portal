import axios from 'axios';

// In Azure Static Web Apps or when using Vite proxy, /api routes to the backend
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor to add Auth Token (Simulated for now, would use MSAL in prod)
// Interceptor to add Auth Token (Simulated for now, would use MSAL in prod)
api.interceptors.request.use((config) => {
  // Check for mock user in localStorage
  const mockUserJson = localStorage.getItem('mockUser');
  if (mockUserJson) {
    try {
      const mockUser = JSON.parse(mockUserJson);
      if (mockUser && mockUser.role) {
        config.headers['X-Mock-User'] = mockUser.role;
      }
    } catch (e) {
      console.error('Failed to parse mock user for header', e);
    }
  } else if (import.meta.env.DEV) {
    // Fallback for Local Dev: If no mock user set (e.g. using Azure Login button locally), 
    // default to Admin to allow Backend access.
    console.warn('Dev Mode: Injecting fallback X-Mock-User: Admin');
    config.headers['X-Mock-User'] = 'Admin';
  }

  // const token = await msalInstance.acquireTokenSilent(...)
  // config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Enhanced error handling
    if (error.code === 'ECONNREFUSED' || error.message?.includes('Network Error')) {
      console.error('Backend API is not reachable. Is it running on', API_BASE_URL, '?');
      error.userMessage = 'Cannot connect to backend API. Please ensure the backend is running.';
    } else if (error.response) {
      // Server responded with error status
      const status = error.response.status;
      if (status === 401) {
        // Could redirect to login here
        error.userMessage = 'Authentication required. Please log in again.';
      } else if (status === 403) {
        error.userMessage = 'Access denied. You do not have permission to perform this action.';
        // Optional: Redirect to /401 page with 403 context if not handled by component
        // window.location.href = '/401'; // losing state.
        error.isUnauthorized = true;
        error.authDetails = {
          status: 403,
          title: 'Forbidden',
          message: 'You have valid credentials but lack the permissions required for this resource.'
        };
      } else if (status === 404) {
        error.userMessage = 'API endpoint not found. The backend may not be fully configured.';
      } else if (status >= 500) {
        // Redirect to 500 page with details
        const errorData = error.response?.data || {};
        const navigateState = {
          status: status,
          message: errorData.message || errorData.detail || errorData.title || 'Internal Server Error',
          detail: errorData.detail,
          stack: errorData.stack,
          originalError: errorData
        };

        // Use window.location for now as we are outside React Context, 
        // or prefer throwing to be caught by UI if possible.
        // But for a global "Crash" page, simple redirection works.
        // Ideally we'd use a history object if available.
        // For now, we'll attach the userMessage for components that catch it, 
        // AND trigger the redirect if it's a critical failure.

        error.userMessage = 'Server error. Redirecting to error page...';

        // Note: react-router's navigate isn't easily accessible outside components.
        // We will store this in a global event or sessionStorage, or simpler:
        // Use a small helper or just rely on the component handling it.
        // BUT the user asked for a PAGE.

        // Let's use a CustomEvent so App.tsx or a layout can listen and navigate?
        // Or simpler: Just re-throw and let the Component (InviteVendorForm) handle the navigation 
        // if we want to preserve form state? 
        // The user said "The 500 error page should have details".

        // Let's try to retain the "Alert" behavior for now in the Code 
        // BUT also offer a link? 
        // No, the user IMPLIES they want to land on a page.

        // Since we can't use useNavigate here easily without setup,
        // we will stick to the previous plan: InviteVendorForm handles the error.

        // WAIT. I can modify InviteVendorForm to Navigate to /500 on error!
        // That is cleaner than hacking api.ts with window.location (which loses state).

        error.isServerError = true;
        error.serverDetails = navigateState;
      }
    }
    return Promise.reject(error);
  }
);