import React, { useEffect, useState } from 'react';
import { VendorService } from '../services/vendorService';
import { VendorMasterData } from '../types';
import { Card, Button } from '../components/ui/Elements';
import { Link } from 'react-router-dom';
import { Edit2 } from 'lucide-react';

export const VendorProfile: React.FC = () => {
  const [data, setData] = useState<VendorMasterData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    VendorService.getCurrentVendor().then((res) => {
      setData(res);
      setLoading(false);
    });
  }, []);

  if (loading) return <div className="p-8 text-center">Loading master data from SAP...</div>;
  if (!data) return <div className="p-8 text-center text-red-500">Failed to load data.</div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Current Master Data</h1>
        <Link to="/requests/new">
          <Button variant="primary">
            <Edit2 className="mr-2 h-4 w-4" />
            Request Change
          </Button>
        </Link>
      </div>

      <p className="text-sm text-gray-500">
        This data is retrieved directly from the SAP ERP system. It is read-only. 
        To modify any field, please submit a Change Request.
      </p>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card title="General Information">
          <dl className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Vendor Name</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.name}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">SAP ID (LIFNR)</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.sapVendorId}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Legal Form</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.legalForm || '-'}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Tax ID 1</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.taxNumber1 || '-'}</dd>
            </div>
          </dl>
        </Card>

        <Card title="Address">
          <dl className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-2">
            <div className="sm:col-span-2">
              <dt className="text-sm font-medium text-gray-500">Street</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.address.street}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">City</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.address.city}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Postal Code</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.address.postalCode}</dd>
            </div>
            <div className="sm:col-span-1">
              <dt className="text-sm font-medium text-gray-500">Country</dt>
              <dd className="mt-1 text-sm text-gray-900">{data.address.country}</dd>
            </div>
          </dl>
        </Card>

        <Card title="Bank Details (Sensitive)" className="lg:col-span-2">
           {data.banks.map((bank, idx) => (
             <div key={idx} className="border-b border-gray-100 pb-4 last:border-0 last:pb-0 mb-4 last:mb-0">
               <dl className="grid grid-cols-1 gap-x-4 gap-y-6 sm:grid-cols-3">
                 <div className="sm:col-span-1">
                    <dt className="text-sm font-medium text-gray-500">Bank Country</dt>
                    <dd className="mt-1 text-sm text-gray-900">{bank.bankCountry}</dd>
                 </div>
                 <div className="sm:col-span-1">
                    <dt className="text-sm font-medium text-gray-500">Bank Key</dt>
                    <dd className="mt-1 text-sm text-gray-900">{bank.bankKey}</dd>
                 </div>
                 <div className="sm:col-span-1">
                    <dt className="text-sm font-medium text-gray-500">Account Number</dt>
                    <dd className="mt-1 text-sm font-mono text-gray-900">{bank.bankAccount}</dd>
                 </div>
               </dl>
             </div>
           ))}
        </Card>
      </div>
    </div>
  );
};