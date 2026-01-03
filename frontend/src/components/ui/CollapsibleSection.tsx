import React, { useState } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';

interface CollapsibleSectionProps {
    title: string;
    defaultExpanded?: boolean;
    children: React.ReactNode;
    className?: string;
}

/**
 * Collapsible Section Component
 * Provides expandable/collapsible UI section with smooth animations
 * Matches SAP design from reference screenshots
 */
export const CollapsibleSection: React.FC<CollapsibleSectionProps> = ({
    title,
    defaultExpanded = true,
    children,
    className = ''
}) => {
    const [isExpanded, setIsExpanded] = useState(defaultExpanded);

    return (
        <div className={`border border-[#4a7ec5] rounded overflow-hidden ${className}`}>
            {/* Header - Blue bar matching SAP design */}
            <button
                type="button"
                onClick={() => setIsExpanded(!isExpanded)}
                className="w-full bg-[#4a7ec5] text-white px-4 py-2 font-bold text-sm flex items-center justify-between hover:bg-[#3a6eb5] transition-colors"
            >
                <span>{title}</span>
                {isExpanded ? (
                    <ChevronUp className="w-4 h-4" />
                ) : (
                    <ChevronDown className="w-4 h-4" />
                )}
            </button>

            {/* Content - Smooth animation */}
            <div
                className={`transition-all duration-300 ease-in-out overflow-hidden ${isExpanded ? 'max-h-[2000px] opacity-100' : 'max-h-0 opacity-0'
                    }`}
            >
                <div className="p-6 bg-white">
                    {children}
                </div>
            </div>
        </div>
    );
};
