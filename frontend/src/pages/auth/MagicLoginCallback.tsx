import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../../context/AuthContext';
import { Card, Button } from '../../components/ui/Elements';
import { RefreshCw, CheckCircle, AlertCircle } from 'lucide-react';

export const MagicLoginCallback: React.FC = () => {
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');
    const navigate = useNavigate();
    const { loginLocal } = useAuth();

    const [status, setStatus] = useState<'verifying' | 'success' | 'error'>('verifying');
    const [error, setError] = useState('');

    const verificationAttempted = React.useRef(false);

    useEffect(() => {
        if (!token) {
            setStatus('error');
            setError('No token provided in the link.');
            return;
        }

        if (verificationAttempted.current) return;
        verificationAttempted.current = true;

        const verifyToken = async () => {
            try {
                const response = await axios.post('/api/auth/verify-magic-link', { token });
                const { token: jwt, user } = response.data;

                await loginLocal(jwt, user);
                setStatus('success');

                // Redirect after short delay
                setTimeout(() => {
                    const roles = user.roles || [];
                    if (roles.includes('Admin')) {
                        navigate('/admin/dashboard');
                    } else if (roles.some((r: string) => ['Requestor', 'Requester', 'VendorUnit', 'BFM', 'Approver', 'Viewer'].includes(r))) {
                        navigate('/approver/worklist');
                    } else {
                        navigate('/profile'); // Vendor default
                    }
                }, 1500);

            } catch (err: any) {
                console.error('Magic Link Verification Failed:', err);
                setStatus('error');
                setError(err.response?.data || 'Link is invalid or has expired.');
            }
        };

        verifyToken();
    }, [token, loginLocal, navigate]);

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
            <Card className="w-full max-w-md text-center p-8">
                {status === 'verifying' && (
                    <div className="space-y-4">
                        <RefreshCw className="h-12 w-12 text-blue-600 animate-spin mx-auto" />
                        <h2 className="text-xl font-semibold text-gray-900">Verifying your link...</h2>
                        <p className="text-gray-500">Please wait while we log you in.</p>
                    </div>
                )}

                {status === 'success' && (
                    <div className="space-y-4">
                        <CheckCircle className="h-12 w-12 text-green-600 mx-auto" />
                        <h2 className="text-xl font-semibold text-gray-900">Login Successful!</h2>
                        <p className="text-gray-500">Redirecting you to the portal...</p>
                    </div>
                )}

                {status === 'error' && (
                    <div className="space-y-4">
                        <AlertCircle className="h-12 w-12 text-red-600 mx-auto" />
                        <h2 className="text-xl font-semibold text-gray-900">Login Failed</h2>
                        <p className="text-red-600 bg-red-50 p-3 rounded-md text-sm">{error}</p>
                        <Button onClick={() => navigate('/login')} className="w-full">
                            Back to Login
                        </Button>
                    </div>
                )}
            </Card>
        </div>
    );
};
