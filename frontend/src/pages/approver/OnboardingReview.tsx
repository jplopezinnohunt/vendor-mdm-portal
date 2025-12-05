
import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { VendorService } from '../../services/vendorService';
import { VendorApplication, ApplicationStatus } from '../../types';
import { Card, Button, Input } from '../../components/ui/Elements';
import { WorkflowTracker } from '../../components/ui/WorkflowTracker';
import { ArrowLeft, CheckCircle, XCircle, ShieldCheck, AlertTriangle } from 'lucide-react';

export const OnboardingReview: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [app, setApp] = useState<VendorApplication | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    if (id) {
      VendorService.getOnboardingRequestById(id).then(data => {
        if (data) setApp(data);
        setLoading(false);
      });
    }
  }, [id]);

  const handleDecision = async (status: ApplicationStatus.Approved | ApplicationStatus.Rejected) => {
    if (!app) return;
    if (status === ApplicationStatus.Rejected && !confirm("Reject this application?")) return;

    setProcessing(true);
    await VendorService.processOnboardingRequest(app.id, status);
    setProcessing(false);
    navigate('/approver/worklist');
  };

  if (loading) return <div className="p-8">Loading application details...</div>;
  if (!app) return <div className="p-8">Application not found.</div>;

  // Compute Workflow Steps based on Status and Sanction Check
  const getWorkflowSteps = () => {
    const sanctionStatus = app.sanctionCheckStatus || 'Pending';
    let sanctionStepStatus: 'complete' | 'current' | 'upcoming' | 'error' = 'upcoming';

    // Determine Sanction Step Status
    if (sanctionStatus === 'Passed') sanctionStepStatus = 'complete';
    else if (sanctionStatus === 'Failed') sanctionStepStatus = 'error';
    else if (sanctionStatus === 'Pending') sanctionStepStatus = 'current';

    const steps: { id: string; name: string; description: string; status: 'complete' | 'current' | 'upcoming' | 'error' }[] = [
      { id: '1', name: 'Vendor Submission', description: 'Application Received', status: 'complete' },
      { id: '1b', name: 'Sanction Screening', description: 'Automated Check', status: sanctionStepStatus },
      { id: '2', name: 'Internal Review', description: 'MDM Approval', status: 'current' },
      { id: '3', name: 'SAP Creation', description: 'Generate Vendor ID', status: 'upcoming' },
    ];

    // Adjust for Overall Application Status
    if (app.status === ApplicationStatus.Approved) {
      steps[1].status = 'complete'; // Sanctions must be passed to approve
      steps[2].status = 'complete';
      steps[3].status = 'complete';
    } else if (app.status === ApplicationStatus.Rejected) {
      // If failed at sanctions
      if (sanctionStatus === 'Failed') {
        steps[1].status = 'error';
        steps[2].status = 'upcoming'; // didn't reach here
      } else {
        // Failed at manual review
        steps[1].status = 'complete';
        steps[2].status = 'error';
      }
    } else {
      // In Progress logic
      if (sanctionStatus === 'Pending' || sanctionStatus === 'Failed') {
        steps[2].status = 'upcoming'; // Block internal review if sanctions pending/failed
      }
    }

    return steps;
  };

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="flex items-center text-sm text-gray-500 hover:text-gray-900">
        <ArrowLeft className="mr-1 h-4 w-4" /> Back to Worklist
      </button>

      <div className="flex flex-col md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Application Document: {app.id}</h1>
          <p className="mt-1 text-sm text-gray-500">Review prospective vendor details.</p>
        </div>
        <div className="mt-4 md:mt-0 flex flex-col items-end space-y-2">
          <div className="flex space-x-3">
            <Button
              variant="danger"
              onClick={() => handleDecision(ApplicationStatus.Rejected)}
              isLoading={processing}
              disabled={app.status !== ApplicationStatus.Submitted}
            >
              <XCircle className="mr-2 h-4 w-4" /> Reject
            </Button>
            <Button
              variant="primary"
              onClick={() => handleDecision(ApplicationStatus.Approved)}
              isLoading={processing}
              disabled={app.status !== ApplicationStatus.Submitted || app.sanctionCheckStatus === 'Failed' || app.sanctionCheckStatus === 'Pending'}
            >
              <CheckCircle className="mr-2 h-4 w-4" /> Approve & Create
            </Button>
          </div>

          {/* Service Indicators */}
          <div className="flex items-center gap-3 text-xs text-gray-500">
            <div className="flex items-center gap-1.5">
              <div className="h-1.5 w-1.5 bg-yellow-500 rounded-full animate-pulse"></div>
              <span>Application Data: Mock</span>
            </div>
            <div className="flex items-center gap-1.5">
              <div className="h-1.5 w-1.5 bg-yellow-500 rounded-full animate-pulse"></div>
              <span>Processing: Mock Only</span>
            </div>
          </div>
        </div>
      </div>

      {/* Horizontal Workflow */}
      <WorkflowTracker steps={getWorkflowSteps()} />

      {/* Read-only Form form matching VendorRegistration structure */}
      <Card title="Document Data">
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">

          {/* Sanction Status Indicator */}
          <div className="md:col-span-2 bg-gray-50 p-4 rounded-md border border-gray-200 flex items-center justify-between">
            <div className="flex items-center">
              <ShieldCheck className={`h-5 w-5 mr-2 ${app.sanctionCheckStatus === 'Passed' ? 'text-green-600' : app.sanctionCheckStatus === 'Failed' ? 'text-red-600' : 'text-yellow-600'}`} />
              <span className="text-sm font-medium text-gray-700">Sanction Screening Status</span>
            </div>
            <div>
              {app.sanctionCheckStatus === 'Passed' && (
                <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-0.5 text-xs font-medium text-green-800">
                  Passed
                </span>
              )}
              {app.sanctionCheckStatus === 'Failed' && (
                <span className="inline-flex items-center rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-medium text-red-800">
                  Failed / Blocked
                </span>
              )}
              {(!app.sanctionCheckStatus || app.sanctionCheckStatus === 'Pending') && (
                <span className="inline-flex items-center rounded-full bg-yellow-100 px-2.5 py-0.5 text-xs font-medium text-yellow-800">
                  Pending Check...
                </span>
              )}
            </div>
          </div>

          <div className="md:col-span-2">
            <Input
              label="Company Name"
              value={app.companyName}
              readOnly
              className="bg-gray-50"
            />
          </div>

          <div>
            <Input
              label="Tax ID / VAT Number"
              value={app.taxId}
              readOnly
              className="bg-gray-50"
            />
          </div>

          <div>
            <Input
              label="Contact Person"
              value={app.contactName}
              readOnly
              className="bg-gray-50"
            />
          </div>

          <div className="md:col-span-2">
            <Input
              label="Email Address"
              value={app.email}
              readOnly
              className="bg-gray-50"
            />
          </div>

          <div className="md:col-span-2 pt-4 border-t border-gray-100">
            <span className="text-sm font-medium text-gray-500">Submission Metadata</span>
            <p className="text-xs text-gray-400 mt-1">Submitted: {new Date(app.submittedAt).toLocaleString()}</p>
            <p className="text-xs text-gray-400">Application ID: {app.id}</p>
          </div>
        </div>
      </Card>
    </div>
  );
};