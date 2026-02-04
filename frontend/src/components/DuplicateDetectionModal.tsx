import React, { useState } from 'react';
import { Eye, GitCompare, AlertTriangle } from 'lucide-react';
import { AccessibleModal } from './ui/AccessibleModal';
import { VendorComparisonModal } from './VendorComparisonModal';
import { VendorDetailModal } from './VendorDetailModal';

import { DuplicateVendor } from '../types/vendor';

interface DuplicateDetectionModalProps {
    isOpen: boolean;
    onClose: () => void;
    onProceed: () => void;
    duplicates: DuplicateVendor[];
    newVendorData?: {
        name?: string;
        familyName?: string;
        givenName?: string;
        dateOfBirth?: string;
        country?: string;
        accountGroup?: string;
        companyCode?: string;
    };
}

/**
 * DuplicateDetectionModal - Displays potential duplicate vendors for verification.
 * Brain v1.2.0 Pattern 29: Accessibility (WCAG) - Uses AccessibleModal primitive.
 */
export const DuplicateDetectionModal: React.FC<DuplicateDetectionModalProps> = ({
    isOpen,
    onClose,
    onProceed,
    duplicates,
    newVendorData
}) => {
    const [selectedVendor, setSelectedVendor] = useState<DuplicateVendor | null>(null);
    const [showCompareModal, setShowCompareModal] = useState(false);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

    if (!isOpen) return null;

    const handleViewDetails = (vendor: DuplicateVendor) => {
        setSelectedVendor(vendor);
        setShowDetailsModal(true);
    };

    const handleCompare = (vendor: DuplicateVendor) => {
        setSelectedVendor(vendor);
        setShowCompareModal(true);
    };

    const closeCompareModal = () => {
        setShowCompareModal(false);
        setSelectedVendor(null);
    };

    const closeDetailsModal = () => {
        setShowDetailsModal(false);
        setSelectedVendor(null);
    };

    return (
        <>
            <AccessibleModal
                isOpen={isOpen}
                onClose={onClose}
                title="Vendor(s) which already exist in SAP"
                description="Vendors with similar information found. Please verify your vendor is not already on the list."
                headerIcon={<AlertTriangle className="w-6 h-6" />}
                maxWidthClassName="max-w-5xl"
                footer={
                    <div className="flex items-center justify-between w-full">
                        <button
                            onClick={onProceed}
                            className="text-blue-600 hover:text-blue-800 font-medium px-4 py-2 border border-blue-600 rounded hover:bg-blue-50 transition-all text-sm"
                        >
                            Not in list, proceed with new request
                        </button>
                        <button
                            onClick={onClose}
                            className="px-8 py-2 bg-gray-100 text-gray-700 rounded font-medium hover:bg-gray-200 transition-all border border-gray-200 text-sm"
                        >
                            Close
                        </button>
                    </div>
                }
            >
                {/* Results Table */}
                <div className="border border-gray-200 rounded-lg overflow-hidden shadow-sm">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-[#1e40af] text-white">
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Vendor Name</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Date of Birth</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">SAP Id</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Req. Id</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Country</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Company</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20">Account Group</th>
                                <th className="px-4 py-3 text-sm font-semibold border-r border-white/20 text-center">Status</th>
                                <th className="px-4 py-3 text-sm font-semibold text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200">
                            {duplicates.map((vendor, idx) => (
                                <tr
                                    key={vendor.sapId}
                                    className={`${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'} hover:bg-blue-50/50 transition-colors group`}
                                >
                                    <td className="px-4 py-4 text-sm font-medium text-gray-900 border-r border-gray-200">{vendor.vendorName}</td>
                                    <td className="px-4 py-4 text-sm text-gray-600 border-r border-gray-200">
                                        {vendor.dateOfBirth ? new Date(vendor.dateOfBirth).toLocaleDateString() : '-'}
                                    </td>
                                    <td className="px-4 py-4 text-sm font-mono text-blue-600 border-r border-gray-200">{vendor.sapId}</td>
                                    <td className="px-4 py-4 text-sm text-gray-600 border-r border-gray-200">{vendor.reqId || '-'}</td>
                                    <td className="px-4 py-4 text-sm text-gray-600 border-r border-gray-200">{vendor.country}</td>
                                    <td className="px-4 py-4 text-sm text-gray-600 border-r border-gray-200">{vendor.companyCode}</td>
                                    <td className="px-4 py-4 text-sm text-gray-600 border-r border-gray-200">{vendor.accountGroup}</td>
                                    <td className="px-4 py-4 text-sm text-center border-r border-gray-200">
                                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${vendor.sapStatus === 'Valid' ? 'bg-green-100 text-green-800' :
                                            vendor.sapStatus === 'Blocked' ? 'bg-red-100 text-red-800' :
                                                'bg-gray-100 text-gray-800'
                                            }`}>
                                            {vendor.blocked && '🔒 '}
                                            {vendor.sapStatus}
                                        </span>
                                    </td>
                                    <td className="px-4 py-4 text-sm text-center">
                                        <div className="flex items-center justify-center gap-2">
                                            <button
                                                type="button"
                                                onClick={() => handleViewDetails(vendor)}
                                                className="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded transition-all"
                                                title="View vendor details"
                                            >
                                                <Eye className="w-3 h-3" />
                                                View
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => handleCompare(vendor)}
                                                className="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium text-green-700 bg-green-50 hover:bg-green-100 border border-green-200 rounded transition-all"
                                                title="Compare with new vendor"
                                            >
                                                <GitCompare className="w-3 h-3" />
                                                Compare
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            {duplicates.length === 0 && (
                                <tr>
                                    <td colSpan={9} className="px-4 py-8 text-center text-gray-500 italic">
                                        No matches found.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </AccessibleModal>

            {/* Comparison Modal */}
            {showCompareModal && selectedVendor && newVendorData && (
                <VendorComparisonModal
                    isOpen={showCompareModal}
                    onClose={closeCompareModal}
                    newVendor={newVendorData}
                    existingVendor={selectedVendor}
                />
            )}

            {/* Detail Modal */}
            {showDetailsModal && selectedVendor && (
                <VendorDetailModal
                    isOpen={showDetailsModal}
                    onClose={closeDetailsModal}
                    vendor={selectedVendor}
                />
            )}
        </>
    );
};
