// Service status helper to detect if backend API is reachable
import { api } from './api';

export const statusService = {
    /**
     * Checks backend health endpoint. Returns 'online' if reachable, otherwise 'mock'.
     */
    checkBackendStatus: async (): Promise<'online' | 'mock'> => {
        try {
            // Assuming a simple health endpoint exists; if not, this will fail and fallback to mock.
            await api.get('/health');
            return 'online';
        } catch {
            return 'mock';
        }
    },
};
