import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Button, Card } from '../components/ui/Elements';
import { X, User, MapPin, Building2, CreditCard } from 'lucide-react';

interface VendorData {
    sapId: string;
    vendorName: string;
    dateOfBirth?: string;
    reqId?: string;
    country: string;
    companyCode: string;
    accountGroup: string;
    sapStatus: string;
    blocked: boolean;
    // Extended fields from SAP
    street?: string;
    city?: string;
    postalCode?: string;
    email?: string;
    phone?: string;
    taxNumber?: string;
    bankAccount?: string;
    iban?: string;
    swift?: string;
}

export const ViewVendor: React.FC = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [loading, setLoading] = useState(true);
    const [vendorData, setVendorData] = useState<VendorData | null>(null);

    useEffect(() => {
        // Get vendor data from query params (passed from duplicate detection modal)
        const sapId = searchParams.get('sapId');
        const vendorName = searchParams.get('vendorName');
        const dateOfBirth = searchParams.get('dateOfBirth');
        const country = searchParams.get('country');
        const companyCode = searchParams.get('companyCode');
        const accountGroup = searchParams.get('accountGroup');
        const sapStatus = searchParams.get('sapStatus');
        const blocked = searchParams.get('blocked') === 'true';
        const reqId = searchParams.get('reqId');

        if (sapId && vendorName) {
            // In a real implementation, you would fetch full vendor details from API
            // For now, we'll use the query params
            setVendorData({
                sapId,
                vendorName,
                dateOfBirth: dateOfBirth || undefined,
                reqId: reqId || undefined,
                country: country || '',
                companyCode: companyCode || '',
                accountGroup: accountGroup || '',
                sapStatus: sapStatus || 'Valid',
                blocked
            });
            setLoading(false);
        } else {
            setLoading(false);
        }
    }, [searchParams]);

    if (loading) {
        return (
            <div className="flex justify-center p-12">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
            </div>
        );
    }

    if (!vendorData) {
        return (
            <div className="max-w-2xl mx-auto py-12">
                <Card>
                    <div className="text-center py-8">
                        <p className="text-gray-600">No vendor data available</p>
                        <Button onClick={() => navigate(-1)} variant="secondary" className="mt-4">
                            Go Back
                        </Button>
                    </div>
                </Card>
            </div>
        );
    }

    const InfoRow = ({ label, value }: { label: string; value?: string | null }) => (
        <div className="py-3 border-b border-gray-100 last:border-0">
            <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">{label}</dt>
            <dd className="text-sm text-gray-900 font-medium">{value || '-'}</dd>
        </div>
    );

    return (
        <div className="max-w-4xl mx-auto py-8 space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900">Vendor Details</h1>
                    <p className="mt-2 text-sm text-gray-600">Read-only view of existing vendor record</p>
                </div>
                <button
                    onClick={() => navigate(-1)}
                    className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                >
                    <X className="w-4 h-4" />
                    Close
                </button>
            </div>

            {/* Status Banner */}
            <div className={`px-6 py-4 rounded-lg border-l-4 ${vendorData.sapStatus === 'Valid' ? 'bg-green-50 border-green-500' :
                vendorData.sapStatus === 'Blocked' ? 'bg-red-50 border-red-500' :
                    'bg-gray-50 border-gray-500'
                }`}>
                <div className="flex items-center justify-between">
                    <div>
                        <h3 className="text-sm font-bold text-gray-900">SAP Status</h3>
                        <p className="text-sm text-gray-700 mt-1">
                            {vendorData.blocked && '🔒 '}
                            {vendorData.sapStatus}
                        </p>
                    </div>
                    <div className="text-right">
                        <p className="text-xs text-gray-500">SAP ID</p>
                        <p className="text-sm font-mono font-bold text-blue-600">{vendorData.sapId}</p>
                    </div>
                </div>
            </div>

            {/* Basic Information */}
            <Card>
                <div className="flex items-center gap-2 pb-4 border-b border-gray-200 mb-4">
                    <User className="w-5 h-5 text-blue-600" />
                    <h2 className="text-lg font-bold text-gray-900">Basic Information</h2>
                </div>
                <dl className="space-y-0">
                    <InfoRow label="Vendor Name" value={vendorData.vendorName} />
                    <InfoRow label="Date of Birth" value={vendorData.dateOfBirth ? new Date(vendorData.dateOfBirth).toLocaleDateString() : undefined} />
                    <InfoRow label="Account Group" value={vendorData.accountGroup} />
                    <InfoRow label="Company Code" value={vendorData.companyCode} />
                    {vendorData.reqId && <InfoRow label="Request ID" value={vendorData.reqId} />}
                </dl>
            </Card>

            {/* Address Information */}
            <Card>
                <div className="flex items-center gap-2 pb-4 border-b border-gray-200 mb-4">
                    <MapPin className="w-5 h-5 text-blue-600" />
                    <h2 className="text-lg font-bold text-gray-900">Address Information</h2>
                </div>
                <dl className="space-y-0">
                    <InfoRow label="Street" value={vendorData.street} />
                    <InfoRow label="City" value={vendorData.city} />
                    <InfoRow label="Postal Code" value={vendorData.postalCode} />
                    <InfoRow label="Country" value={vendorData.country} />
                </dl>
            </Card>

            {/* Contact Information */}
            <Card>
                <div className="flex items-center gap-2 pb-4 border-b border-gray-200 mb-4">
                    <Building2 className="w-5 h-5 text-blue-600" />
                    <h2 className="text-lg font-bold text-gray-900">Contact Information</h2>
                </div>
                <dl className="space-y-0">
                    <InfoRow label="Email" value={vendorData.email} />
                    <InfoRow label="Phone" value={vendorData.phone} />
                    <InfoRow label="Tax Number" value={vendorData.taxNumber} />
                </dl>
            </Card>

            {/* Banking Information */}
            <Card>
                <div className="flex items-center gap-2 pb-4 border-b border-gray-200 mb-4">
                    <CreditCard className="w-5 h-5 text-blue-600" />
                    <h2 className="text-lg font-bold text-gray-900">Banking Information</h2>
                </div>
                <dl className="space-y-0">
                    <InfoRow label="Bank Account" value={vendorData.bankAccount} />
                    <InfoRow label="IBAN" value={vendorData.iban} />
                    <InfoRow label="SWIFT/BIC" value={vendorData.swift} />
                </dl>
            </Card>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-4">
                <Button onClick={() => navigate(-1)} variant="secondary">
                    Close
                </Button>
            </div>
        </div>
    );
};
