import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { VendorService } from '../../services/vendorService';
import { ChangeRequest, ChangeRequestStatus } from '../../types';
import { Card, Button, StatusBadge } from '../../components/ui/Elements';
import { WorkflowTracker } from '../../components/ui/WorkflowTracker';
import { ArrowLeft, CheckCircle, XCircle, FileText } from 'lucide-react';

export const RequestReview: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [request, setRequest] = useState<ChangeRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    if (id) {
      VendorService.getChangeRequestById(id).then(data => {
        if (data) setRequest(data);
        setLoading(false);
      });
    }
  }, [id]);

  const handleDecision = async (status: ChangeRequestStatus.Approved | ChangeRequestStatus.Rejected) => {
    if (!request) return;
    if (status === ChangeRequestStatus.Rejected && !confirm("Are you sure you want to reject this request?")) return;

    setProcessing(true);
    await VendorService.processChangeRequest(request.id, status);
    setProcessing(false);
    navigate('/approver/worklist');
  };

  if (loading) return <div>Loading details...</div>;
  if (!request) return <div>Request not found</div>;

  // Compute Workflow Steps
  const getWorkflowSteps = () => {
    const isHighRisk = request.items.some(i => i.isSensitive);
    
    // Base steps
    const steps: { id: string; name: string; description: string; status: 'complete' | 'current' | 'upcoming' | 'error' }[] = [
        { id: '1', name: 'Submitted', description: 'By Vendor', status: 'complete' },
        { id: '2', name: 'Internal Review', description: 'MDM Team', status: 'current' },
    ];

    // High risk adds approval step (simplified for visual)
    if (isHighRisk) {
        steps.push({ id: '2b', name: 'Secondary Approval', description: 'Finance/Compliance', status: 'upcoming' });
    }

    steps.push({ id: '3', name: 'Apply to SAP', description: 'Auto-Commit', status: 'upcoming' });

    // Update statuses based on request.status
    if (request.status === ChangeRequestStatus.Approved || request.status === ChangeRequestStatus.Applied) {
        steps.forEach(s => s.status = 'complete');
    } else if (request.status === ChangeRequestStatus.Rejected) {
        steps[1].status = 'error';
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
          <h1 className="text-2xl font-bold text-gray-900">Review Change Request</h1>
          <p className="mt-1 text-sm text-gray-500">ID: {request.id} • Vendor: {request.vendorId}</p>
        </div>
        <div className="mt-4 md:mt-0 flex space-x-3">
          <Button 
            variant="danger" 
            onClick={() => handleDecision(ChangeRequestStatus.Rejected)}
            isLoading={processing}
            disabled={request.status !== ChangeRequestStatus.New && request.status !== ChangeRequestStatus.InReview}
          >
            <XCircle className="mr-2 h-4 w-4" /> Reject
          </Button>
          <Button 
            variant="primary" 
            onClick={() => handleDecision(ChangeRequestStatus.Approved)}
            isLoading={processing}
            disabled={request.status !== ChangeRequestStatus.New && request.status !== ChangeRequestStatus.InReview}
          >
            <CheckCircle className="mr-2 h-4 w-4" /> Approve
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Main Details */}
        <div className="lg:col-span-2 space-y-6">
          <Card title="Workflow Progress">
             <WorkflowTracker steps={getWorkflowSteps()} />
          </Card>

          <Card title="Field Changes">
             <div className="overflow-hidden">
               <table className="min-w-full divide-y divide-gray-200">
                 <thead className="bg-gray-50">
                   <tr>
                     <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Field</th>
                     <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Original Value</th>
                     <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Requested Value</th>
                   </tr>
                 </thead>
                 <tbody className="divide-y divide-gray-200 bg-white">
                   {request.items.map(item => (
                     <tr key={item.id} className={item.isSensitive ? 'bg-orange-50' : ''}>
                       <td className="px-6 py-4 text-sm font-medium text-gray-900">
                         {item.fieldName} 
                         {item.isSensitive && <span className="ml-2 text-xs text-red-600 font-bold">(Sensitive)</span>}
                       </td>
                       <td className="px-6 py-4 text-sm text-gray-500 font-mono">{item.oldValue}</td>
                       <td className="px-6 py-4 text-sm text-gray-900 font-bold font-mono bg-yellow-50">{item.newValue}</td>
                     </tr>
                   ))}
                 </tbody>
               </table>
             </div>
          </Card>
        </div>

        {/* Sidebar Info */}
        <div className="space-y-6">
           <Card title="Request Info">
             <dl className="space-y-3">
               <div>
                 <dt className="text-sm font-medium text-gray-500">Type</dt>
                 <dd className="text-sm text-gray-900">{request.requestType}</dd>
               </div>
               <div>
                 <dt className="text-sm font-medium text-gray-500">Created</dt>
                 <dd className="text-sm text-gray-900">{new Date(request.createdAt).toLocaleString()}</dd>
               </div>
                <div>
                 <dt className="text-sm font-medium text-gray-500">Current Status</dt>
                 <dd className="text-sm text-gray-900"><StatusBadge status={request.status} /></dd>
               </div>
             </dl>
           </Card>

           <Card title="Attachments">
             {request.attachments.length === 0 ? (
               <p className="text-sm text-gray-500">No documents attached.</p>
             ) : (
               <ul className="divide-y divide-gray-200">
                 {request.attachments.map(att => (
                   <li key={att.id} className="py-3 flex items-center justify-between">
                     <div className="flex items-center">
                       <FileText className="h-5 w-5 text-gray-400 mr-2" />
                       <span className="text-sm font-medium text-brand-600 truncate max-w-[150px]">{att.fileName}</span>
                     </div>
                     <button className="text-xs text-gray-500 hover:text-gray-700 underline">View</button>
                   </li>
                 ))}
               </ul>
             )}
           </Card>
        </div>
      </div>
    </div>
  );
};