import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { ShieldCheck, ArrowRight, Sparkles, Building2 } from 'lucide-react';
import { VersionInfo } from '../components/VersionInfo';

export const Login: React.FC = () => {
  const { login, isAuthenticated, isLoading } = useAuth();
  const navigate = useNavigate();
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  React.useEffect(() => {
    if (isAuthenticated) {
      navigate('/profile');
    }
  }, [isAuthenticated, navigate]);

  const handleLogin = async () => {
    setIsLoggingIn(true);
    try {
      await login();
      // Redirect will happen automatically or via useEffect
    } catch (error) {
      console.error("Login failed", error);
      setIsLoggingIn(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen relative overflow-hidden bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      {/* Background Elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute top-0 left-0 w-full h-full bg-white opacity-40"></div>
        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-blue-400 mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
        <div className="absolute top-24 right-0 w-96 h-96 rounded-full bg-purple-400 mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
      </div>

      <div className="relative sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-white/80 backdrop-blur-lg py-8 px-4 shadow-2xl sm:rounded-2xl sm:px-10 border border-white/50">

          <div className="sm:mx-auto sm:w-full sm:max-w-md text-center mb-8">
            <div className="inline-flex items-center justify-center p-3 bg-gradient-to-r from-blue-600 to-indigo-600 rounded-xl shadow-lg mb-4">
              <Building2 className="h-8 w-8 text-white" />
            </div>
            <h2 className="text-3xl font-extrabold text-gray-900 tracking-tight">
              Vendor Portal
            </h2>
            <p className="mt-2 text-sm text-gray-600">
              Secure access for Suppliers and Staff
            </p>
          </div>
          .animate-blob {animation: blob 7s infinite; }
          .animation-delay-2000 {animation - delay: 2s; }
      `}</style>
      </div>
      );
};