import React from 'react';
import { FileText, CheckCircle, Clock, AlertCircle } from 'lucide-react';

export type WorkflowStatus = 'draft' | 'in-progress' | 'validation' | 'completed';

interface VendorHeaderPanelProps {
    requestId: string | null;
    vendorName: string;
    workflow: WorkflowStatus;
    lastModification: string | null;
    sapNumber: string | null;
    lastSavedBy: string;
    companyCode: string;
    dutyStation?: string;
    isDraft?: boolean;
}

export const VendorHeaderPanel: React.FC<VendorHeaderPanelProps> = ({
    requestId,
    vendorName,
    workflow,
    lastModification,
    sapNumber,
    lastSavedBy,
    companyCode,
    dutyStation,
    isDraft = true,
}) => {
    const getWorkflowIcons = () => {
        const icons = [];

        if (workflow === 'draft') {
            icons.push(<FileText key="draft" className="w-4 h-4 text-gray-400" title="Draft" />);
        } else if (workflow === 'in-progress') {
            icons.push(<Clock key="progress" className="w-4 h-4 text-blue-500" title="In Progress" />);
        } else if (workflow === 'validation') {
            icons.push(<AlertCircle key="validation" className="w-4 h-4 text-yellow-500" title="Validation" />);
        } else if (workflow === 'completed') {
            icons.push(<CheckCircle key="completed" className="w-4 h-4 text-green-500" title="Completed" />);
        }

        return icons;
    };

    return (
        <div className="bg-white border border-gray-300 shadow-sm mb-6">
            {/* Header Bar */}
            <div className="bg-[#4a7ec5] text-white px-4 py-2">
                <h2 className="text-sm font-bold">
                    {isDraft ? 'Physical person: ' : 'Vendor Master: '}
                    {vendorName || 'New Vendor'}
                </h2>
            </div>

            {/* Information Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-x-6 gap-y-3 p-4 bg-gray-50">
                {/* Request Id */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Request Id:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {requestId || <span className="text-red-600">Not yet saved</span>}
                    </span>
                </div>

                {/* Vendor Name */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Vendor Name:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {vendorName || <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>

                {/* Workflow */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Workflow:
                    </label>
                    <div className="flex items-center gap-2">
                        {getWorkflowIcons()}
                    </div>
                </div>

                {/* Last modification */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Last modification:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {lastModification || <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>

                {/* SAP Number */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        SAP Number:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {sapNumber || <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>

                {/* Last saved by */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Last saved by:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {requestId ? lastSavedBy : <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>

                {/* Company Code */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Company Code:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {companyCode || <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>

                {/* Duty Station / Sector */}
                <div className="flex flex-col">
                    <label className="text-[10px] font-bold text-gray-600 uppercase tracking-wider mb-1">
                        Duty Station / Sector:
                    </label>
                    <span className="text-sm font-bold text-gray-900">
                        {dutyStation || <span className="text-gray-400 italic">-</span>}
                    </span>
                </div>
            </div>
        </div>
    );
};
