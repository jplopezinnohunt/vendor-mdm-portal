import axios from 'axios';

// In Azure Static Web Apps, /api routes automatically to the backend
// For local dev, point to the ASP.NET Core API (port 5001 - 5000 is used by AirPlay on macOS)
// Can be overridden with VITE_API_BASE_URL environment variable
const API_BASE_URL = import.meta.env.DEV 
  ? (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001/api')
  : '/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor to add Auth Token (Simulated for now, would use MSAL in prod)
api.interceptors.request.use((config) => {
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
      } else if (status === 404) {
        error.userMessage = 'API endpoint not found. The backend may not be fully configured.';
      } else if (status >= 500) {
        error.userMessage = 'Server error. Please try again later.';
      }
    }
    return Promise.reject(error);
  }
);