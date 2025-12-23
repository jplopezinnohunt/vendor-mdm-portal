import React, { useEffect, useState } from 'react';
import { useForm, SubmitHandler } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { VendorService } from '../services/vendorService';
import { VendorProfileFormData, ChangeRequestItem } from '../types';
import { Button, Input, Card } from '../components/ui/Elements';
import { useAuth } from '../context/AuthContext';
import { Search } from 'lucide-react';
import { DynamicFormSection } from '../components/DynamicFormHelper';

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
  const { user } = useAuth();
  const isApprover = user?.role === 'Approver' || user?.role === 'Admin';

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [originalData, setOriginalData] = useState<VendorProfileFormData | null>(null);

  // Approver Mode State
  const [searchId, setSearchId] = useState('');
  const [selectedVendorId, setSelectedVendorId] = useState<string | null>(null);

  const { register, handleSubmit, watch, reset, formState: { errors, dirtyFields } } = useForm<VendorProfileFormData>();

  // 1. Logic to Load Data
  const loadVendorData = async (vendorId: string) => {
    setLoading(true);
    try {
      const data = await VendorService.getCurrentVendor(vendorId);
      const flattened: VendorProfileFormData = {
        name: data.name,
        email: data.email,
        street: data.address.street,
        city: data.address.city,
        postalCode: data.address.postalCode,
        country: data.address.country,
        taxNumber1: data.taxNumber1 || '',
        bankAccount: data.banks[0]?.bankAccount || '',
        bankKey: data.banks[0]?.bankKey || '',
        iban: data.banks[0]?.iban || '',
        bankCountry: data.banks[0]?.bankCountry || '',
        swift: '', // Not in mock yet, default empty
        companyCode: data.companyCode || 'UNES',
        accountGroup: data.accountGroup || 'INDV',
        contactPerson: data.contactPerson || '',
        contactPhone: data.contactPhone || '',
        birthDate: data.birthDate || '',
        gender: data.gender || '',
        profession: data.profession || '',
        birthCountry: data.birthCountry || '',
        eventDate: data.events?.[0]?.date || ''
      };
      setOriginalData(flattened);
      reset(flattened);
      setSelectedVendorId(vendorId);
    } catch (error) {
      console.error(error);
      alert('Failed to load vendor data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isApprover) {
      // If approver, wait for selection. Don't load anything yet.
      setLoading(false);
    } else {
      // If Vendor, load their own profile (default ID or from token)
      loadVendorData('100450');
    }
  }, [isApprover, reset]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (!searchId) return;
    loadVendorData(searchId);
  };

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
      const files: File[] = [];

      // 4. Submit to Backend
      // If Approver, we pass the selected Vendor ID
      const targetVendorId = isApprover && selectedVendorId ? selectedVendorId : '100450';
      await VendorService.submitChangeRequest(deltas, files, targetVendorId);

      if (isApprover) {
        alert('Change Request initiated successfully.');
        navigate('/approver/worklist');
      } else {
        navigate('/requests');
      }
    } catch (error) {
      console.error(error);
      alert('Failed to submit request');
    } finally {
      setSubmitting(false);
    }
  };

  // ---------------- Render Logic ----------------

  // If Approver has not selected a vendor yet
  if (isApprover && !selectedVendorId) {
    return (
      <div className="max-w-xl mx-auto space-y-6 py-12">
        <div className="text-center">
          <h2 className="text-2xl font-bold text-gray-900">Initiate Master Data Change</h2>
          <p className="mt-2 text-sm text-gray-600">
            Search for a vendor by SAP ID to modify their master data.
          </p>
        </div>
        <Card title="Find Vendor">
          <form onSubmit={handleSearch} className="space-y-4">
            <Input
              label="SAP Vendor ID"
              placeholder="e.g. 100450"
              value={searchId}
              onChange={(e) => setSearchId(e.target.value)}
            />
            <Button type="submit" className="w-full flex items-center justify-center" disabled={loading}>
              <Search className="h-4 w-4 mr-2" />
              {loading ? 'Searching...' : 'Load Vendor Data'}
            </Button>
          </form>
        </Card>
      </div>
    );
  }

  if (loading) return (
    <div className="flex justify-center p-12">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
    </div>
  );

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="border-b border-gray-200 pb-5">
        <h3 className="text-lg font-medium leading-6 text-gray-900">
          {isApprover ? `Update Master Data: ${originalData?.name}` : 'Create Change Request'}
        </h3>
        <p className="mt-1 max-w-4xl text-sm text-gray-500">
          Modify the fields below. Only changed fields will be submitted.
          {isApprover && ' As an approver, low-risk changes may be auto-approved.'}
        </p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

        {/* Dynamic Sections */}
        <DynamicFormSection
          section="General"
          register={register}
          errors={errors}
          watch={watch}
          vendorType={originalData?.accountGroup || 'INDV'}
          flowType={isApprover ? 'CHANGE_INTERNAL' : 'CHANGE_VENDOR'}
          originalData={originalData || undefined}
        />

        {/* Conditional Event Details (if applicable) can still be handled by helper or inline if specific */}
        {(originalData?.accountGroup === 'EVNT' || originalData?.accountGroup === 'PART') && (
          <Card title="Event Details">
            <div className="p-4 bg-blue-50 text-blue-800 rounded mb-4">
              Event vendors have simplified address requirements.
            </div>
          </Card>
        )}

        <DynamicFormSection
          section="Address"
          register={register}
          errors={errors}
          watch={watch}
          vendorType={originalData?.accountGroup || 'INDV'}
          flowType={isApprover ? 'CHANGE_INTERNAL' : 'CHANGE_VENDOR'}
        />

        <DynamicFormSection
          section="Bank"
          register={register}
          errors={errors}
          watch={watch}
          vendorType={originalData?.accountGroup || 'INDV'}
          flowType={isApprover ? 'CHANGE_INTERNAL' : 'CHANGE_VENDOR'}
        />

        <div className="flex justify-end space-x-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={submitting}>
            {isApprover ? 'Submit Changes' : 'Submit Change Request'}
          </Button>
        </div>
      </form>
    </div >
  );
};