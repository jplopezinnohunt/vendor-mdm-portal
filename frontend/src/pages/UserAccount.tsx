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

    if (!user) return null;

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
                    {user.username.substring(0, 2).toUpperCase()}
                </div>
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{user.username}</h1>
                    <p className="text-gray-500">{user.email}</p>
                </div>
                <div className="ml-auto">
                    <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-medium 
                        ${user.authMethod === 'LocalStrong' ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800'}`}>
                        {user.authMethod === 'LocalStrong' ? 'Password Protected' : user.authMethod || 'SSO'}
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
                                <h3 className="text-sm font-medium text-gray-900">Authentication Method</h3>
                                <p className="text-xs text-gray-500 mt-1">
                                    You are currently logged in using <strong>{user.authMethod}</strong>.
                                </p>
                            </div>
                        </div>

                        {user.roles && user.roles.length > 0 && (
                            <div className="flex items-start space-x-3 p-3 bg-gray-50 rounded-lg">
                                <ShieldCheck className="w-5 h-5 text-blue-600 mt-0.5" />
                                <div>
                                    <h3 className="text-sm font-medium text-gray-900">Assigned Roles</h3>
                                    <div className="flex flex-wrap gap-2 mt-2">
                                        {user.roles.map(r => (
                                            <span key={r} className="px-2 py-0.5 bg-white border border-gray-200 rounded text-xs text-gray-600">
                                                {r}
                                            </span>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </Card>

                {/* Change Password Card */}
                <Card title="Change Password">
                    {user.authMethod === 'LocalStrong' ? (
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
                    ) : (
                        <div className="text-center py-8 space-y-3">
                            <div className="bg-gray-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto text-gray-400">
                                <Lock className="w-6 h-6" />
                            </div>
                            <h3 className="text-sm font-medium text-gray-900">Managed Externally</h3>
                            <p className="text-xs text-gray-500 max-w-xs mx-auto">
                                Your account is managed by an external identity provider (Azure AD) or Magic Link.
                                Please contact your administrator to change your password.
                            </p>
                        </div>
                    )}
                </Card>
            </div>
        </div>
    );
};
