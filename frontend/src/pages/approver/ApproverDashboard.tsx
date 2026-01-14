import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { VendorService } from '../../services/vendorService';
import { ChangeRequest, ChangeRequestStatus, VendorApplication, ApplicationStatus, RequestType } from '../../types';
import { Card, StatusBadge, Button, Modal } from '../../components/ui/Elements';
import { CheckSquare, UserPlus, FileText, Search, Filter, Plus, Send, Clock, Mail, CheckCircle, XCircle, Ban, RefreshCw, Copy, AlertTriangle } from 'lucide-react';
import { api } from '../../services/api';

interface Invitation {
  id: string;
  vendorLegalName: string;
  primaryContactEmail: string;
  status: string;
  invitedByName: string;
  createdAt: string;
  expiresAt: string;
  vendorApplicationId?: string;
  emailSent?: boolean;
  currentStage?: string;
}

interface ApproverDashboardProps {
  mode?: 'worklist' | 'history';
}

export const ApproverDashboard: React.FC<ApproverDashboardProps> = ({ mode = 'worklist' }) => {
  const [changeRequests, setChangeRequests] = useState<ChangeRequest[]>([]);
  const [onboardingRequests, setOnboardingRequests] = useState<VendorApplication[]>([]);
  const [invitations, setInvitations] = useState<Invitation[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'onboarding' | 'changes' | 'invitations'>('onboarding');
  const navigate = useNavigate();

  // Filter States
  const [obFilters, setObFilters] = useState({
    id: '',
    company: '',
    tax: '',
    status: ''
  });

  const [crFilters, setCrFilters] = useState({
    id: '',
    vendor: '',
    type: '',
    status: '',
    risk: ''
  });

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        // Fetch real data from APIs
        let invitationsData: Invitation[] = [];
        try {
          const invRes = await api.get('/invitation/list');
          invitationsData = invRes.data.invitations;
        } catch (e) {
          console.error('Failed to fetch invitations, using mock fallback', e);
          invitationsData = [
            {
              id: 'INV-MOCK-1',
              vendorLegalName: 'Mock Vendor (API Failed)',
              primaryContactEmail: 'mock@example.com',
              status: 'Pending',
              invitedByName: 'System',
              createdAt: new Date().toISOString(),
              expiresAt: new Date(Date.now() + 86400000).toISOString(),
              emailSent: false
            }
          ];
        }

        // Fetch Pending Onboarding Applications (Reviews)
        let onboardingData: VendorApplication[] = [];
        try {
          const reviewRes = await api.get('/review/pending');
          onboardingData = reviewRes.data.map((item: any) => ({
            id: item.id,
            companyName: item.companyName,
            taxId: item.taxId || 'N/A',
            contactName: item.contactName,
            email: item.contactEmail,
            status: 'Submitted', // Normalize to Submitted for UI
            submittedAt: item.createdAt,
            sanctionCheckStatus: item.attributes?.SanctionsStatus || 'Pending',
            attributes: item.attributes
          }));
        } catch (e) {
          console.error('Failed to fetch pending reviews', e);
        }

        // Initialize missing data to prevent ReferenceError
        const changeRequestsData: ChangeRequest[] = [];

        /*
        // Mock Invitations (Disabled)
        const invitationsData: Invitation[] = [
          {
            id: 'INV-001',
             // ...
          }
        ]; 
        */

        if (mode === 'worklist') {
          const pendingChanges = changeRequestsData.filter(r =>
            r.status !== ChangeRequestStatus.Approved &&
            r.status !== ChangeRequestStatus.Rejected &&
            r.status !== ChangeRequestStatus.Applied
          );

          const worklistInvitations = invitationsData.filter(i =>
            i.status === 'Pending' || i.status === 'Accepted' || i.status === 'Expired'
          );

          setChangeRequests(pendingChanges);
          setOnboardingRequests(onboardingData);
          setInvitations(worklistInvitations);
        } else {
          // History mode
          const historyChanges = changeRequestsData.filter(r =>
            r.status === ChangeRequestStatus.Approved ||
            r.status === ChangeRequestStatus.Rejected ||
            r.status === ChangeRequestStatus.Applied ||
            r.status === ChangeRequestStatus.Error
          );

          const historyInvitations = invitationsData.filter(i =>
            i.status !== 'Pending' && i.status !== 'Accepted' && i.status !== 'Expired'
          );

          setChangeRequests(historyChanges);
          setOnboardingRequests([]);
          setInvitations(historyInvitations);
        }
      } catch (error) {
        console.error('Error loading dashboard data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [mode]);

  const totalCount = changeRequests.length + onboardingRequests.length;
  const highRiskCount = changeRequests.filter(r => (r.items || []).some(i => i.isSensitive)).length;

  // Filter Logic
  const filteredOnboarding = onboardingRequests.filter(app =>
    app.id.toLowerCase().includes(obFilters.id.toLowerCase()) &&
    app.companyName.toLowerCase().includes(obFilters.company.toLowerCase()) &&
    app.taxId.toLowerCase().includes(obFilters.tax.toLowerCase()) &&
    (obFilters.status === '' || app.status === obFilters.status)
  );

  const filteredChanges = changeRequests.filter(req => {
    const isSensitive = (req.items || []).some(i => i.isSensitive);
    const risk = isSensitive ? 'High Risk' : 'Standard';
    return (
      req.id.toLowerCase().includes(crFilters.id.toLowerCase()) &&
      req.vendorId.toLowerCase().includes(crFilters.vendor.toLowerCase()) &&
      (crFilters.type === '' || req.requestType === crFilters.type) &&
      (crFilters.status === '' || req.status === crFilters.status) &&
      (crFilters.risk === '' || risk === crFilters.risk)
    );
  });

  // Helper to determine fine-grained status for Onboarding
  const getOnboardingStatusDisplay = (app: VendorApplication) => {
    if (app.status === ApplicationStatus.Approved) return { label: 'Approved', style: 'bg-green-100 text-green-800' };
    if (app.status === ApplicationStatus.Rejected) return { label: 'Rejected', style: 'bg-red-100 text-red-800' };

    // Check attributes for Sanctions Status if available
    const sanctionsStatus = app.attributes?.SanctionsStatus || app.sanctionCheckStatus;

    if (sanctionsStatus === 'Pending') return { label: 'Sanction Screening', style: 'bg-yellow-100 text-yellow-800' };
    if (sanctionsStatus === 'Failed' || sanctionsStatus === 'PotentialMatch') return { label: 'Sanction Hit', style: 'bg-red-100 text-red-800' };

    // Status is Submitted/PendingReview
    if (app.status === 'PendingReview') return { label: 'Internal Review', style: 'bg-blue-100 text-blue-800' };

    // Passed sanctions, so it's in internal review
    return { label: 'Internal Review', style: 'bg-blue-100 text-blue-800' };
  };

  // UI Helpers for Filters
  const FilterInput = ({ value, onChange, placeholder }: { value: string, onChange: (val: string) => void, placeholder?: string }) => (
    <div className="relative rounded-md shadow-sm mt-1">
      <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-2">
        <Search className="h-3 w-3 text-gray-400" />
      </div>
      <input
        type="text"
        className="block w-full rounded-md border-gray-300 pl-8 focus:border-brand-500 focus:ring-brand-500 sm:text-xs py-1"
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
    </div>
  );

  const FilterSelect = ({ value, onChange, options }: { value: string, onChange: (val: string) => void, options: string[] }) => (
    <select
      className="block w-full rounded-md border-gray-300 focus:border-brand-500 focus:ring-brand-500 sm:text-xs py-1 mt-1"
      value={value}
      onChange={(e) => onChange(e.target.value)}
    >
      <option value="">All</option>
      {options.map((opt) => (
        <option key={opt} value={opt}>{opt}</option>
      ))}
    </select>
  );

  const getInvitationStatusBadge = (inv: Invitation) => {
    const badges = {
      Pending: {
        icon: Clock,
        className: 'bg-yellow-100 text-yellow-800',
        label: 'Pending'
      },
      Accepted: {
        icon: Mail,
        className: 'bg-blue-100 text-blue-800',
        label: 'Accepted'
      },
      Completed: {
        icon: CheckCircle,
        className: 'bg-green-100 text-green-800',
        label: 'Completed'
      },
      Expired: {
        icon: XCircle,
        className: 'bg-gray-100 text-gray-800',
        label: 'Expired'
      },
      Cancelled: {
        icon: Ban,
        className: 'bg-red-100 text-red-800',
        label: 'Cancelled'
      },
      // Virtual Statuses for Dashboard
      InProgress: {
        icon: RefreshCw,
        className: 'bg-indigo-100 text-indigo-800',
        label: 'In Progress'
      },
      Enriched: {
        icon: CheckSquare,
        className: 'bg-purple-100 text-purple-800',
        label: 'Vendor Submitted'
      }
    };

    let badgeKey = inv.status;

    // Override status based on CurrentStage if Pending or MfaVerified
    if (inv.status === 'Pending' || (inv.status as any) === 'MfaVerified') {
      if (inv.currentStage === 'InitialInfoCompleted' || inv.currentStage === 'Enriched' || inv.currentStage === 'MfaVerified') {
        badgeKey = 'InProgress';
      }
      if (inv.currentStage === 'Enriched') {
        badgeKey = 'Enriched';
      }
    }

    const badge = badges[badgeKey as keyof typeof badges] || badges.Pending;
    const Icon = badge.icon;

    return (
      <div className="flex items-center gap-2">
        <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>
          <Icon className="h-3 w-3" />
          {badge.label}
        </span>
        {inv.emailSent === false && (
          <div className="group relative">
            <AlertTriangle className="h-4 w-4 text-red-500 cursor-help" />
            <div className="absolute bottom-full mb-2 hidden group-hover:block w-48 bg-gray-900 text-white text-xs rounded p-2 z-10">
              Email delivery failed. Please resend or copy link manually.
            </div>
          </div>
        )}
      </div>
    );
  };

  const [actionLoading, setActionLoading] = useState<string | null>(null);
  const [confirmAction, setConfirmAction] = useState<{ id: string, type: 'resend' | 'cancel' } | null>(null);
  const [resentLink, setResentLink] = useState<{ id: string, link: string, emailSent: boolean } | null>(null);

  const handleResend = async (id: string) => {
    setActionLoading(id + '-resend');
    try {
      const response = await api.post(`/invitation/resend/${id}`);
      const link = `${window.location.origin}${response.data.invitationLink}`;
      const emailSent = response.data.emailSent;
      setResentLink({ id, link, emailSent });
      // Refresh invitations
      const res = await api.get('/invitation/list');
      setInvitations(mode === 'worklist'
        ? (res.data.invitations || []).filter((i: any) => i.status === 'Pending' || i.status === 'Accepted' || i.status === 'Expired')
        : (res.data.invitations || []).filter((i: any) => i.status !== 'Pending' && i.status !== 'Accepted')
      );
    } catch (error: any) {
      console.error('Failed to resend invitation:', error);
      alert(error.userMessage || 'Failed to resend invitation');
    } finally {
      setActionLoading(null);
      setConfirmAction(null);
    }
  };

  const handleCancel = async (id: string) => {
    setActionLoading(id + '-cancel');
    try {
      await api.post(`/invitation/cancel/${id}`);
      alert('Invitation has been cancelled successfully.');
      // Refresh invitations
      const res = await api.get('/invitation/list');
      setInvitations(mode === 'worklist'
        ? (res.data.invitations || []).filter((i: any) => i.status === 'Pending' || i.status === 'Accepted' || i.status === 'Expired')
        : (res.data.invitations || []).filter((i: any) => i.status !== 'Pending' && i.status !== 'Accepted')
      );
    } catch (error: any) {
      console.error('Failed to cancel invitation:', error);
      alert(error.userMessage || 'Failed to cancel invitation');
    } finally {
      setActionLoading(null);
      setConfirmAction(null);
    }
  };

  const handleCopy = (link: string) => {
    navigator.clipboard.writeText(link);
    alert('Link copied to clipboard');
  };

  return (
    <div className="space-y-8">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">
            {mode === 'worklist' ? 'Approver Worklist' : 'Request History'}
          </h1>
          <p className="mt-2 text-sm text-gray-700">
            {mode === 'worklist'
              ? 'Review pending onboarding applications and master data change requests.'
              : 'View past decisions and finalized requests.'}
          </p>
        </div>
        {mode === 'worklist' && (
          <div className="mt-4 sm:ml-16 sm:mt-0 sm:flex-none flex flex-wrap gap-2">
            <Button
              variant="outline"
              onClick={() => navigate('/approver/invite-vendor')}
              className="flex items-center gap-2"
            >
              <Mail className="h-4 w-4" />
              Invite Vendor
            </Button>
            <Button
              variant="secondary"
              onClick={() => navigate('/approver/select-vendor')}
              className="flex items-center gap-2"
            >
              <FileText className="h-4 w-4" />
              Update Vendor
            </Button>
            <Button
              onClick={() => navigate('/approver/create-vendor')}
              className="flex items-center gap-2"
            >
              <Plus className="h-4 w-4" />
              Create Vendor
            </Button>
          </div>
        )}
      </div>

      {/* KPI Cards - Only show in Worklist mode */}
      {mode === 'worklist' && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6">
            <dt className="truncate text-sm font-medium text-gray-500">Total Pending</dt>
            <dd className="mt-1 text-3xl font-semibold tracking-tight text-gray-900">
              {changeRequests.length + onboardingRequests.length + invitations.length}
            </dd>
          </div>
          <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6 border-l-4 border-blue-500">
            <dt className="truncate text-sm font-medium text-gray-500">New Onboarding</dt>
            <dd className="mt-1 text-3xl font-semibold tracking-tight text-blue-600">{onboardingRequests.length}</dd>
          </div>
          <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6 border-l-4 border-yellow-500">
            <dt className="truncate text-sm font-medium text-gray-500">Open Invitations</dt>
            <dd className="mt-1 text-3xl font-semibold tracking-tight text-yellow-600">
              {invitations.length}
            </dd>
          </div>
          <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6 border-l-4 border-red-500">
            <dt className="truncate text-sm font-medium text-gray-500">High Risk Changes</dt>
            <dd className="mt-1 text-3xl font-semibold tracking-tight text-red-600">
              {highRiskCount}
            </dd>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8" aria-label="Tabs">
          <button
            onClick={() => setActiveTab('onboarding')}
            className={`${activeTab === 'onboarding'
              ? 'border-brand-500 text-brand-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center`}
          >
            <UserPlus className={`mr-2 h-5 w-5 ${activeTab === 'onboarding' ? 'text-brand-500' : 'text-gray-400'}`} />
            Onboarding Requests
            <span className={`ml-2 py-0.5 px-2.5 rounded-full text-xs font-medium ${activeTab === 'onboarding' ? 'bg-brand-100 text-brand-600' : 'bg-gray-100 text-gray-900'}`}>
              {onboardingRequests.length}
            </span>
          </button>
          <button
            onClick={() => setActiveTab('changes')}
            className={`${activeTab === 'changes'
              ? 'border-brand-500 text-brand-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center`}
          >
            <FileText className={`mr-2 h-5 w-5 ${activeTab === 'changes' ? 'text-brand-500' : 'text-gray-400'}`} />
            Vendor Master Data Changes
            <span className={`ml-2 py-0.5 px-2.5 rounded-full text-xs font-medium ${activeTab === 'changes' ? 'bg-brand-100 text-brand-600' : 'bg-gray-100 text-gray-900'}`}>
              {changeRequests.length}
            </span>
          </button>
          <button
            onClick={() => setActiveTab('invitations')}
            className={`${activeTab === 'invitations'
              ? 'border-brand-500 text-brand-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center`}
          >
            <Mail className={`mr-2 h-5 w-5 ${activeTab === 'invitations' ? 'text-brand-500' : 'text-gray-400'}`} />
            Sent Invitations
            <span className={`ml-2 py-0.5 px-2.5 rounded-full text-xs font-medium ${activeTab === 'invitations' ? 'bg-brand-100 text-brand-600' : 'bg-gray-100 text-gray-900'}`}>
              {invitations.length}
            </span>
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      <div className="mt-4">
        {activeTab === 'onboarding' ? (
          /* Onboarding Requests Table */
          <Card className="px-0 py-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-32">
                      Application ID
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                      Company Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                      Tax ID
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-40">
                      Submitted Date
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-40">
                      Workflow Status
                    </th>
                    <th className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                  </tr>
                  {/* Filter Row */}
                  <tr className="bg-gray-50">
                    <td className="px-6 py-2">
                      <FilterInput value={obFilters.id} onChange={(v) => setObFilters({ ...obFilters, id: v })} placeholder="Filter ID" />
                    </td>
                    <td className="px-6 py-2">
                      <FilterInput value={obFilters.company} onChange={(v) => setObFilters({ ...obFilters, company: v })} placeholder="Filter Name" />
                    </td>
                    <td className="px-6 py-2">
                      <FilterInput value={obFilters.tax} onChange={(v) => setObFilters({ ...obFilters, tax: v })} placeholder="Filter Tax" />
                    </td>
                    <td className="px-6 py-2"></td>
                    <td className="px-6 py-2">
                      <FilterSelect
                        value={obFilters.status}
                        onChange={(v) => setObFilters({ ...obFilters, status: v })}
                        options={Object.values(ApplicationStatus)}
                      />
                    </td>
                    <td className="px-6 py-2"></td>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 bg-white">
                  {loading ? (
                    <tr><td colSpan={6} className="px-6 py-4 text-center">Loading...</td></tr>
                  ) : filteredOnboarding.length === 0 ? (
                    <tr><td colSpan={6} className="px-6 py-4 text-center text-gray-500">No matching requests found.</td></tr>
                  ) : (
                    filteredOnboarding.map((app) => {
                      const statusDisplay = getOnboardingStatusDisplay(app);
                      return (
                        <tr key={app.id}>
                          <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">{app.id}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-900">{app.companyName}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{app.taxId}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            {new Date(app.submittedAt).toLocaleDateString()}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm">
                            <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${statusDisplay.style}`}>
                              {statusDisplay.label}
                            </span>
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                            {mode === 'worklist' ? (
                              <Link to={`/approver/onboarding/${app.id}`}>
                                <Button size="sm" variant="outline">
                                  <CheckSquare className="mr-2 h-3 w-3" /> Review
                                </Button>
                              </Link>
                            ) : (
                              <span className="text-gray-400 text-xs">Closed</span>
                            )}
                          </td>
                        </tr>
                      )
                    })
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        ) : activeTab === 'invitations' ? (
          /* Sent Invitations Table */
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <h3 className="text-lg font-medium text-gray-900">Vendor Invitations</h3>
              <Button
                size="sm"
                onClick={() => navigate('/approver/invite-vendor')}
                className="flex items-center gap-2"
              >
                <Plus className="h-4 w-4" />
                Invite Vendor
              </Button>
            </div>
            <Card className="px-0 py-0">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Vendor</th>
                      <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Email</th>
                      <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Status</th>
                      <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Created</th>
                      <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Expires</th>
                      <th className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200 bg-white">
                    {loading ? (
                      <tr><td colSpan={6} className="px-6 py-4 text-center">Loading...</td></tr>
                    ) : invitations.length === 0 ? (
                      <tr><td colSpan={6} className="px-6 py-4 text-center text-gray-500">No invitations found.</td></tr>
                    ) : (
                      invitations.map((inv) => (
                        <tr key={inv.id}>
                          <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">{inv.vendorLegalName}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{inv.primaryContactEmail}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm">
                            {getInvitationStatusBadge(inv)}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            {new Date(inv.createdAt).toLocaleDateString()}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            {new Date(inv.expiresAt).toLocaleDateString()}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                            <div className="flex items-center justify-end gap-2">
                              {(inv.status === 'Pending' || inv.status === 'Expired' || inv.status === 'Accepted') && (
                                <Button
                                  size="sm"
                                  variant="outline"
                                  type="button"
                                  className="text-brand-600 border-brand-200 hover:bg-brand-50"
                                  isLoading={actionLoading === inv.id + '-resend'}
                                  onClick={() => setConfirmAction({ id: inv.id, type: 'resend' })}
                                  title="Resend Email"
                                >
                                  <Mail className="h-3 w-3" />
                                </Button>
                              )}
                              {(inv.status === 'Pending' || inv.status === 'Accepted') && (
                                <Button
                                  size="sm"
                                  variant="outline"
                                  type="button"
                                  className="text-red-600 border-red-200 hover:bg-red-50"
                                  isLoading={actionLoading === inv.id + '-cancel'}
                                  onClick={() => setConfirmAction({ id: inv.id, type: 'cancel' })}
                                  title="Cancel Invitation"
                                >
                                  <Ban className="h-3 w-3" />
                                </Button>
                              )}
                              {inv.vendorApplicationId && (
                                <Link to={`/approver/onboarding/${inv.vendorApplicationId}`}>
                                  <Button size="sm" variant="primary">
                                    View App
                                  </Button>
                                </Link>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </Card>
          </div>
        ) : (
          /* Vendor Changes Table */
          <Card className="px-0 py-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-32">Request ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-32">Vendor ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Type</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-40">Submitted</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-32">Status</th>
                    <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500 w-32">Risk Level</th>
                    <th className="relative px-6 py-3"><span className="sr-only">Actions</span></th>
                  </tr>
                  {/* Filter Row */}
                  <tr className="bg-gray-50">
                    <td className="px-6 py-2">
                      <FilterInput value={crFilters.id} onChange={(v) => setCrFilters({ ...crFilters, id: v })} placeholder="Filter ID" />
                    </td>
                    <td className="px-6 py-2">
                      <FilterInput value={crFilters.vendor} onChange={(v) => setCrFilters({ ...crFilters, vendor: v })} placeholder="Filter Vendor" />
                    </td>
                    <td className="px-6 py-2">
                      <FilterSelect
                        value={crFilters.type}
                        onChange={(v) => setCrFilters({ ...crFilters, type: v })}
                        options={Object.values(RequestType)}
                      />
                    </td>
                    <td className="px-6 py-2"></td>
                    <td className="px-6 py-2">
                      <FilterSelect
                        value={crFilters.status}
                        onChange={(v) => setCrFilters({ ...crFilters, status: v })}
                        options={Object.values(ChangeRequestStatus)}
                      />
                    </td>
                    <td className="px-6 py-2">
                      <FilterSelect
                        value={crFilters.risk}
                        onChange={(v) => setCrFilters({ ...crFilters, risk: v })}
                        options={['Standard', 'High Risk']}
                      />
                    </td>
                    <td className="px-6 py-2"></td>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 bg-white">
                  {loading ? (
                    <tr><td colSpan={7} className="px-6 py-4 text-center">Loading...</td></tr>
                  ) : filteredChanges.length === 0 ? (
                    <tr><td colSpan={7} className="px-6 py-4 text-center text-gray-500">No matching change requests.</td></tr>
                  ) : (
                    filteredChanges.map((req) => {
                      const isSensitive = req.items.some(i => i.isSensitive);
                      return (
                        <tr key={req.id}>
                          <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">{req.id}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{req.vendorId}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{req.requestType}</td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                            {new Date(req.createdAt).toLocaleDateString()}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm">
                            <StatusBadge status={req.status} />
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-sm">
                            {isSensitive ? (
                              <span className="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                                High Risk
                              </span>
                            ) : (
                              <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">
                                Standard
                              </span>
                            )}
                          </td>
                          <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                            {mode === 'worklist' ? (
                              <Link to={`/approver/requests/${req.id}`}>
                                <Button size="sm" variant="outline">
                                  <CheckSquare className="mr-2 h-3 w-3" /> Review
                                </Button>
                              </Link>
                            ) : (
                              <span className="text-gray-400 text-xs">Closed</span>
                            )}
                          </td>
                        </tr>
                      );
                    })
                  )}
                </tbody>
              </table>
            </div>
          </Card>
        )}
      </div>

      <Modal
        isOpen={!!confirmAction}
        onClose={() => !actionLoading && setConfirmAction(null)}
        title={confirmAction?.type === 'resend' ? 'Resend Invitation' : 'Cancel Invitation'}
        footer={
          <div className="flex gap-3">
            <Button
              variant="secondary"
              onClick={() => setConfirmAction(null)}
              disabled={!!actionLoading}
            >
              No, Close
            </Button>
            <Button
              variant={confirmAction?.type === 'resend' ? 'primary' : 'danger'}
              onClick={() => confirmAction?.type === 'resend'
                ? handleResend(confirmAction!.id)
                : handleCancel(confirmAction!.id)}
              isLoading={!!actionLoading}
            >
              {confirmAction?.type === 'resend' ? 'Yes, Resend' : 'Yes, Cancel Access'}
            </Button>
          </div>
        }
      >
        <p className="text-gray-600 text-left">
          {confirmAction?.type === 'resend'
            ? 'Are you sure you want to resend this invitation email to the vendor? This will generate a new secure link and expire the previous one.'
            : 'Are you sure you want to CANCEL this invitation? The vendor will no longer be able to use the link and any progress may be lost.'}
        </p>
      </Modal>

      {/* Resent Link Modal */}
      <Modal
        isOpen={!!resentLink}
        onClose={() => setResentLink(null)}
        title={resentLink?.emailSent ? "Invitation Resent Successfully" : "Invitation Generated - Email Failed"}
        footer={
          <Button onClick={() => setResentLink(null)}>Done</Button>
        }
      >
        <div className="space-y-4">
          {resentLink?.emailSent ? (
            <p className="text-sm text-gray-600 text-left">
              A new invitation has been sent. You can also manually copy the link below:
            </p>
          ) : (
            <div className="bg-orange-50 border-l-4 border-orange-400 p-4">
              <div className="flex text-left">
                <div className="flex-shrink-0">
                  <Mail className="h-5 w-5 text-orange-400" />
                </div>
                <div className="ml-3">
                  <p className="text-sm text-orange-700">
                    The invitation was generated but the <strong>email could not be sent</strong>.
                    Please copy and send the link manually to the vendor.
                  </p>
                </div>
              </div>
            </div>
          )}
          <div className="flex items-center gap-2 p-3 bg-gray-50 rounded-md border border-gray-200">
            <code className="text-xs text-brand-700 break-all flex-1">{resentLink?.link}</code>
            <button
              onClick={() => handleCopy(resentLink?.link || '')}
              className="p-2 bg-white border border-gray-300 rounded hover:bg-gray-50 flex-shrink-0"
              title="Copy to clipboard"
            >
              <Copy className="h-4 w-4 text-gray-600" />
            </button>
          </div>
        </div>
      </Modal>
    </div >
  );
};