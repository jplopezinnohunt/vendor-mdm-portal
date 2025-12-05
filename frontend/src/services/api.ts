import axios from 'axios';

// In Azure Static Web Apps, /api routes automatically to the backend
// For local dev, we point to the Functions emulator
const API_BASE_URL = import.meta.env.DEV ? 'http://localhost:7071/api' : '/api';

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
    // Standard error handling, e.g., redirect to login on 401
    return Promise.reject(error);
  }
);