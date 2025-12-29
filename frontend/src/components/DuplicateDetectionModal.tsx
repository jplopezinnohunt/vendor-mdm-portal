import React from 'react';
import { X, AlertTriangle, ExternalLink, User, Globe, Hash, Info } from 'lucide-react';
import { Button } from './Elements';

interface DuplicateVendor {
    vendorName: string;
    dateOfBirth?: string;
    sapId: string;
    reqId?: string;
    country: string;
    companyCode: string;
    accountGroup: string;
}

interface DuplicateDetectionModalProps {
    isOpen: boolean;
    onClose: () => void;
    onProceed: () => void;
    duplicates: DuplicateVendor[];
}

export const DuplicateDetectionModal: React.FC<DuplicateDetectionModalProps> = ({
    isOpen,
    onClose,
    onProceed,
    duplicates
}) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-in fade-in duration-300">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-5xl overflow-hidden animate-in zoom-in-95 duration-300">
                {/* Header - UNESCO QAS Style */}
                <div className="bg-[#1e40af] text-white px-6 py-4 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <h2 className="text-xl font-semibold">Vendor(s) which already exist in SAP</h2>
                    </div>
                    <button
                        onClick={onClose}
                        className="p-1 hover:bg-white/20 rounded-full transition-colors"
                    >
                        <X className="w-6 h-6" />
                    </button>
                </div>

                <div className="p-8">
                    {/* Informational Message */}
                    <div className="mb-6 flex items-start gap-4 text-gray-700">
                        <p className="text-[15px]">
                            Vendors with some similar information. Please ensure that your vendor is not already on the list.
                        </p>
                    </div>

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
                                    <th className="px-4 py-3 text-sm font-semibold">Account Group</th>
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
                                        <td className="px-4 py-4 text-sm text-gray-600">{vendor.accountGroup}</td>
                                    </tr>
                                ))}
                                {duplicates.length === 0 && (
                                    <tr>
                                        <td colSpan={7} className="px-4 py-8 text-center text-gray-500 italic">
                                            No matches found.
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>

                    {/* Actions */}
                    <div className="mt-8 flex items-center justify-between pt-6 border-t border-gray-100">
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
                </div>
            </div>
        </div>
    );
};
