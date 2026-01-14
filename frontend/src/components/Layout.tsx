import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell } from 'lucide-react';
import { SidebarProvider, SidebarInset, SidebarTrigger } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';
import { ImpersonationBanner } from './ImpersonationBanner';

const MainLayoutContent: React.FC = () => {
  return (
    <SidebarInset>
      {/* Top Navigation Bar - Fully Responsive */}
      <header className="flex h-14 md:h-16 shrink-0 items-center gap-2 border-b bg-white px-3 md:px-4 shadow-sm">
        <SidebarTrigger className="-ml-1" />
        <Separator orientation="vertical" className="mr-2 h-4" />
        <div className="flex items-center gap-2">
          <div className="flex h-7 w-7 md:h-8 md:w-8 items-center justify-center rounded-lg bg-brand-600 text-white font-bold text-xs md:text-sm">
            VM
          </div>
          <span className="text-sm md:text-base font-semibold hidden sm:block">
            Vendor Portal
          </span>
        </div>

        {/* Top-Right: Notifications + User Profile */}
        <div className="ml-auto flex items-center gap-2 md:gap-4">
          <button className="rounded-full p-1.5 md:p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2">
            <span className="sr-only">View notifications</span>
            <Bell className="h-5 w-5 md:h-6 md:w-6" />
          </button>
          <UserNav />
        </div>
      </header>

      <ImpersonationBanner />

      {/* Page Content - Responsive Padding */}
      <main className="flex-1 overflow-auto p-3 md:p-4 lg:p-6">
        <Outlet />
      </main>

      {/* Footer with Service Status */}
      <footer className="border-t bg-white p-3 md:p-4">
        <div className="mx-auto max-w-7xl px-3 sm:px-6 md:px-8">
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