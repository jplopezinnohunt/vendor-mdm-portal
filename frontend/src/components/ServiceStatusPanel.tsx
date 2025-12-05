import React, { useEffect, useState } from 'react';
import { statusService } from '../services/statusService';

/**
 * Checks a specific endpoint and resolves to 'online' if the request succeeds,
 * otherwise returns 'mock'.
 */
const checkEndpoint = async (url: string): Promise<'online' | 'mock'> => {
    try {
        await statusService.checkBackendStatus(); // simple health check first
        // try the specific endpoint – we only need a GET, ignore body
        await fetch(url, { method: 'GET' });
        return 'online';
    } catch {
        return 'mock';
    }
};

export const ServiceStatusPanel: React.FC = () => {
    const [vendorStatus, setVendorStatus] = useState<'online' | 'mock'>('mock');
    const [requestsStatus, setRequestsStatus] = useState<'online' | 'mock'>('mock');
    const [onboardingStatus, setOnboardingStatus] = useState<'online' | 'mock'>('mock');

    useEffect(() => {
        // Run checks sequentially – can be parallel if desired
        const runChecks = async () => {
            const v = await checkEndpoint('http://localhost:7001/api/vendor/100450');
            setVendorStatus(v);
            const r = await checkEndpoint('http://localhost:7001/api/changerequest/vendor/100450');
            setRequestsStatus(r);
            const o = await checkEndpoint('http://localhost:7001/api/onboarding');
            setOnboardingStatus(o);
        };
        runChecks();
    }, []);

    const badge = (status: 'online' | 'mock') => (
        <span className={`ml-2 px-2 py-0.5 text-xs rounded ${status === 'online' ? 'bg-green-600' : 'bg-yellow-600'} text-white`}>
            {status === 'online' ? 'Online' : 'Mock'}
        </span>
    );

    return (
        <div className="flex flex-col space-y-1 text-sm text-gray-600 bg-white/80 backdrop-blur-sm p-2 rounded border border-gray-200 mt-2">
            <div>Vendor Service:{badge(vendorStatus)}</div>
            <div>Change‑Request Service:{badge(requestsStatus)}</div>
            <div>Onboarding Service:{badge(onboardingStatus)}</div>
        </div>
    );
};
