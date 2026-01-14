import React from 'react';
import { Outlet } from 'react-router-dom';
import { Bell } from 'lucide-react';
import { SidebarProvider, SidebarTrigger } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';
import { AppSidebar } from './app-sidebar';
import { UserNav } from './user-nav';
import { ServiceStatusPanel } from './ServiceStatusPanel';
import { ImpersonationBanner } from './ImpersonationBanner';

export const MainLayout: React.FC = () => {
  return (
    <SidebarProvider defaultOpen={true}>
      <div className="relative flex min-h-screen w-full">
        <AppSidebar />

        {/* Main content that shifts based on sidebar state */}
        <main
          className="flex-1 flex flex-col min-w-0 bg-background transition-all duration-200 ease-linear
            md:ml-0
            peer-data-[state=expanded]:md:ml-[var(--sidebar-width)]
            peer-data-[state=collapsed]:md:ml-[var(--sidebar-width-icon)]"
          style={{
            '--sidebar-width': '16rem',
            '--sidebar-width-icon': '3rem'
          } as React.CSSProperties}
        >
          <header className="sticky top-0 z-10 flex h-16 shrink-0 items-center gap-2 border-b bg-white px-4 shadow-sm">
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

          <div className="flex-1 p-4">
            <Outlet />
          </div>

          <footer className="border-t bg-white p-4">
            <ServiceStatusPanel />
          </footer>
        </main>
      </div>
    </SidebarProvider>
  );
};