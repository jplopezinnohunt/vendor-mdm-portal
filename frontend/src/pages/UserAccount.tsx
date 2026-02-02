import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Card, Button, Input } from '../components/ui/Elements';
import axios from 'axios';
import { Lock, ShieldCheck, AlertCircle, CheckCircle } from 'lucide-react';

export const UserAccount: React.FC = () => {
    const { user } = useAuth();
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [status, setStatus] = useState<{ type: 'success' | 'error', message: string } | null>(null);
    const [loading, setLoading] = useState(false);

    // Show loading state if user is not yet loaded
    if (!user) {
        return (
            <div className="flex h-96 items-center justify-center">
                <div className="text-center space-y-3">
                    <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-600 border-t-transparent mx-auto"></div>
                    <p className="text-sm text-gray-500">Loading account information...</p>
                </div>
            </div>
        );
    }

    const handleChangePassword = async (e: React.FormEvent) => {
        e.preventDefault();
        setStatus(null);

        if (newPassword !== confirmPassword) {
            setStatus({ type: 'error', message: "New passwords do not match." });
            return;
        }

        if (newPassword.length < 8) {
            setStatus({ type: 'error', message: "Password must be at least 8 characters long." });
            return;
        }

        setLoading(true);
        try {
            await axios.post('/api/auth/change-password', {
                oldPassword,
                newPassword
            });
            setStatus({ type: 'success', message: "Password updated successfully." });
            setOldPassword('');
            setNewPassword('');
            setConfirmPassword('');
        } catch (error: any) {
            setStatus({
                type: 'error',
                message: error.response?.data?.message || "Failed to update password."
            });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="space-y-6 max-w-4xl mx-auto">
            <div className="flex items-center space-x-4">
                <div className="h-12 w-12 rounded-full bg-brand-100 flex items-center justify-center text-brand-600 font-bold text-xl">
                    {user.name.substring(0, 2).toUpperCase()}
                </div>
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{user.name}</h1>
                    <p className="text-gray-500">{user.email}</p>
                </div>
                <div className="ml-auto">
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {user.role}
                    </span>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                {/* Account Details Card */}
                <Card title="Account Security">
                    <div className="space-y-4">
                        <div className="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                            <ShieldCheck className="w-5 h-5 text-green-600 mt-0.5" />
                            <div>
                                <h3 className="text-sm font-medium text-gray-900">Account Type</h3>
                                <p className="text-xs text-gray-500 mt-1">
                                    You are logged in as <strong>{user.role}</strong>.
                                </p>
                            </div>
                        </div>

                        <div className="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                            <ShieldCheck className="w-5 h-5 text-blue-600 mt-0.5" />
                            <div>
                                <h3 className="text-sm font-medium text-gray-900">User ID</h3>
                                <p className="text-xs text-gray-500 mt-1 font-mono">
                                    {user.id}
                                </p>
                            </div>
                        </div>
                    </div>
                </Card>

                {/* Change Password Card */}
                <Card title="Change Password">
                    <form onSubmit={handleChangePassword} className="space-y-4">
                        {status && (
                            <div className={`p-3 rounded-md flex items-center gap-2 text-sm ${status.type === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}>
                                {status.type === 'success' ? <CheckCircle className="w-4 h-4" /> : <AlertCircle className="w-4 h-4" />}
                                {status.message}
                            </div>
                        )}

                        <Input
                            label="Current Password"
                            type="password"
                            value={oldPassword}
                            onChange={e => setOldPassword(e.target.value)}
                            required
                        />
                        <Input
                            label="New Password"
                            type="password"
                            value={newPassword}
                            onChange={e => setNewPassword(e.target.value)}
                            required
                            minLength={8}
                        />
                        <Input
                            label="Confirm New Password"
                            type="password"
                            value={confirmPassword}
                            onChange={e => setConfirmPassword(e.target.value)}
                            required
                            minLength={8}
                        />

                        <div className="pt-2">
                            <Button type="submit" className="w-full" disabled={loading}>
                                {loading ? 'Updating...' : 'Update Password'}
                            </Button>
                        </div>
                    </form>
                </Card>
            </div>
        </div>
    );
};
