import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell } from 'lucide-react';
import { SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';

export const MainLayout: React.FC = () => {
  return (
    <SidebarProvider defaultOpen={false}>
      <div className="flex min-h-screen w-full">
        {/* Collapsible Sidebar */}
        <AppSidebar />

        {/* Main Content Area */}
        <div className="flex flex-1 flex-col">
          {/* Top Navigation Bar */}
          <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b bg-white px-4 shadow-sm">
            {/* Top-Left: Toggle + App Branding */}
            <div className="flex items-center gap-3">
              <SidebarTrigger />
              <div className="flex items-center gap-2">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white font-bold text-sm">
                  VM
                </div>
                <span className="text-base font-semibold hidden sm:block">
                  Vendor Portal
                </span>
              </div>
            </div>

            {/* Top-Right: Notifications + User Profile */}
            <div className="flex items-center gap-4">
              <button className="rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2">
                <span className="sr-only">View notifications</span>
                <Bell className="h-6 w-6" />
              </button>
              <UserNav />
            </div>
          </header>

          {/* Page Content */}
          <main className="flex-1 overflow-auto">
            <Outlet />
          </main>

          {/* Footer with Service Status */}
          <footer className="border-t bg-white p-4">
            <div className="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
              <ServiceStatusPanel />
            </div>
          </footer>
        </div>
      </div>
    </SidebarProvider>
  );
};