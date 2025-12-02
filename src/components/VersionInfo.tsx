import React from 'react';
import { version } from '../version';

interface VersionInfoProps {
    className?: string;
}

export const VersionInfo: React.FC<VersionInfoProps> = ({ className = '' }) => {
    // Format the date for display
    const formatDate = (dateString: string): string => {
        try {
            const date = new Date(dateString);
            return date.toLocaleString('en-US', {
                year: 'numeric',
                month: 'short',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
            });
        } catch {
            return dateString;
        }
    };

    return (
        <div className={`text-xs text-gray-400 ${className}`}>
            <span className="font-semibold">{version.version}</span>
            {' • '}
            <span>Built on {formatDate(version.buildDate)}</span>
            {version.commitHash !== 'unknown' && (
                <>
                    {' • '}
                    <span className="font-mono">{version.commitHash}</span>
                </>
            )}
        </div>
    );
};
