import React, { useState } from 'react';
import { useAuth, UserRole } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import { UserPlus, Building2, ArrowRight, ShieldCheck, UserCog, Sparkles, Database } from 'lucide-react';
import { VersionInfo } from '../components/VersionInfo';

export const Login: React.FC = () => {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [loginRole, setLoginRole] = useState<UserRole | null>(null);

  React.useEffect(() => {
    if (isAuthenticated) navigate('/profile');
  }, [isAuthenticated, navigate]);

  const handleLogin = async (role: UserRole = 'Vendor') => {
    setIsLoggingIn(true);
    setLoginRole(role);

    // Simulates the Azure AD / Corporate ID login flow
    await login(role);

    setIsLoggingIn(false);
    navigate('/profile');
  };

  return (
    <div className="min-h-screen relative overflow-hidden">
      {/* Animated Gradient Background */}
      <div className="absolute inset-0 bg-gradient-animated opacity-10"></div>

      {/* Decorative Elements */}
      <div className="absolute top-0 left-0 w-96 h-96 bg-blue-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
      <div className="absolute top-0 right-0 w-96 h-96 bg-purple-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
      <div className="absolute bottom-0 left-1/2 w-96 h-96 bg-pink-400 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-4000"></div>

      <div className="relative flex flex-col items-center justify-center py-12 px-4 sm:px-6 lg:px-8 min-h-screen">
        {/* Header Section */}
        <div className="text-center max-w-3xl mx-auto mb-12 animate-fade-in">
          <div className="inline-flex items-center justify-center p-2 bg-gradient-to-r from-blue-600 to-purple-600 rounded-2xl mb-6 shadow-lg">
            <Sparkles className="h-8 w-8 text-white" />
          </div>
          <h1 className="text-5xl font-extrabold text-gray-900 tracking-tight sm:text-6xl mb-4">
            <span className="text-gradient">Partner</span> with Us
          </h1>
          <p className="mt-4 text-lg text-gray-600 max-w-2xl mx-auto leading-relaxed">
            Welcome to the centralized vendor management system. Please select an option below to
            proceed with your application or manage your existing profile.
          </p>

          {/* Service Status Indicator */}
          <div className="mt-6 inline-flex items-center gap-2 px-4 py-2 bg-white/80 backdrop-blur-sm rounded-full shadow-md border border-gray-200">
            <div className="flex items-center gap-2">
              <div className="h-2 w-2 bg-yellow-500 rounded-full animate-pulse"></div>
              <span className="text-xs font-medium text-gray-600">Mock Authentication</span>
            </div>
            <span className="text-gray-300">|</span>
            <div className="flex items-center gap-2">
              <Database className="h-3 w-3 text-gray-400" />
              <span className="text-xs text-gray-500">Backend: Offline</span>
            </div>
          </div>
        </div>

        {/* Main Cards Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-5xl w-full mb-16">
          {/* New Vendor Option */}
          <div className="card-premium p-8 group animate-fade-in" style={{ animationDelay: '0.1s' }}>
            <div className="relative">
              <div className="absolute -inset-1 bg-gradient-to-r from-blue-600 to-blue-400 rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-300"></div>
              <div className="relative">
                <div className="h-16 w-16 bg-gradient-to-br from-blue-500 to-blue-600 rounded-2xl flex items-center justify-center mb-6 shadow-lg group-hover:shadow-xl transition-shadow">
                  <UserPlus className="h-8 w-8 text-white" />
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-3">New Vendor?</h2>
                <p className="text-gray-600 mb-8 h-12 leading-relaxed">
                  Submit your application to become an authorized supplier.
                </p>
                <Link
                  to="/register"
                  className="inline-flex items-center text-blue-600 font-semibold hover:text-blue-700 text-lg group/link"
                >
                  Start Application
                  <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover/link:translate-x-1" />
                </Link>

                {/* Service Indicator */}
                <div className="mt-6 pt-6 border-t border-gray-100">
                  <div className="flex items-center gap-2 text-xs text-gray-500">
                    <div className="h-1.5 w-1.5 bg-blue-500 rounded-full"></div>
                    <span>Form Validation: Client-side</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Existing Vendor Option */}
          <div className="card-premium p-8 group animate-fade-in" style={{ animationDelay: '0.2s' }}>
            <div className="relative">
              <div className="absolute -inset-1 bg-gradient-to-r from-green-600 to-green-400 rounded-2xl blur opacity-25 group-hover:opacity-40 transition duration-300"></div>
              <div className="relative">
                <div className="h-16 w-16 bg-gradient-to-br from-green-500 to-green-600 rounded-2xl flex items-center justify-center mb-6 shadow-lg group-hover:shadow-xl transition-shadow">
                  <Building2 className="h-8 w-8 text-white" />
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-3">Existing Vendor?</h2>
                <p className="text-gray-600 mb-8 h-12 leading-relaxed">
                  Log in to update your company details, tax info, and services.
                </p>
                <button
                  onClick={() => handleLogin('Vendor')}
                  disabled={isLoggingIn && loginRole === 'Vendor'}
                  className="inline-flex items-center text-green-600 font-semibold hover:text-green-700 text-lg group/link bg-transparent border-none p-0 cursor-pointer focus:outline-none disabled:opacity-50"
                >
                  {isLoggingIn && loginRole === 'Vendor' ? (
                    <>
                      <div className="h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full animate-spin mr-2"></div>
                      Authenticating...
                    </>
                  ) : (
                    <>
                      Access Portal
                      <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover/link:translate-x-1" />
                    </>
                  )}
                </button>

                {/* Service Indicator */}
                <div className="mt-6 pt-6 border-t border-gray-100">
                  <div className="flex items-center gap-2 text-xs text-gray-500">
                    <div className="h-1.5 w-1.5 bg-yellow-500 rounded-full animate-pulse"></div>
                    <span>Auth: Mock (localStorage)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Internal Access Section */}
        <div className="max-w-xl w-full glass rounded-2xl p-6 shadow-xl animate-fade-in" style={{ animationDelay: '0.3s' }}>
          <div className="flex items-center justify-center gap-2 mb-4">
            <ShieldCheck className="h-4 w-4 text-gray-400" />
            <p className="text-center text-xs text-gray-500 uppercase tracking-wider font-semibold">
              Internal System Access (Demo)
            </p>
          </div>

          <div className="flex flex-col sm:flex-row justify-center gap-4">
            <button
              onClick={() => handleLogin('Approver')}
              disabled={isLoggingIn && loginRole === 'Approver'}
              className="flex items-center justify-center px-4 py-2.5 text-sm text-gray-700 hover:text-brand-600 transition-colors bg-white rounded-lg shadow-sm hover:shadow-md border border-gray-200 disabled:opacity-50"
            >
              {isLoggingIn && loginRole === 'Approver' ? (
                <div className="h-4 w-4 border-2 border-brand-600 border-t-transparent rounded-full animate-spin mr-2"></div>
              ) : (
                <ShieldCheck className="h-4 w-4 mr-2" />
              )}
              Log in as Approver
            </button>

            <button
              onClick={() => handleLogin('Admin')}
              disabled={isLoggingIn && loginRole === 'Admin'}
              className="flex items-center justify-center px-4 py-2.5 text-sm text-gray-700 hover:text-brand-600 transition-colors bg-white rounded-lg shadow-sm hover:shadow-md border border-gray-200 disabled:opacity-50"
            >
              {isLoggingIn && loginRole === 'Admin' ? (
                <div className="h-4 w-4 border-2 border-brand-600 border-t-transparent rounded-full animate-spin mr-2"></div>
              ) : (
                <UserCog className="h-4 w-4 mr-2" />
              )}
              Log in as Administrator
            </button>
          </div>

          {/* Service Indicators for Internal Access */}
          <div className="mt-4 pt-4 border-t border-gray-200/50 flex items-center justify-center gap-4 text-xs text-gray-500">
            <div className="flex items-center gap-1.5">
              <div className="h-1.5 w-1.5 bg-yellow-500 rounded-full animate-pulse"></div>
              <span>Mock Roles</span>
            </div>
            <span className="text-gray-300">•</span>
            <div className="flex items-center gap-1.5">
              <div className="h-1.5 w-1.5 bg-gray-400 rounded-full"></div>
              <span>No Azure AD</span>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-12 text-center animate-fade-in" style={{ animationDelay: '0.4s' }}>
          <div className="text-sm text-gray-500 mb-2">
            &copy; {new Date().getFullYear()} Vendor Master Data Portal. Secured by Azure AD.
          </div>
          <VersionInfo />
        </div>
      </div>

      <style>{`
        @keyframes blob {
          0%, 100% { transform: translate(0, 0) scale(1); }
          25% { transform: translate(20px, -50px) scale(1.1); }
          50% { transform: translate(-20px, 20px) scale(0.9); }
          75% { transform: translate(50px, 50px) scale(1.05); }
        }
        
        .animate-blob {
          animation: blob 7s infinite;
        }
        
        .animation-delay-2000 {
          animation-delay: 2s;
        }
        
        .animation-delay-4000 {
          animation-delay: 4s;
        }
      `}</style>
    </div>
  );
};