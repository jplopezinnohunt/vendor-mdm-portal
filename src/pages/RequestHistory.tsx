import React, { useEffect, useState } from 'react';
import { VendorService } from '../services/vendorService';
import { ChangeRequest, ChangeRequestStatus } from '../types';
import { Card, StatusBadge } from '../components/ui/Elements';

export const RequestHistory: React.FC = () => {
  const [requests, setRequests] = useState<ChangeRequest[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    VendorService.getChangeRequests().then(data => {
      // Filter to show only historical/finalized requests
      const history = data.filter(r => 
        r.status === ChangeRequestStatus.Approved || 
        r.status === ChangeRequestStatus.Rejected ||
        r.status === ChangeRequestStatus.Applied ||
        r.status === ChangeRequestStatus.Error
      );
      setRequests(history);
      setLoading(false);
    });
  }, []);

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Request History</h1>
          <p className="mt-2 text-sm text-gray-700">
            A history of approved and rejected requests.
          </p>
        </div>
      </div>
      
      <Card className="px-0 py-0">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Request ID
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Type
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Status
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Created
                </th>
                <th scope="col" className="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider text-gray-500">
                  Sensitive
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {loading ? (
                 <tr><td colSpan={5} className="px-6 py-4 text-center">Loading...</td></tr>
              ) : requests.length === 0 ? (
                 <tr><td colSpan={5} className="px-6 py-4 text-center text-gray-500">No history found.</td></tr>
              ) : requests.map((req) => {
                const isSensitive = req.items.some(i => i.isSensitive);
                return (
                <tr key={req.id}>
                  <td className="whitespace-nowrap px-6 py-4 text-sm font-medium text-gray-900">
                    {req.id}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {req.requestType}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm">
                    <StatusBadge status={req.status} />
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {new Date(req.createdAt).toLocaleDateString()}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-sm text-gray-500">
                    {isSensitive ? (
                        <span className="text-red-600 font-bold text-xs">HIGH RISK</span>
                    ) : (
                        <span className="text-gray-400 text-xs">Standard</span>
                    )}
                  </td>
                </tr>
              )})}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};