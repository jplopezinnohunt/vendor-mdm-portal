import React from 'react';
import { X, ArrowLeftRight, CheckCircle, XCircle } from 'lucide-react';

interface ComparisonData {
    label: string;
    newValue: string | undefined;
    existingValue: string | undefined;
}

interface VendorComparisonModalProps {
    isOpen: boolean;
    onClose: () => void;
    newVendor: {
        name?: string;
        familyName?: string;
        givenName?: string;
        dateOfBirth?: string;
        country?: string;
        accountGroup?: string;
        companyCode?: string;
    };
    existingVendor: {
        vendorName: string;
        dateOfBirth?: string;
        sapId: string;
        country: string;
        companyCode: string;
        accountGroup: string;
        sapStatus: string;
        blocked: boolean;
    };
}

export const VendorComparisonModal: React.FC<VendorComparisonModalProps> = ({
    isOpen,
    onClose,
    newVendor,
    existingVendor
}) => {
    if (!isOpen) return null;

    // Prepare comparison data
    const newVendorName = newVendor.familyName
        ? `${newVendor.familyName}, ${newVendor.givenName}`
        : newVendor.name;

    const comparisons: ComparisonData[] = [
        {
            label: 'Vendor Name',
            newValue: newVendorName,
            existingValue: existingVendor.vendorName
        },
        {
            label: 'Date of Birth',
            newValue: newVendor.dateOfBirth
                ? new Date(newVendor.dateOfBirth).toLocaleDateString()
                : undefined,
            existingValue: existingVendor.dateOfBirth
                ? new Date(existingVendor.dateOfBirth).toLocaleDateString()
                : undefined
        },
        {
            label: 'Country',
            newValue: newVendor.country,
            existingValue: existingVendor.country
        },
        {
            label: 'Account Group',
            newValue: newVendor.accountGroup,
            existingValue: existingVendor.accountGroup
        },
        {
            label: 'Company Code',
            newValue: newVendor.companyCode,
            existingValue: existingVendor.companyCode
        }
    ];

    const ComparisonRow = ({ data }: { data: ComparisonData }) => {
        const isDifferent = data.newValue !== data.existingValue;
        const hasData = data.newValue || data.existingValue;

        if (!hasData) return null;

        return (
            <div className={`border-b border-gray-200 py-4 ${isDifferent ? 'bg-yellow-50/50' : ''}`}>
                <div className="grid grid-cols-3 gap-4 items-center">
                    <div className="text-sm font-medium text-gray-700">{data.label}</div>
                    <div className="text-sm">
                        <span className={`font-medium ${isDifferent ? 'text-blue-700' : 'text-gray-900'}`}>
                            {data.newValue || '-'}
                        </span>
                    </div>
                    <div className="text-sm">
                        <span className={`font-medium ${isDifferent ? 'text-orange-700' : 'text-gray-900'}`}>
                            {data.existingValue || '-'}
                        </span>
                    </div>
                </div>
                {isDifferent && (
                    <div className="mt-1 ml-0 text-xs text-yellow-700 flex items-center gap-1">
                        <XCircle className="w-3 h-3" />
                        Difference detected
                    </div>
                )}
            </div>
        );
    };

    const hasDifferences = comparisons.some(c => c.newValue !== c.existingValue);

    return (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-hidden animate-in zoom-in-95 duration-300">
                {/* Header */}
                <div className="bg-gradient-to-r from-blue-600 to-blue-700 text-white px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <ArrowLeftRight className="w-6 h-6" />
                        <h2 className="text-xl font-semibold">Compare Vendors</h2>
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
                    {/* Column Headers */}
                    <div className="grid grid-cols-3 gap-4 mb-4 pb-3 border-b-2 border-gray-300">
                        <div className="text-sm font-bold text-gray-500 uppercase">Field</div>
                        <div className="text-sm font-bold text-blue-700 uppercase">
                            New Vendor (Creating)
                        </div>
                        <div className="text-sm font-bold text-orange-700 uppercase">
                            Existing in SAP (ID: {existingVendor.sapId})
                        </div>
                    </div>

                    {/* Comparison Rows */}
                    <div className="space-y-0">
                        {comparisons.map((comp, idx) => (
                            <ComparisonRow key={idx} data={comp} />
                        ))}
                    </div>

                    {/* Status Banner */}
                    <div className={`mt-6 p-4 rounded-lg border-l-4 ${existingVendor.sapStatus === 'Blocked'
                            ? 'bg-red-50 border-red-500'
                            : 'bg-green-50 border-green-500'
                        }`}>
                        <div className="flex items-start gap-3">
                            <div className="flex-1">
                                <h4 className="text-sm font-bold text-gray-900">Existing Vendor Status</h4>
                                <p className="text-sm text-gray-700 mt-1">
                                    {existingVendor.blocked && '🔒 '}{existingVendor.sapStatus}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Differences Summary */}
                    {hasDifferences && (
                        <div className="mt-6 p-4 bg-yellow-50 border-l-4 border-yellow-500 rounded-lg">
                            <h4 className="text-sm font-bold text-yellow-900 flex items-center gap-2">
                                <XCircle className="w-4 h-4" />
                                Differences Detected
                            </h4>
                            <p className="text-sm text-yellow-800 mt-1">
                                The new vendor and existing vendor have different information. Review carefully to determine if they are the same entity or genuinely different vendors.
                            </p>
                        </div>
                    )}

                    {!hasDifferences && (
                        <div className="mt-6 p-4 bg-green-50 border-l-4 border-green-500 rounded-lg">
                            <h4 className="text-sm font-bold text-green-900 flex items-center gap-2">
                                <CheckCircle className="w-4 h-4" />
                                No Differences
                            </h4>
                            <p className="text-sm text-green-800 mt-1">
                                The new vendor and existing vendor have identical information. This is likely a duplicate.
                            </p>
                        </div>
                    )}
                </div>

                {/* Actions */}
                <div className="border-t border-gray-200 px-6 py-4 bg-gray-50 flex justify-end gap-3">
                    <button
                        onClick={onClose}
                        className="px-6 py-2 bg-white text-gray-700 rounded-lg font-medium hover:bg-gray-100 transition-all border border-gray-300 text-sm"
                    >
                        Close Comparison
                    </button>
                </div>
            </div>
        </div>
    );
};
