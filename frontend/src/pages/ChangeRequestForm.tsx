import React, { useEffect, useState } from 'react';
import { useForm, SubmitHandler } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { VendorService } from '../services/vendorService';
import { VendorProfileFormData, ChangeRequestItem } from '../types';
import { Button, Input, Card } from '../components/ui/Elements';

// Helper to determine field mapping (simplified for demo)
const getFieldMeta = (key: string) => {
  const mapping: Record<string, { table: string, field: string, sensitive: boolean }> = {
    name: { table: 'LFA1', field: 'NAME1', sensitive: false },
    street: { table: 'LFA1', field: 'STRAS', sensitive: false },
    city: { table: 'LFA1', field: 'ORT01', sensitive: false },
    bankAccount: { table: 'LFBK', field: 'BANKN', sensitive: true },
    // Add other mappings...
  };
  return mapping[key] || { table: 'UNKNOWN', field: key.toUpperCase(), sensitive: false };
};

export const ChangeRequestForm: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [originalData, setOriginalData] = useState<VendorProfileFormData | null>(null);

  const { register, handleSubmit, reset, formState: { errors, dirtyFields } } = useForm<VendorProfileFormData>();

  // 1. Load Current SAP Data to prefill form
  useEffect(() => {
    VendorService.getCurrentVendor().then((data) => {
      const flattened: VendorProfileFormData = {
        name: data.name,
        email: data.email,
        street: data.address.street,
        city: data.address.city,
        postalCode: data.address.postalCode,
        country: data.address.country,
        taxNumber1: data.taxNumber1 || '',
        // Just grabbing first bank for demo simplicity
        bankAccount: data.banks[0]?.bankAccount || '',
        bankKey: data.banks[0]?.bankKey || '',
        iban: data.banks[0]?.iban || ''
      };
      setOriginalData(flattened);
      reset(flattened);
      setLoading(false);
    });
  }, [reset]);

  const onSubmit: SubmitHandler<VendorProfileFormData> = async (formData) => {
    if (!originalData) return;
    setSubmitting(true);

    try {
      // 2. Compute Deltas (Core Logic)
      const deltas: ChangeRequestItem[] = [];

      (Object.keys(dirtyFields) as Array<keyof VendorProfileFormData>).forEach((key) => {
        const newVal = formData[key];
        const oldVal = originalData[key];
        
        if (newVal !== oldVal) {
          const meta = getFieldMeta(key);
          deltas.push({
            id: crypto.randomUUID(), // Local gen
            tableName: meta.table,
            fieldName: meta.field,
            oldValue: String(oldVal),
            newValue: String(newVal),
            isSensitive: meta.sensitive
          });
        }
      });

      if (deltas.length === 0) {
        alert("No changes detected.");
        setSubmitting(false);
        return;
      }

      // 3. Handle File Uploads (Mock)
      const files: File[] = []; // In a real app, bind file input state here

      // 4. Submit to Backend
      await VendorService.submitChangeRequest(deltas, files);
      
      navigate('/requests');
    } catch (error) {
      console.error(error);
      alert('Failed to submit request');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div>Loading form...</div>;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="border-b border-gray-200 pb-5">
        <h3 className="text-lg font-medium leading-6 text-gray-900">Create Change Request</h3>
        <p className="mt-1 max-w-4xl text-sm text-gray-500">
          Modify the fields below. Only changed fields will be submitted for approval.
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card title="General Data">
           <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
              <div className="sm:col-span-4">
                <Input 
                  label="Company Name" 
                  {...register('name', { required: 'Name is required' })}
                  error={errors.name?.message}
                />
              </div>
              <div className="sm:col-span-4">
                <Input 
                  label="Email Address" 
                  type="email"
                  {...register('email', { required: true })}
                />
              </div>
           </div>
        </Card>

        <Card title="Address Data">
           <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
              <div className="sm:col-span-6">
                <Input 
                  label="Street Address" 
                  {...register('street')}
                />
              </div>
              <div className="sm:col-span-3">
                <Input 
                  label="City" 
                  {...register('city')}
                />
              </div>
              <div className="sm:col-span-3">
                <Input 
                  label="Postal Code" 
                  {...register('postalCode')}
                />
              </div>
           </div>
        </Card>

        <Card title="Bank Data (Sensitive)" className="border-l-4 border-l-orange-400">
           <div className="p-2 mb-4 bg-orange-50 text-orange-800 text-sm rounded">
             Warning: Changing bank details requires uploading a Bank Letter proof and secondary approval.
           </div>
           <div className="grid grid-cols-1 gap-y-6 gap-x-4 sm:grid-cols-6">
              <div className="sm:col-span-3">
                <Input 
                  label="Bank Key / Routing" 
                  {...register('bankKey')}
                />
              </div>
              <div className="sm:col-span-3">
                <Input 
                  label="Account Number" 
                  {...register('bankAccount')}
                />
              </div>
              <div className="sm:col-span-6">
                <label className="block text-sm font-medium text-gray-700 mb-1">Proof Document (PDF)</label>
                <input type="file" className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-brand-50 file:text-brand-700 hover:file:bg-brand-100"/>
              </div>
           </div>
        </Card>

        <div className="flex justify-end space-x-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={submitting}>Submit Change Request</Button>
        </div>
      </form>
    </div>
  );
};