import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { VendorService } from '../../services/vendorService';
import { VendorApplication, ApplicationStatus } from '../../types';
import { Card, Button } from '../../components/ui/Elements';
import { WorkflowTracker } from '../../components/ui/WorkflowTracker';
import { ArrowLeft, CheckCircle, XCircle, ShieldCheck, AlertTriangle, Save } from 'lucide-react';
import { api } from '../../services/api';

export const OnboardingReview: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [app, setApp] = useState<VendorApplication | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  // Form setup
  const { register, handleSubmit, reset, watch, formState: { isDirty } } = useForm();

  useEffect(() => {
    if (id) {
      // Fetch application details
      api.get(`/review/${id}`)
        .then(response => {
          const data = response.data;
          // Parse attributes if needed
          let attributes: any = {};
          if (typeof data.attributes === 'string') {
            try {
              attributes = JSON.parse(data.attributes);
            } catch (e) {
              console.error("Failed to parse attributes", e);
            }
          } else {
            attributes = data.attributes || {};
          }

          data.attributes = attributes;
          setApp(data);

          // Flatten data for the form
          // We normalize 'DynamicRegistrationForm' nested structures to match 'CreateVendorForm' flat structure
          const flatValues = {
            companyName: data.companyName,
            taxId: data.taxId,
            contactName: data.contactName,
            email: data.email,
            // Spread root attributes (Name2, SearchTerm, etc.)
            ...attributes,
            // Flatten Address if present (DynamicForm uses nested address object)
            street1: attributes.address?.streetName || attributes.street1 || '',
            street2: attributes.address?.streetName2 || attributes.street2 || '',
            street3: attributes.address?.streetName3 || attributes.street3 || '',
            street4: attributes.address?.streetName4 || attributes.street4 || '',
            postalCode: attributes.address?.postalCode || attributes.postalCode || '',
            city: attributes.address?.city || attributes.city || '',
            country: attributes.address?.country || attributes.country || '',
            // Flatten Fax/Phone if nested
            fax: attributes.contactInfo?.fax || attributes.fax || '',
            phone: attributes.contactInfo?.phone || attributes.telephone || '',
            mobilePhone: attributes.contactInfo?.mobilePhone || attributes.mobilePhone || '',
          };

          reset(flatValues);

          setLoading(false);
        })
        .catch(err => {
          console.error("Failed to load review details", err);
          setLoading(false);
        });
    }
  }, [id, reset]);

  const onApprove = async (formData: any) => {
    if (!confirm("Approve this application and create vendor in SAP with the current data?")) return;

    setProcessing(true);
    try {
      // Send full form data as enriched attributes
      await api.post(`/review/${app?.id}/approve`, {
        comments: "Approved via Web Portal",
        enrichedAttributes: formData
      });
      navigate('/approver/worklist');
    } catch (error) {
      console.error("Approval failed", error);
      alert("Failed to approve application");
      setProcessing(false);
    }
  };

  const onReject = async () => {
    const reason = prompt("Please enter rejection reason:");
    if (!reason) return;

    setProcessing(true);
    try {
      await api.post(`/review/${app?.id}/reject`, { reason });
      navigate('/approver/worklist');
    } catch (error) {
      console.error("Rejection failed", error);
      alert("Failed to reject application");
      setProcessing(false);
    }
  };

  if (loading) return <div className="p-8">Loading application details...</div>;
  if (!app) return <div className="p-8">Application not found.</div>;

  // Compute Workflow Steps based on Status and Sanction Check
  const getWorkflowSteps = () => {
    const sanctionStatus = app.sanctionCheckStatus || 'Pending';
    let sanctionStepStatus: 'complete' | 'current' | 'upcoming' | 'error' = 'upcoming';

    if (sanctionStatus === 'Passed') sanctionStepStatus = 'complete';
    else if (sanctionStatus === 'Failed') sanctionStepStatus = 'error';
    else if (sanctionStatus === 'Pending') sanctionStepStatus = 'current';

    const steps: { id: string; name: string; description: string; status: 'complete' | 'current' | 'upcoming' | 'error' }[] = [
      { id: '1', name: 'Vendor Submission', description: 'Application Received', status: 'complete' },
      { id: '1b', name: 'Sanction Screening', description: 'Automated Check', status: sanctionStepStatus },
      { id: '2', name: 'Internal Review', description: 'Enrich & Approve', status: 'current' },
      { id: '3', name: 'SAP Creation', description: 'Generate Vendor ID', status: 'upcoming' },
    ];

    if (app.status === ApplicationStatus.Approved) {
      steps[1].status = 'complete';
      steps[2].status = 'complete';
      steps[3].status = 'complete';
    } else if (app.status === ApplicationStatus.Rejected) {
      if (sanctionStatus === 'Failed') {
        steps[1].status = 'error';
        steps[2].status = 'upcoming';
      } else {
        steps[1].status = 'complete';
        steps[2].status = 'error';
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
          <h1 className="text-2xl font-bold text-gray-900">Review Application: {app.id}</h1>
          <p className="mt-1 text-sm text-gray-500">Validate vendor data and add internal classification.</p>
        </div>
        <div className="mt-4 md:mt-0 flex flex-col items-end space-y-2">
          <div className="flex space-x-3">
            <Button
              variant="danger"
              onClick={onReject}
              isLoading={processing}
              disabled={app.status !== ApplicationStatus.Submitted && app.status !== ApplicationStatus.PendingReview}
            >
              <XCircle className="mr-2 h-4 w-4" /> Reject
            </Button>
            <Button
              variant="primary"
              onClick={handleSubmit(onApprove)}
              isLoading={processing}
              disabled={(app.status !== ApplicationStatus.Submitted && app.status !== ApplicationStatus.PendingReview) || app.sanctionCheckStatus === 'Failed' || app.sanctionCheckStatus === 'Pending'}
            >
              <CheckCircle className="mr-2 h-4 w-4" /> Approve & Create
            </Button>
          </div>
        </div>
      </div>

      <WorkflowTracker steps={getWorkflowSteps()} />

      <form className="space-y-6">

        {/* INTERNAL ENRICHMENT SECTION */}
        <Card title="Internal Enrichment (Required for SAP)" className="border-l-4 border-l-blue-500">
          <div className="bg-blue-50 p-4 mb-4 rounded text-sm text-blue-800">
            Please classify this vendor before approval. These fields are required for SAP creation.
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Account Group *</label>
              <select {...register('accountGroup')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500">
                <option value="">Select Group...</option>
                <option value="INDV">Individual (INDV)</option>
                <option value="HQSU">Company (HQSU)</option>
                <option value="NGOS">NGO (NGOS)</option>
                <option value="EVNT">Event/Meeting (EVNT)</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Currency *</label>
              <select {...register('currency')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500">
                <option value="EUR">EUR</option>
                <option value="USD">USD</option>
                <option value="GBP">GBP</option>
                <option value="CHF">CHF</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Tax Code 1</label>
              <input {...register('taxCode1')} className="w-full px-3 py-2 border rounded focus:ring-blue-500 focus:border-blue-500" placeholder="e.g. 10" />
            </div>
            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Tax Code 2</label>
              <input {...register('taxCode2')} className="w-full px-3 py-2 border rounded focus:ring-blue-500 focus:border-blue-500" placeholder="e.g. 20" />
            </div>
            <div className="md:col-span-2">
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Permitted Payee (SAP ID)</label>
              <input {...register('permittedPayee')} className="w-full px-3 py-2 border rounded focus:ring-blue-500 focus:border-blue-500" placeholder="Alternative payee ID if applicable" />
            </div>
          </div>
        </Card>

        <Card title="Vendor Submitted Data">
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">

            {/* Sanction Status (Read Only) */}
            <div className="md:col-span-2 bg-gray-50 p-4 rounded-md border border-gray-200 flex items-center justify-between">
              <div className="flex items-center">
                <ShieldCheck className={`h-5 w-5 mr-2 ${app.sanctionCheckStatus === 'Passed' ? 'text-green-600' : app.sanctionCheckStatus === 'Failed' ? 'text-red-600' : 'text-yellow-600'}`} />
                <span className="text-sm font-medium text-gray-700">Sanction Screening Status</span>
              </div>
              <div>
                <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${app.sanctionCheckStatus === 'Passed' ? 'bg-green-100 text-green-800' : app.sanctionCheckStatus === 'Failed' ? 'bg-red-100 text-red-800' : 'bg-yellow-100 text-yellow-800'}`}>
                  {app.sanctionCheckStatus || 'Pending'}
                </span>
              </div>
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Company / Legal Name</label>
              <input {...register('companyName')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500" />
            </div>

            {/* Extended Names */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Name 2</label>
              <input {...register('name2')} className="w-full px-3 py-2 border rounded text-sm" placeholder="Continuation of name" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Name 3</label>
              <input {...register('name3')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Name 4</label>
              <input {...register('name4')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Search Term 1</label>
              <input {...register('searchTerm1')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Tax ID (1)</label>
              <input {...register('taxId')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tax Number 2 (Optional)</label>
              <input {...register('taxNumber2')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Primary Contact</label>
              <input {...register('contactName')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500" />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-bold text-gray-700 mb-1 uppercase tracking-wider">Email</label>
              <input {...register('email')} className="w-full px-3 py-2 border rounded bg-white focus:ring-blue-500 focus:border-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Fax</label>
              <input {...register('fax')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>

            <div className="md:col-span-2 pt-4 border-t">
              <h3 className="text-sm font-bold text-gray-900 mb-4">Extended Address Details</h3>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">House No</label>
              <input {...register('houseNo')} className="w-full px-3 py-2 border rounded text-sm" placeholder="No" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Street</label>
              <input {...register('street1')} className="w-full px-3 py-2 border rounded text-sm" placeholder="Street" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Street 2</label>
              <input {...register('street2')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Street 3</label>
              <input {...register('street3')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Street 4</label>
              <input {...register('street4')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Postal Code</label>
              <input {...register('postalCode')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">City</label>
              <input {...register('city')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Country</label>
              <input {...register('country')} className="w-full px-3 py-2 border rounded text-sm" />
            </div>


            <div className="md:col-span-2 pt-4 border-t border-gray-100">
              <span className="text-sm font-medium text-gray-500">Submission Metadata</span>
              <p className="text-xs text-gray-400 mt-1">Submitted: {new Date(app.submittedAt).toLocaleString()}</p>
              <p className="text-xs text-gray-400">Application ID: {app.id}</p>
            </div>
          </div>
        </Card>
      </form>
    </div>
  );
};