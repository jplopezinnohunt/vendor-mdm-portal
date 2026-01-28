import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Card, Button, Input } from '../../components/ui/Elements';
import QRCode from 'react-qr-code';
import axios from 'axios';

// Move to a proper service in a real app
const api = axios.create({ baseURL: '/api' });

export const InvitationPage: React.FC = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const token = searchParams.get('token');

    const [step, setStep] = useState<'validate' | 'password' | '2fa' | 'success'>('validate');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [qrValue, setQrValue] = useState('');
    const [totpCode, setTotpCode] = useState('');
    const [error, setError] = useState('');

    useEffect(() => {
        if (!token) {
            setError('Missing invitation token');
            return;
        }

        // Validate Token
        api.get(`/auth/validate-invite/${token}`)
            .then(res => {
                setEmail(res.data.email);
                setStep('password');
            })
            .catch(() => setError('Invalid or expired invitation token'));
    }, [token]);

    const handlePasswordSubmit = async () => {
        if (password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }
        if (password.length < 8) {
            setError('Password must be at least 8 characters');
            return;
        }

        try {
            // Setup Password & Request 2FA Secret
            const res = await api.post('/auth/complete-setup', { token, password });
            setQrValue(res.data.qrCode);
            setStep('2fa');
            setError('');
        } catch (err) {
            setError('Failed to setup password');
        }
    };

    const handleVerify2fa = async () => {
        try {
            const res = await api.post('/auth/verify-2fa-setup', { token, code: totpCode });
            // Save token and redirect
            localStorage.setItem('token', res.data.token);
            setStep('success');
        } catch (err) {
            setError('Invalid 2FA Code. Please try again.');
        }
    };

    if (error && step === 'validate') {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50">
                <Card className="w-full max-w-md p-8 text-center">
                    <h2 className="text-xl font-bold text-red-600 mb-4">Invitation Error</h2>
                    <p className="text-gray-600">{error}</p>
                </Card>
            </div>
        );
    }

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50 p-4">
            <Card className="w-full max-w-md p-6">
                <div className="text-center mb-8">
                    <h1 className="text-2xl font-bold text-gray-900">Vendor Portal Setup</h1>
                    <p className="text-sm text-gray-500">{email}</p>
                </div>

                {error && (
                    <div className="mb-4 bg-red-50 text-red-700 p-3 rounded-md text-sm">
                        {error}
                    </div>
                )}

                {step === 'password' && (
                    <div className="space-y-4">
                        <h3 className="text-lg font-medium">Step 1: Set Password</h3>
                        <Input
                            label="Password"
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                        <Input
                            label="Confirm Password"
                            type="password"
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                        <Button className="w-full" onClick={handlePasswordSubmit}>Continue</Button>
                    </div>
                )}

                {step === '2fa' && (
                    <div className="space-y-6 text-center">
                        <h3 className="text-lg font-medium">Step 2: Setup 2FA</h3>
                        <p className="text-sm text-gray-600 px-4">
                            Scan this QR code with your Authenticator App (Google Authenticator, Microsoft Authenticator).
                        </p>

                        <div className="flex justify-center p-4 bg-white rounded-lg border">
                            <QRCode value={qrValue} size={200} />
                        </div>

                        <div>
                            <Input
                                label="Enter 6-digit Code"
                                placeholder="000 000"
                                className="text-center text-lg tracking-widest"
                                value={totpCode}
                                onChange={(e) => setTotpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                            />
                        </div>

                        <Button className="w-full" onClick={handleVerify2fa} disabled={totpCode.length !== 6}>
                            Verify & Complete
                        </Button>
                    </div>
                )}

                {step === 'success' && (
                    <div className="text-center space-y-4">
                        <div className="h-12 w-12 bg-green-100 rounded-full flex items-center justify-center mx-auto">
                            <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-medium text-green-900">Setup Complete!</h3>
                        <p className="text-gray-500">Your account is active and 2FA is enabled.</p>
                        <Button className="w-full" onClick={() => navigate('/admin')}>
                            Go to Dashboard
                        </Button>
                    </div>
                )}
            </Card>
        </div>
    );
};
