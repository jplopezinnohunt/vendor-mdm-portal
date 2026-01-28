import { api } from './api';

export interface User {
    id: string;
    username: string;
    email: string;
    roles: string[];
    authProvider: string;
    authMethod?: string; // "MagicLink" | "LocalStrong" | "AzureAd"
    azureAdObjectId?: string;
    lastLogonAt?: string;
    isBlocked: boolean;
    createdAt: string;
    status: string; // "Pending" | "Active"
}

export interface CreateUserRequest {
    username: string;
    email: string;
    roles: string[];
    password?: string; // Optional for local users
}

export const UserService = {
    getAllUsers: async (): Promise<User[]> => {
        const response = await api.get('/user');
        return response.data;
    },

    createUser: async (user: CreateUserRequest): Promise<User> => {
        const response = await api.post('/user', user);
        return response.data;
    },

    inviteUser: async (email: string, roles: string[], authMethod?: string): Promise<void> => {
        await api.post('/auth/invite', { email, roles, authMethod });
    },

    resendInvite: async (email: string): Promise<void> => {
        await api.post('/auth/resend-invite', { email });
    },

    updateUserRoles: async (userId: string, roles: string[], authMethod?: string): Promise<void> => {
        await api.put(`/user/${userId}/roles`, { roles, authMethod });
    },

    updateBlockStatus: async (userId: string, isBlocked: boolean): Promise<void> => {
        await api.put(`/user/${userId}/block`, { isBlocked });
    }
};
