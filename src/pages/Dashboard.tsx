import React, { useEffect, useState } from 'react';
import { Card, Button, StatusBadge } from '../components/ui/Elements';
import { useAuth } from '../context/AuthContext';
import { VendorService } from '../services/vendorService';
import { ChangeRequest } from '../types';
import { Link } from 'react-router-dom';
import { PlusCircle, ArrowRight, FileText, CheckCircle, Clock } from 'lucide-react';

export const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [requests, setRequests] = useState<ChangeRequest[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    VendorService.getChangeRequests().then(data => {
      setRequests(data);
      setIsLoading(false);
    });
  }, []);

  const pendingCount = requests.filter(r => r.status === 'IN_REVIEW' || r.status === 'NEW').length;
  const approvedCount = requests.filter(r => r.status === 'APPROVED' || r.status === 'APPLIED').length;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Welcome, {user?.name}</h1>
        <p className="mt-1 text-sm text-gray-500">Manage your vendor profile and track change requests.</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-3">
        <Card className="flex items-center p-4">
          <div className="rounded-full bg-yellow-100 p-3 text-yellow-600">
            <Clock className="h-6 w-6" />
          </div>
          <div className="ml-4">
            <h2 className="text-sm font-medium text-gray-500">Pending Requests</h2>
            <p className="text-2xl font-semibold text-gray-900">{pendingCount}</p>
          </div>
        </Card>

        <Card className="flex items-center p-4">
          <div className="rounded-full bg-green-100 p-3 text-green-600">
            <CheckCircle className="h-6 w-6" />
          </div>
          <div className="ml-4">
            <h2 className="text-sm font-medium text-gray-500">Approved</h2>
            <p className="text-2xl font-semibold text-gray-900">{approvedCount}</p>
          </div>
        </Card>
        
        <div className="flex flex-col justify-center space-y-2">
            <Link to="/requests/new">
                <Button className="w-full justify-center">
                    <PlusCircle className="mr-2 h-5 w-5" />
                    New Request
                </Button>
            </Link>
            <Link to="/profile">
                <Button variant="outline" className="w-full justify-center">
                    View Master Data
                </Button>
            </Link>
        </div>
      </div>

      {/* Recent Requests Table */}
      <Card title="Recent Change Requests">
        {isLoading ? (
          <div className="p-4 text-center text-gray-500">Loading requests...</div>
        ) : requests.length === 0 ? (
          <div className="p-4 text-center text-gray-500">No requests found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">ID</th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Type</th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Status</th>
                  <th className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">Date</th>
                  <th className="relative px-6 py-3"><span className="sr-only">View</span></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200 bg-white">
                {requests.slice(0, 5).map((req) => (
                  <tr key={req.id}>
                    <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">{req.id}</td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">{req.requestType}</td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      <StatusBadge status={req.status} />
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                      {new Date(req.createdAt).toLocaleDateString()}
                    </td>
                    <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                      <Link to="/requests" className="text-brand-600 hover:text-brand-900">
                        Details
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <div className="mt-4 border-t border-gray-200 pt-4 text-right">
            <Link to="/requests" className="inline-flex items-center text-sm font-medium text-brand-600 hover:text-brand-500">
                View all requests <ArrowRight className="ml-1 h-4 w-4" />
            </Link>
        </div>
      </Card>
    </div>
  );
};