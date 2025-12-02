import React from 'react';
import { useAuth, UserRole } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import { UserPlus, Building2, ArrowRight, ShieldCheck, UserCog } from 'lucide-react';

export const Login: React.FC = () => {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (isAuthenticated) navigate('/profile');
  }, [isAuthenticated, navigate]);

  const handleLogin = async (role: UserRole = 'Vendor') => {
    // Simulates the Azure AD / Corporate ID login flow
    await login(role);
    navigate('/profile');
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="text-center max-w-3xl mx-auto mb-12">
        <h1 className="text-4xl font-extrabold text-gray-900 tracking-tight sm:text-5xl">
          Partner with Us
        </h1>
        <p className="mt-4 text-lg text-gray-500 max-w-2xl mx-auto">
          Welcome to the centralized vendor management system. Please select an option below to
          proceed with your application or manage your existing profile.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-5xl w-full mb-16">
        {/* New Vendor Option */}
        <div className="bg-white overflow-hidden shadow-lg rounded-2xl p-8 transition-all hover:shadow-xl hover:-translate-y-1">
          <div className="h-16 w-16 bg-blue-50 rounded-2xl flex items-center justify-center mb-6">
            <UserPlus className="h-8 w-8 text-blue-600" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-3">New Vendor?</h2>
          <p className="text-gray-600 mb-8 h-12">
            Submit your application to become an authorized supplier.
          </p>
          <Link 
            to="/register" 
            className="inline-flex items-center text-blue-600 font-semibold hover:text-blue-700 text-lg group"
          >
            Start Application 
            <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover:translate-x-1" />
          </Link>
        </div>

        {/* Existing Vendor Option */}
        <div className="bg-white overflow-hidden shadow-lg rounded-2xl p-8 transition-all hover:shadow-xl hover:-translate-y-1">
          <div className="h-16 w-16 bg-green-50 rounded-2xl flex items-center justify-center mb-6">
            <Building2 className="h-8 w-8 text-green-600" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 mb-3">Existing Vendor?</h2>
          <p className="text-gray-600 mb-8 h-12">
            Log in to update your company details, tax info, and services.
          </p>
          <button 
            onClick={() => handleLogin('Vendor')}
            className="inline-flex items-center text-green-600 font-semibold hover:text-green-700 text-lg group bg-transparent border-none p-0 cursor-pointer focus:outline-none"
          >
            Access Portal 
            <ArrowRight className="ml-2 h-5 w-5 transition-transform group-hover:translate-x-1" />
          </button>
        </div>
      </div>

      {/* Internal Access Footer */}
      <div className="max-w-xl w-full border-t border-gray-200 pt-8">
        <p className="text-center text-xs text-gray-400 uppercase tracking-wider mb-4 font-semibold">
          Internal System Access (Demo)
        </p>
        <div className="flex justify-center space-x-8">
          <button 
            onClick={() => handleLogin('Approver')}
            className="flex items-center text-sm text-gray-500 hover:text-brand-600 transition-colors"
          >
            <ShieldCheck className="h-4 w-4 mr-2" />
            Log in as Approver
          </button>
          <button 
            onClick={() => handleLogin('Admin')}
            className="flex items-center text-sm text-gray-500 hover:text-brand-600 transition-colors"
          >
            <UserCog className="h-4 w-4 mr-2" />
            Log in as Administrator
          </button>
        </div>
      </div>

      <div className="mt-12 text-center text-sm text-gray-400">
        &copy; {new Date().getFullYear()} Vendor Master Data Portal. Secured by Azure AD.
      </div>
    </div>
  );
};