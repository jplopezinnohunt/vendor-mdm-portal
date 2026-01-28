import React, { useState } from 'react';
import { useAuth, UserRole } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Shield, ChevronDown, ChevronUp, Lock, RefreshCw } from 'lucide-react';
import { version } from '../version';
import axios from 'axios';
import { Input, Button } from '../components/ui/Elements';

export const Login = () => {
  const { login, mockLogin, loginLocal } = useAuth();
  const navigate = useNavigate();
  const [showMockOptions, setShowMockOptions] = useState(false);
  const [showLocalLogin, setShowLocalLogin] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const [loginStep, setLoginStep] = useState<'email' | 'password' | '2fa' | 'magicLinkSent'>('email');

  // Local Auth State
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [error, setError] = useState('');

  const handleAzureLogin = async () => {
    setIsLoading(true);
    try {
      await login();
    } catch (error) {
      console.error('Login error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleLocalLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      if (loginStep === 'email') {
        // Step 1: Discover Auth Method
        const res = await axios.post('/api/auth/lookup', { email });
        const method = res.data.authMethod || 'MagicLink';

        if (method === 'LocalStrong') {
          setLoginStep('password');
          setIsLoading(false);
        } else {
          // Default to Magic Link for everything else (including 'MagicLink', 'AzureAd' fallback, or unknown)
          await axios.post('/api/auth/magic-link', { email });
          setLoginStep('magicLinkSent');
          setIsLoading(false);
        }
        return;
      }

      if (loginStep === 'password') {
        // Step 2: Password Login (for Admins)
        const response = await axios.post('/api/auth/login-local', { email, password });
        if (response.data.require2fa) {
          setLoginStep('2fa');
          setIsLoading(false);
          return;
        }
        const { token, user } = response.data;
        await loginLocal(token, user);
        navigate('/admin/dashboard');
        return;
      }

      if (loginStep === '2fa') {
        // Step 3: Verify 2FA
        const response = await axios.post('/api/auth/login-2fa', { email, code: totpCode });
        const { token, user } = response.data;
        await loginLocal(token, user);
        navigate('/admin/dashboard');
      }

    } catch (err: any) {
      console.error("Login process failed", err);
      // If password fail, just show error
      if (loginStep === 'password') {
        setError("Invalid password. Please try again.");
      } else if (loginStep === '2fa') {
        setError("Invalid code.");
      } else {
        // If lookup failed (e.g. 500), mostly just default to Magic Link sent to be generic ?
        // Or show error
        setError("Unable to proceed. Please check your email or try again later.");
      }
      setIsLoading(false);
    }
  };

  const handleMockLogin = async (role: UserRole) => {
    setIsLoading(true);
    try {
      await mockLogin(role);

      // Navigate based on role
      switch (role) {
        case 'Admin':
          navigate('/admin/dashboard');
          break;
        case 'VendorUnit':
        case 'BFM':
        case 'Requestor':
          navigate('/approver/worklist');
          break;
        case 'Vendor':
        default:
          navigate('/profile');
          break;
      }
    } catch (error) {
      console.error('Mock login error:', error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50">
      <div className="w-full max-w-md">
        {/* Main Login Card */}
        <div className="bg-white rounded-2xl shadow-xl p-8 space-y-6">
          {/* Header */}
          <div className="text-center space-y-2">
            <h1 className="text-3xl font-bold text-gray-900">Vendor Portal</h1>
            <p className="text-gray-600">Secure access for Suppliers and Staff</p>
          </div>

          {!showLocalLogin ? (
            <>
              {/* Azure AD Login Button */}
              <button
                onClick={handleAzureLogin}
                disabled={isLoading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 px-6 rounded-lg transition-colors duration-200 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Sign in with UNESCO Account
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>

              <div className="text-center">
                <button onClick={() => setShowLocalLogin(true)} className="text-sm text-brand-600 hover:underline">
                  Log in with Email or Username (Vendors/Local Admin)
                </button>
              </div>
            </>
          ) : (
            <form onSubmit={handleLocalLogin} className="space-y-4">
              {loginStep === 'email' && (
                <>
                  <Input label="Email or Username" type="text" value={email} onChange={e => setEmail(e.target.value)} required autoFocus />
                  <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading ? 'Checking...' : 'Continue'}
                  </Button>
                </>
              )}

              {loginStep === 'password' && (
                <>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium text-gray-700">{email}</span>
                    <button type="button" onClick={() => setLoginStep('email')} className="text-xs text-blue-600 hover:underline">Change</button>
                  </div>
                  <Input label="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} required autoFocus />
                  <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading ? 'Signing in...' : 'Sign in'}
                  </Button>
                </>
              )}

              {loginStep === 'magicLinkSent' && (
                <div className="text-center space-y-4">
                  <div className="bg-green-50 p-4 rounded-full w-16 h-16 flex items-center justify-center mx-auto text-green-600">
                    <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
                  </div>
                  <h3 className="text-lg font-medium text-gray-900">Check your email</h3>
                  <p className="text-sm text-gray-600">We sent a magic login link to <strong>{email}</strong>.</p>
                  <p className="text-xs text-gray-500">Click the link in the email to sign in.</p>
                  <Button type="button" variant="secondary" className="w-full" onClick={() => setLoginStep('email')}>
                    Back to Login
                  </Button>
                </div>
              )}

              {loginStep === '2fa' && (
                <div className="text-center space-y-4">
                  <div className="bg-blue-50 p-4 rounded-full w-16 h-16 flex items-center justify-center mx-auto text-blue-600">
                    <Lock className="w-8 h-8" />
                  </div>
                  <h3 className="text-lg font-medium">Two-Factor Authentication</h3>
                  <p className="text-sm text-gray-500">Enter the code from your authenticator app.</p>
                  <Input
                    label="6-Digit Code"
                    className="text-center text-xl tracking-widest font-mono"
                    value={totpCode}
                    onChange={e => setTotpCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                    autoFocus
                    required
                  />
                  <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading ? 'Verifying...' : 'Verify & Login'}
                  </Button>
                </div>
              )}

              {error && <div className="text-red-500 text-sm text-center bg-red-50 p-2 rounded">{error}</div>}

              <div className="text-center">
                <button type="button" onClick={() => setShowLocalLogin(false)} className="text-sm text-gray-500 hover:text-gray-800">
                  Back to SSO Login
                </button>
              </div>
            </form>
          )}

          {/* Divider */}
          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-200"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-4 bg-white text-gray-500">Trusted by</span>
            </div>
          </div>

          {/* Azure AD Badge */}
          <div className="flex items-center justify-center gap-2 text-gray-500 text-sm">
            <Shield className="w-4 h-4" />
            <span className="font-medium">AZURE ACTIVE DIRECTORY</span>
          </div>

          {/* Mock Login Options */}
          <div className="pt-4 border-t border-gray-200">
            <button
              onClick={() => setShowMockOptions(!showMockOptions)}
              className="w-full flex items-center justify-center gap-2 text-sm text-gray-600 hover:text-gray-900 transition-colors"
            >
              <span className="font-medium">Sign-in options</span>
              {showMockOptions ? (
                <ChevronUp className="w-4 h-4" />
              ) : (
                <ChevronDown className="w-4 h-4" />
              )}
            </button>

            {showMockOptions && (
              <div className="mt-4 space-y-2 p-4 bg-gray-50 rounded-lg border border-gray-200">
                <div className="text-xs text-gray-500 mb-3 flex items-center gap-1">
                  <span className="px-2 py-0.5 bg-yellow-100 text-yellow-800 rounded font-medium">DEV MODE</span>
                  <span>Test different roles</span>
                </div>

                <button
                  onClick={() => handleMockLogin('Vendor')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50 text-sm"
                >
                  Sign in as Vendor
                </button>

                <button
                  onClick={() => handleMockLogin('Requestor')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50 text-sm"
                >
                  Sign in as Requestor
                </button>

                <button
                  onClick={() => handleMockLogin('VendorUnit')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50 text-sm"
                >
                  Sign in as Vendor Unit Approver
                </button>

                <button
                  onClick={() => handleMockLogin('BFM')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50 text-sm"
                >
                  Sign in as BFM Approver
                </button>

                <button
                  onClick={() => handleMockLogin('Admin')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50 text-sm"
                >
                  Sign in as Administrator
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="mt-6 text-center text-xs text-gray-500">
          {version.version} • Built on {new Date(version.buildDate).toLocaleString()} • {version.commitHash} ({version.branch})
        </div>
      </div>
    </div>
  );
};