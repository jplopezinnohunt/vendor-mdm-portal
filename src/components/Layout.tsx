import React, { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth, UserRole } from '../context/AuthContext';
import { 
  LayoutDashboard, 
  User, 
  FileText, 
  LogOut, 
  Menu, 
  X,
  Bell,
  ClipboardList,
  Settings,
  ShieldAlert
} from 'lucide-react';

const VENDOR_NAV = [
  { name: 'My Profile', href: '/profile', icon: User },
  { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
  { name: 'My Requests', href: '/requests', icon: FileText },
];

const APPROVER_NAV = [
  { name: 'My Worklist', href: '/approver/worklist', icon: ClipboardList },
  { name: 'Request History', href: '/approver/history', icon: FileText }, // Reusing RequestHistory page logic if needed, or separate
];

const ADMIN_NAV = [
  { name: 'System Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
  { name: 'Workflow Rules', href: '/admin/rules', icon: Settings },
  { name: 'Audit Logs', href: '/admin/audit', icon: ShieldAlert },
];

export const MainLayout: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  // Determine navigation based on role
  let navigation = VENDOR_NAV;
  if (user?.role === 'Approver') {
    navigation = APPROVER_NAV;
  } else if (user?.role === 'Admin') {
    navigation = ADMIN_NAV;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div 
          className="fixed inset-0 z-40 bg-gray-600 bg-opacity-75 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar Navigation */}
      <div className={`fixed inset-y-0 z-50 flex w-64 flex-col bg-brand-900 transition-transform lg:translate-x-0 ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}`}>
        <div className="flex h-16 shrink-0 items-center bg-brand-900 px-4">
          <span className="text-xl font-bold text-white">Vendor Portal</span>
          <button className="ml-auto text-white lg:hidden" onClick={() => setSidebarOpen(false)}>
            <X className="h-6 w-6" />
          </button>
        </div>
        <div className="flex flex-1 flex-col overflow-y-auto pt-5 pb-4">
           {/* Role Badge */}
           <div className="px-4 mb-4">
             <span className="inline-flex items-center rounded-md bg-brand-800 px-2 py-1 text-xs font-medium text-brand-100 ring-1 ring-inset ring-brand-700/10">
               {user?.role === 'Vendor' ? 'Vendor Account' : user?.role === 'Admin' ? 'System Administrator' : 'Internal Approver'}
             </span>
           </div>

          <nav className="flex-1 space-y-1 px-2">
            {navigation.map((item) => (
              <NavLink
                key={item.name}
                to={item.href}
                className={({ isActive }) =>
                  `group flex items-center rounded-md px-2 py-2 text-base font-medium ${
                    isActive
                      ? 'bg-brand-800 text-white'
                      : 'text-brand-100 hover:bg-brand-800 hover:text-white'
                  }`
                }
                onClick={() => setSidebarOpen(false)}
              >
                <item.icon className="mr-4 h-6 w-6 shrink-0" aria-hidden="true" />
                {item.name}
              </NavLink>
            ))}
          </nav>
        </div>
        <div className="flex shrink-0 border-t border-brand-800 bg-brand-900 p-4">
          <div className="flex w-full items-center">
             <div className="ml-3">
              <p className="text-sm font-medium text-white">{user?.name}</p>
              {user?.role === 'Vendor' && (
                <p className="text-xs font-medium text-brand-200">ID: {user?.sapId}</p>
              )}
              <button 
                onClick={handleLogout}
                className="mt-2 flex items-center text-xs text-brand-200 hover:text-white"
              >
                <LogOut className="mr-1 h-3 w-3" /> Sign out
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex flex-col lg:pl-64">
        {/* Top Header */}
        <div className="sticky top-0 z-10 flex h-16 shrink-0 bg-white shadow">
          <button
            type="button"
            className="px-4 text-gray-500 focus:outline-none lg:hidden"
            onClick={() => setSidebarOpen(true)}
          >
            <Menu className="h-6 w-6" />
          </button>
          
          <div className="flex flex-1 justify-end px-4 sm:px-6 lg:px-8">
            <div className="flex items-center">
              <button className="rounded-full bg-white p-1 text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2">
                <span className="sr-only">View notifications</span>
                <Bell className="h-6 w-6" />
              </button>
            </div>
          </div>
        </div>

        {/* Page Content */}
        <main className="flex-1">
          <div className="py-6">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
              <Outlet />
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};