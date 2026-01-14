import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell, PanelLeft } from 'lucide-react';
import { SidebarProvider, SidebarInset, useSidebar } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { Button } from '@/components/ui/button';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';
import { ImpersonationBanner } from './ImpersonationBanner';

const MainLayoutContent: React.FC = () => {
  return (
    <SidebarInset>
      <ImpersonationBanner />
      {/* Top Navigation Bar */}
      <header className="flex h-16 shrink-0 items-center gap-2 border-b bg-background px-4">
        <SidebarTrigger className="-ml-1" />
        <Separator orientation="vertical" className="mr-2 h-4" />
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white font-bold text-sm">
            VM
          </div>
          <span className="text-base font-semibold hidden sm:block">
            Vendor Portal
          </span>
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
      <div className="flex flex-1 flex-col gap-4 p-4">
        <Outlet />
      </div>

      {/* Footer with Service Status */}
      <footer className="border-t bg-white p-4">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 md:px-8">
          <ServiceStatusPanel />
        </div>
      </footer>
    </SidebarInset>
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