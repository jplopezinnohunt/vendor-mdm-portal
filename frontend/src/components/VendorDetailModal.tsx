import React from 'react';
import { X, User, MapPin, Building2, CreditCard } from 'lucide-react';

interface VendorDetailModalProps {
    isOpen: boolean;
    onClose: () => void;
    vendor: {
        vendorName: string;
        dateOfBirth?: string;
        sapId: string;
        reqId?: string;
        country: string;
        companyCode: string;
        accountGroup: string;
        sapStatus: string;
        blocked: boolean;
    };
}

export const VendorDetailModal: React.FC<VendorDetailModalProps> = ({
    isOpen,
    onClose,
    vendor
}) => {
    if (!isOpen) return null;

    const InfoRow = ({ label, value }: { label: string; value?: string | null }) => (
        <div className="py-3 border-b border-gray-100 last:border-0">
            <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">{label}</dt>
            <dd className="text-sm text-gray-900 font-medium">{value || '-'}</dd>
        </div>
    );

    return (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-hidden animate-in zoom-in-95 duration-300">
                {/* Header */}
                <div className="bg-gradient-to-r from-purple-600 to-purple-700 text-white px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <User className="w-6 h-6" />
                        <h2 className="text-xl font-semibold">Vendor Details</h2>
                    </div>
                    <button
                        onClick={onClose}
                        className="p-1 hover:bg-white/20 rounded-full transition-colors"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                {/* Content */}
                <div className="p-6 overflow-y-auto max-h-[calc(90vh-140px)]">
                    {/* Status Banner */}
                    <div className={`mb-6 px-6 py-4 rounded-lg border-l-4 ${vendor.sapStatus === 'Valid' ? 'bg-green-50 border-green-500' :
                            vendor.sapStatus === 'Blocked' ? 'bg-red-50 border-red-500' :
                                'bg-gray-50 border-gray-500'
                        }`}>
                        <div className="flex items-center justify-between">
                            <div>
                                <h3 className="text-sm font-bold text-gray-900">SAP Status</h3>
                                <p className="text-sm text-gray-700 mt-1">
                                    {vendor.blocked && '🔒 '}
                                    {vendor.sapStatus}
                                </p>
                            </div>
                            <div className="text-right">
                                <p className="text-xs text-gray-500">SAP ID</p>
                                <p className="text-sm font-mono font-bold text-blue-600">{vendor.sapId}</p>
                            </div>
                        </div>
                    </div>

                    {/* Basic Information */}
                    <div className="mb-6">
                        <div className="flex items-center gap-2 pb-3 border-b-2 border-gray-200 mb-4">
                            <User className="w-5 h-5 text-purple-600" />
                            <h3 className="text-lg font-bold text-gray-900">Basic Information</h3>
                        </div>
                        <dl className="space-y-0">
                            <InfoRow label="Vendor Name" value={vendor.vendorName} />
                            <InfoRow
                                label="Date of Birth"
                                value={vendor.dateOfBirth ? new Date(vendor.dateOfBirth).toLocaleDateString() : undefined}
                            />
                            <InfoRow label="Account Group" value={vendor.accountGroup} />
                            <InfoRow label="Company Code" value={vendor.companyCode} />
                            {vendor.reqId && <InfoRow label="Request ID" value={vendor.reqId} />}
                        </dl>
                    </div>

                    {/* Address Information */}
                    <div className="mb-6">
                        <div className="flex items-center gap-2 pb-3 border-b-2 border-gray-200 mb-4">
                            <MapPin className="w-5 h-5 text-purple-600" />
                            <h3 className="text-lg font-bold text-gray-900">Address Information</h3>
                        </div>
                        <dl className="space-y-0">
                            <InfoRow label="Country" value={vendor.country} />
                        </dl>
                    </div>

                    {/* Note */}
                    <div className="mt-6 p-4 bg-blue-50 border-l-4 border-blue-500 rounded-lg">
                        <p className="text-sm text-blue-800">
                            <strong>Note:</strong> This is a summary view. Full vendor details including banking information
                            are available in the SAP system.
                        </p>
                    </div>
                </div>

                {/* Actions */}
                <div className="border-t border-gray-200 px-6 py-4 bg-gray-50 flex justify-end gap-3">
                    <button
                        onClick={onClose}
                        className="px-6 py-2 bg-white text-gray-700 rounded-lg font-medium hover:bg-gray-100 transition-all border border-gray-300 text-sm"
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
};
