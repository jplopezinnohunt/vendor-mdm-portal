import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { VendorService } from '../../services/vendorService';
import { ChangeRequest, ChangeRequestStatus, VendorApplication, ApplicationStatus, RequestType } from '../../types';
import { Card, StatusBadge, Button } from '../../components/ui/Elements';
import { CheckSquare, UserPlus, FileText, Search, Filter } from 'lucide-react';

interface ApproverDashboardProps {
  mode?: 'worklist' | 'history';
}

export const ApproverDashboard: React.FC<ApproverDashboardProps> = ({ mode = 'worklist' }) => {
  const [changeRequests, setChangeRequests] = useState<ChangeRequest[]>([]);
  const [onboardingRequests, setOnboardingRequests] = useState<VendorApplication[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'onboarding' | 'changes'>('onboarding');

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
      
      const [allChangeRequests, allOnboarding] = await Promise.all([
        VendorService.getAllChangeRequests(),
        VendorService.getOnboardingRequests()
      ]);

      if (mode === 'worklist') {
         // Filter for items that need attention
        const pendingChanges = allChangeRequests.filter(r => 
            r.status === ChangeRequestStatus.New || 
            r.status === ChangeRequestStatus.InReview ||
            r.status === ChangeRequestStatus.Applied 
        );
        const pendingOnboarding = allOnboarding.filter(a => a.status === ApplicationStatus.Submitted);

        setChangeRequests(pendingChanges.filter(r => r.status !== ChangeRequestStatus.Applied && r.status !== ChangeRequestStatus.Approved && r.status !== ChangeRequestStatus.Rejected));
        setOnboardingRequests(pendingOnboarding);
      } else {
        // History Mode: Final statuses
        const historyChanges = allChangeRequests.filter(r => 
            r.status === ChangeRequestStatus.Approved || 
            r.status === ChangeRequestStatus.Rejected ||
            r.status === ChangeRequestStatus.Applied ||
            r.status === ChangeRequestStatus.Error
        );
        const historyOnboarding = allOnboarding.filter(a => 
            a.status === ApplicationStatus.Approved || 
            a.status === ApplicationStatus.Rejected
        );

        setChangeRequests(historyChanges);
        setOnboardingRequests(historyOnboarding);
      }
    
      setLoading(false);
    };

    loadData();
  }, [mode]);

  const totalCount = changeRequests.length + onboardingRequests.length;
  const highRiskCount = changeRequests.filter(r => r.items.some(i => i.isSensitive)).length;

  // Filter Logic
  const filteredOnboarding = onboardingRequests.filter(app => 
    app.id.toLowerCase().includes(obFilters.id.toLowerCase()) &&
    app.companyName.toLowerCase().includes(obFilters.company.toLowerCase()) &&
    app.taxId.toLowerCase().includes(obFilters.tax.toLowerCase()) &&
    (obFilters.status === '' || app.status === obFilters.status)
  );

  const filteredChanges = changeRequests.filter(req => {
    const isSensitive = req.items.some(i => i.isSensitive);
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
    
    // Status is Submitted, check sanctions
    if (app.sanctionCheckStatus === 'Pending') return { label: 'Sanction Screening', style: 'bg-yellow-100 text-yellow-800' };
    if (app.sanctionCheckStatus === 'Failed') return { label: 'Sanction Failed', style: 'bg-red-100 text-red-800' };
    
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
      </div>

      {/* KPI Cards - Only show in Worklist mode */}
      {mode === 'worklist' && (
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6">
          <dt className="truncate text-sm font-medium text-gray-500">Total Pending</dt>
          <dd className="mt-1 text-3xl font-semibold tracking-tight text-gray-900">{totalCount}</dd>
        </div>
        <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6">
          <dt className="truncate text-sm font-medium text-gray-500">New Onboarding</dt>
          <dd className="mt-1 text-3xl font-semibold tracking-tight text-blue-600">{onboardingRequests.length}</dd>
        </div>
        <div className="bg-white overflow-hidden rounded-lg shadow px-4 py-5 sm:p-6">
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
            className={`${
              activeTab === 'onboarding'
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
            className={`${
              activeTab === 'changes'
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
                       <FilterInput value={obFilters.id} onChange={(v) => setObFilters({...obFilters, id: v})} placeholder="Filter ID" />
                    </td>
                    <td className="px-6 py-2">
                       <FilterInput value={obFilters.company} onChange={(v) => setObFilters({...obFilters, company: v})} placeholder="Filter Name" />
                    </td>
                    <td className="px-6 py-2">
                       <FilterInput value={obFilters.tax} onChange={(v) => setObFilters({...obFilters, tax: v})} placeholder="Filter Tax" />
                    </td>
                    <td className="px-6 py-2"></td>
                    <td className="px-6 py-2">
                       <FilterSelect 
                         value={obFilters.status} 
                         onChange={(v) => setObFilters({...obFilters, status: v})} 
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
                    )})
                  )}
                </tbody>
              </table>
            </div>
          </Card>
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
                       <FilterInput value={crFilters.id} onChange={(v) => setCrFilters({...crFilters, id: v})} placeholder="Filter ID" />
                    </td>
                    <td className="px-6 py-2">
                       <FilterInput value={crFilters.vendor} onChange={(v) => setCrFilters({...crFilters, vendor: v})} placeholder="Filter Vendor" />
                    </td>
                    <td className="px-6 py-2">
                        <FilterSelect 
                         value={crFilters.type} 
                         onChange={(v) => setCrFilters({...crFilters, type: v})} 
                         options={Object.values(RequestType)} 
                       />
                    </td>
                    <td className="px-6 py-2"></td>
                    <td className="px-6 py-2">
                       <FilterSelect 
                         value={crFilters.status} 
                         onChange={(v) => setCrFilters({...crFilters, status: v})} 
                         options={Object.values(ChangeRequestStatus)} 
                       />
                    </td>
                    <td className="px-6 py-2">
                       <FilterSelect 
                         value={crFilters.risk} 
                         onChange={(v) => setCrFilters({...crFilters, risk: v})} 
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
    </div>
  );
};