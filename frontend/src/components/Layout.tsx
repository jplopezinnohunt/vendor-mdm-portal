import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell, PanelLeft } from 'lucide-react';
import { SidebarProvider, useSidebar } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { Button } from '@/components/ui/button';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';
import { ImpersonationBanner } from './ImpersonationBanner';

const MainLayoutContent: React.FC = () => {
  const { toggleSidebar } = useSidebar();

  return (
    <div className="flex min-h-screen flex-1 flex-col transition-all duration-200 ease-linear md:pl-[var(--sidebar-width-icon)] md:peer-data-[state=expanded]:pl-[var(--sidebar-width)]">
      <ImpersonationBanner />
      {/* Top Navigation Bar */}
      <header className="sticky top-0 z-10 flex h-16 shrink-0 items-center gap-2 border-b bg-white px-4 shadow-sm">
        <div className="flex items-center gap-2">
          <Button
            onClick={toggleSidebar}
            variant="ghost"
            size="icon"
            className="-ml-1 h-7 w-7 text-gray-500 hover:text-gray-900"
            aria-label="Toggle Sidebar"
          >
            <PanelLeft className="h-4 w-4" />
          </Button>
          <Separator orientation="vertical" className="mr-2 h-4" />
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
        <div className="ml-auto flex items-center gap-4">
          <button className="rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2">
            <span className="sr-only">View notifications</span>
            <Bell className="h-6 w-6" />
          </button>
          <UserNav />
        </div>
      </header>

      {/* Page Content */}
      <div className="flex-1 overflow-auto p-4">
        <Outlet />
      </div>

      {/* Footer with Service Status */}
      <footer className="border-t bg-white p-4">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
          <ServiceStatusPanel />
        </div>
      </footer>
    </div>
  );
};

export const MainLayout: React.FC = () => {
  return (
    <SidebarProvider defaultOpen={true}>
      <AppSidebar />
      <MainLayoutContent />
    </SidebarProvider>
  );
};