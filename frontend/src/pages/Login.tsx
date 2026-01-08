import React, { useState } from 'react';
import { useAuth, UserRole } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Shield, ChevronDown, ChevronUp } from 'lucide-react';

export const Login = () => {
  const { login, mockLogin } = useAuth();
  const navigate = useNavigate();
  const [showMockOptions, setShowMockOptions] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

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

  const handleMockLogin = async (role: UserRole) => {
    setIsLoading(true);
    try {
      await mockLogin(role);

      // Navigate based on role
      switch (role) {
        case 'Admin':
          navigate('/admin/dashboard');
          break;
        case 'Approver':
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
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50"
                >
                  Sign in as Vendor
                </button>

                <button
                  onClick={() => handleMockLogin('Approver')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50"
                >
                  Sign in as Approver
                </button>

                <button
                  onClick={() => handleMockLogin('Admin')}
                  disabled={isLoading}
                  className="w-full bg-white hover:bg-gray-50 text-gray-700 font-medium py-2 px-4 rounded border border-gray-300 transition-colors disabled:opacity-50"
                >
                  Sign in as Administrator
                </button>
              </div>
            )}
          </div>
        </div>

        {/* Footer */}
        <div className="mt-6 text-center text-xs text-gray-500">
          v164 • Built on Jan 8, 2026, 01:10 PM • 961f3e8
        </div>
      </div>
    </div>
  );
};