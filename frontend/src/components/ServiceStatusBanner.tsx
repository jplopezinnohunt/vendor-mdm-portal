import React, { useEffect, useState } from 'react';
import { statusService } from '../services/statusService';

/**
 * Displays a banner indicating whether the backend API is reachable (online) or using mock data.
 * This component is placed in the main layout so it appears on every screen.
 */
export const ServiceStatusBanner: React.FC = () => {
    const [status, setStatus] = useState<'online' | 'mock'>('mock');
    const [checking, setChecking] = useState(true);

    useEffect(() => {
        const check = async () => {
            const result = await statusService.checkBackendStatus();
            setStatus(result);
            setChecking(false);
        };
        check();
    }, []);

    if (checking) {
        return null; // don't flash banner while checking
    }

    const bgClass = status === 'online' ? 'bg-green-600' : 'bg-yellow-600';
    const text = status === 'online' ? 'Backend Connected' : 'Using Mock Data';

    return (
        <div className={`w-full text-center text-white py-1 text-sm ${bgClass}`}>
            {text}
        </div>
    );
};
