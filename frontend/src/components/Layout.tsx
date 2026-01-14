import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell } from 'lucide-react';
import { SidebarProvider, SidebarTrigger, SidebarInset } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';
import { ImpersonationBanner } from './ImpersonationBanner';

export const MainLayout: React.FC = () => {
  return (
    <SidebarProvider
      defaultOpen={true}
      style={{
        '--sidebar-width': '15rem',
        '--sidebar-width-icon': '3rem',
      } as React.CSSProperties}
    >
      <AppSidebar />
      <SidebarInset>
        <header className="flex h-16 shrink-0 items-center gap-2 border-b bg-white px-4 shadow-sm bg-background">
          <SidebarTrigger className="-ml-1" />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <div className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-white font-bold text-sm">
              VM
            </div>
            <span className="text-base font-semibold">Vendor Portal</span>
          </div>

          <div className="ml-auto flex items-center gap-4">
            <button className="rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-500">
              <Bell className="h-6 w-6" />
            </button>
            <UserNav />
          </div>
        </header>

        <ImpersonationBanner />

        <div className="flex-1 overflow-auto bg-gray-50/50 p-4 sm:p-6 lg:p-8">
          <div className="w-full">
            <Outlet />
          </div>
        </div>

        <footer className="border-t bg-white p-4">
          <ServiceStatusPanel />
        </footer>
      </SidebarInset>
    </SidebarProvider>
  );
};