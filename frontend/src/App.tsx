import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { NotFound } from './pages/NotFound';
import { Unauthorized } from './pages/Unauthorized';
import { FutureFeature } from './pages/FutureFeature';
import { AuthProvider, useAuth, UserRole } from './context/AuthContext';
import { SignalRProvider } from './context/SignalRContext';
import { MainLayout } from './components/Layout';
import { Dashboard } from './pages/Dashboard';
import { VendorProfile } from './pages/VendorProfile';
import { ChangeRequestForm } from './pages/ChangeRequestForm';
import { CreateVendorForm } from './pages/approver/CreateVendorForm';
import { RequestHistory } from './pages/RequestHistory';
import { Login } from './pages/Login';
import { VendorRegistration } from './pages/VendorRegistration';
import { InvitationPage } from './pages/auth/InvitationPage';
import { MagicLoginCallback } from './pages/auth/MagicLoginCallback';
import { InvitationRegistration } from './pages/InvitationRegistration';
import { ServerError } from './pages/ServerError';
import { ApproverDashboard } from './pages/approver/ApproverDashboard';
import { RequestReview } from './pages/approver/RequestReview';
import { OnboardingReview } from './pages/approver/OnboardingReview';
import { VendorSelectionList } from './pages/approver/VendorSelectionList';
import { AdminDashboard } from './pages/admin/AdminDashboard';
import { InviteVendorForm } from './pages/admin/InviteVendorForm';
import { InvitationManagement } from './pages/admin/InvitationManagement';
import { SystemStatus } from './pages/admin/SystemStatus';
import { ViewVendor } from './pages/ViewVendor';
import BranchingStrategy from './pages/BranchingStrategy';
import { UserManagement } from './pages/admin/UserManagement';
import { ArchitectureOverview } from './pages/admin/ArchitectureOverview';
import { InfrastructureCosts } from './pages/admin/InfrastructureCosts';
import { UserAccount } from './pages/UserAccount';
import { EventDashboard } from './pages/EventDashboard';
import { EventDetail } from './pages/EventDetail';


// Protected Route Guard
const ProtectedRoute = ({ children, allowedRoles }: { children?: React.ReactNode, allowedRoles?: UserRole[] }) => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) {
    return (
      <div className="flex h-screen w-full items-center justify-center bg-gray-50">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-600 border-t-transparent"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && user && !allowedRoles.includes(user.role)) {
    // Role mismatch redirect
    if (user.role === 'Admin') return <Navigate to="/admin/dashboard" replace />;
    if (user.role === 'Requestor' || user.role === 'VendorUnit' || user.role === 'BFM' || user.role === 'Approver') {
      return <Navigate to="/approver/worklist" replace />;
    }
    return <Navigate to="/profile" replace />;
  }

  return <>{children}</>;
};


import { DebugConsole } from './components/dev/DebugConsole';

const App: React.FC = () => {
  return (
    <AuthProvider>
      <SignalRProvider>
        <DebugConsole />
        <BrowserRouter>
        <Routes>
          {/* Public Routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<VendorRegistration />} />
          <Route path="/accept-invite" element={<InvitationPage />} />
          <Route path="/invitation/register/:token" element={<InvitationRegistration />} />
          <Route path="/magic-login" element={<MagicLoginCallback />} />

          {/* Main Layout Wrapper */}
          <Route path="/" element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }>
            {/* VENDOR ROUTES */}
            <Route index element={<RoleBasedRedirect />} />

            <Route path="profile" element={
              <ProtectedRoute allowedRoles={['Vendor']}>
                <VendorProfile />
              </ProtectedRoute>
            } />
            <Route path="dashboard" element={
              <ProtectedRoute allowedRoles={['Vendor']}>
                <Dashboard />
              </ProtectedRoute>
            } />
            <Route path="requests" element={
              <ProtectedRoute allowedRoles={['Vendor']}>
                <RequestHistory />
              </ProtectedRoute>
            } />
            <Route path="requests/new" element={
              <ProtectedRoute allowedRoles={['Vendor']}>
                <ChangeRequestForm />
              </ProtectedRoute>
            } />

            {/* APPROVER ROUTES */}
            <Route path="approver/worklist" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Approver', 'Admin']}>
                <ApproverDashboard mode="worklist" />
              </ProtectedRoute>
            } />
            <Route path="approver/history" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Approver', 'Admin']}>
                <ApproverDashboard mode="history" />
              </ProtectedRoute>
            } />
            <Route path="approver/requests/:id" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin', 'BFM', 'VendorUnit', 'Requestor']}>
                <RequestReview />
              </ProtectedRoute>
            } />
            <Route path="approver/onboarding/:id" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin', 'BFM', 'VendorUnit', 'Requestor']}>
                <OnboardingReview />
              </ProtectedRoute>
            } />
            <Route path="approver/invite-vendor" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Admin']}>
                <InviteVendorForm />
              </ProtectedRoute>
            } />
            <Route path="approver/create-vendor" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Admin']}>
                <CreateVendorForm />
              </ProtectedRoute>
            } />
            <Route path="approver/select-vendor" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Admin']}>
                <VendorSelectionList />
              </ProtectedRoute>
            } />
            <Route path="approver/update-vendor/:vendorId" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'BFM', 'Admin']}>
                <ChangeRequestForm />
              </ProtectedRoute>
            } />
            <Route path="view-vendor" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin']}>
                <ViewVendor />
              </ProtectedRoute>
            } />

            {/* EVENT ROUTES */}
            <Route path="events" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'Admin']}>
                <EventDashboard />
              </ProtectedRoute>
            } />
            <Route path="events/:id" element={
              <ProtectedRoute allowedRoles={['Requestor', 'VendorUnit', 'Admin']}>
                <EventDetail />
              </ProtectedRoute>
            } />


            {/* ADMIN ROUTES */}
            <Route path="admin/dashboard" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <AdminDashboard />
              </ProtectedRoute>
            } />
            <Route path="admin/rules" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <AdminDashboard />
              </ProtectedRoute>
            } />
            <Route path="admin/system-status" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <SystemStatus />
              </ProtectedRoute>
            } />
            <Route path="admin/strategy" element={
              <ProtectedRoute allowedRoles={['Admin', 'Approver', 'Vendor']}>
                <BranchingStrategy />
              </ProtectedRoute>
            } />
            <Route path="admin/users" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <UserManagement />
              </ProtectedRoute>
            } />
            <Route path="admin/architecture" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <ArchitectureOverview />
              </ProtectedRoute>
            } />
            <Route path="admin/costs" element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <InfrastructureCosts />
              </ProtectedRoute>
            } />

            {/* SHARED ROUTES */}
            <Route path="account" element={
              <ProtectedRoute>
                <UserAccount />
              </ProtectedRoute>
            } />

          </Route>

          {/* Catch all */}
          <Route path="/401" element={<Unauthorized />} />
          <Route path="/500" element={<ServerError />} />
          <Route path="/coming-soon" element={<FutureFeature />} />
          <Route path="/404" element={<NotFound />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </BrowserRouter>
      </SignalRProvider>
    </AuthProvider>
  );
};


// Helper to redirect to correct home page based on role
const RoleBasedRedirect = () => {
  const { user, isLoading } = useAuth();

  if (isLoading) return null;

  // Admin goes to admin dashboard
  if (user?.role === 'Admin') return <Navigate to="/admin/dashboard" replace />;

  // Requestor, VendorUnit, BFM go to approver worklist
  if (user?.role === 'Requestor' || user?.role === 'VendorUnit' || user?.role === 'BFM' || user?.role === 'Approver') {
    return <Navigate to="/approver/worklist" replace />;
  }

  // Default to Vendor profile
  return <Navigate to="/profile" replace />;
};

export default App;