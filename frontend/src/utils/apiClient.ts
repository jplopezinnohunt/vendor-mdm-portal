import axios, { AxiosError } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5001';

// Create axios instance with interceptors
const apiClient = axios.create({
    baseURL: API_BASE_URL,
});

// Request interceptor - add auth token
apiClient.interceptors.request.use(
    (config) => {
        // Use 'localToken' to match AuthContext storage key
        const token = localStorage.getItem('localToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor - handle token expiration
apiClient.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
        // Check if token expired
        if (error.response?.status === 401) {
            const tokenExpired = error.response.headers['token-expired'];

            if (tokenExpired === 'true') {
                // Token expired - clear auth and redirect to login
                localStorage.removeItem('localToken');
                localStorage.removeItem('mockUser');
                localStorage.removeItem('sessionTimestamp');

                // Show session expired message
                alert('Your session has expired. Please log in again.');

                // Redirect to login
                window.location.href = '/login';
            }
        }

        return Promise.reject(error);
    }
);

export default apiClient;
