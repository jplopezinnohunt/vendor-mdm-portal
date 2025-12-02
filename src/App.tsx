import React from 'react';
import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth, UserRole } from './context/AuthContext';
import { MainLayout } from './components/Layout';
import { Dashboard } from './pages/Dashboard';
import { VendorProfile } from './pages/VendorProfile';
import { ChangeRequestForm } from './pages/ChangeRequestForm';
import { RequestHistory } from './pages/RequestHistory';
import { Login } from './pages/Login';
import { VendorRegistration } from './pages/VendorRegistration';
import { ApproverDashboard } from './pages/approver/ApproverDashboard';
import { RequestReview } from './pages/approver/RequestReview';
import { OnboardingReview } from './pages/approver/OnboardingReview';
import { AdminDashboard } from './pages/admin/AdminDashboard';

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
    if (user.role === 'Approver') return <Navigate to="/approver/worklist" replace />;
    if (user.role === 'Admin') return <Navigate to="/admin/dashboard" replace />;
    return <Navigate to="/profile" replace />;
  }

  return <>{children}</>;
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <HashRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<VendorRegistration />} />
          
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
              <ProtectedRoute allowedRoles={['Approver', 'Admin']}>
                <ApproverDashboard mode="worklist" />
              </ProtectedRoute>
            } />
             <Route path="approver/history" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin']}>
                <ApproverDashboard mode="history" />
              </ProtectedRoute>
            } />
            <Route path="approver/requests/:id" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin']}>
                <RequestReview />
              </ProtectedRoute>
            } />
            <Route path="approver/onboarding/:id" element={
              <ProtectedRoute allowedRoles={['Approver', 'Admin']}>
                <OnboardingReview />
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
          </Route>

          {/* Catch all */}
          <Route path="*" element={<RoleBasedRedirect />} />
        </Routes>
      </HashRouter>
    </AuthProvider>
  );
};

// Helper to redirect to correct home page based on role
const RoleBasedRedirect = () => {
  const { user, isLoading } = useAuth();
  
  if (isLoading) return null;
  
  if (user?.role === 'Approver') return <Navigate to="/approver/worklist" replace />;
  if (user?.role === 'Admin') return <Navigate to="/admin/dashboard" replace />;
  
  // Default to Vendor profile
  return <Navigate to="/profile" replace />;
};

export default App;