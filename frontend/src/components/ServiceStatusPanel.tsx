import React, { useEffect, useState } from 'react';
import { statusService } from '../services/statusService';
import { API_BASE_URL } from '../services/api';

type SystemStatus = 'Local' | 'AZURE MOCK' | 'AZURE Final' | 'Mock (Client)';

/**
 * Checks a specific endpoint and determines status based on environment and connectivity.
 */
const checkEndpoint = async (url: string): Promise<SystemStatus> => {
    try {
        await statusService.checkBackendStatus(); // simple health check first
        // try the specific endpoint – we only need a GET, ignore body
        await fetch(url, { method: 'GET' });

        // Connectivity Success: Determine specific environment
        if (API_BASE_URL.includes('localhost') || API_BASE_URL.includes('127.0.0.1')) {
            // Allow overriding Local status if we are explicitly testing Azure Mock locally
            if (import.meta.env.VITE_SERVICE_LEVEL === 'MOCK') return 'AZURE MOCK';
            if (import.meta.env.VITE_SERVICE_LEVEL === 'FINAL') return 'AZURE Final';
            return 'Local';
        } else {
            if (import.meta.env.VITE_SERVICE_LEVEL === 'FINAL') return 'AZURE Final';
            // Default to AZURE MOCK for remote connections
            return 'AZURE MOCK';
        }
    } catch {
        // Connectivity Failed: Fallback to Client Side Mock
        return 'Mock (Client)';
    }
};

export const ServiceStatusPanel: React.FC = () => {
    const [vendorStatus, setVendorStatus] = useState<SystemStatus>('Mock (Client)');
    const [requestsStatus, setRequestsStatus] = useState<SystemStatus>('Mock (Client)');
    const [onboardingStatus, setOnboardingStatus] = useState<SystemStatus>('Mock (Client)');

    const baseUrl = API_BASE_URL.startsWith('http') ? API_BASE_URL : window.location.origin + API_BASE_URL;

    useEffect(() => {
        // Run checks sequentially – can be parallel if desired
        const runChecks = async () => {
            const v = await checkEndpoint(`${baseUrl}/changerequest/100450`);
            setVendorStatus(v);
            const r = await checkEndpoint(`${baseUrl}/changerequest/id/cr-001`); // Using a valid pattern
            setRequestsStatus(r);
            const o = await checkEndpoint(`${baseUrl}/review/pending`);
            setOnboardingStatus(o);
        };
        runChecks();
    }, [baseUrl]);

    const renderStatusRow = (label: string, status: SystemStatus) => {
        let colorClass = 'bg-gray-500';

        if (status === 'Local') {
            colorClass = 'bg-blue-600';
        } else if (status === 'AZURE Final') {
            colorClass = 'bg-green-700';
        } else if (status === 'AZURE MOCK') {
            colorClass = 'bg-purple-600';
        } else if (status === 'Mock (Client)') {
            colorClass = 'bg-amber-500';
        }

        return (
            <div className="flex items-center justify-start py-1">
                <span className="font-medium w-48">{label}:</span>
                <span className={`px-2 py-0.5 text-xs rounded ${colorClass} text-white whitespace-nowrap w-24 text-center`}>
                    {status}
                </span>
            </div>
        );
    };

    return (
        <div className="flex flex-col space-y-1 text-sm text-gray-600 bg-white/80 backdrop-blur-sm p-3 rounded border border-gray-200 mt-2">
            <div className="mb-2">
                {renderStatusRow('Vendor Service', vendorStatus)}
                {renderStatusRow('Change‑Request Service', requestsStatus)}
                {renderStatusRow('Onboarding Service', onboardingStatus)}
            </div>

            <div className="border-t border-gray-200 pt-2 mt-2">
                <p className="text-xs font-semibold text-gray-500 mb-1">Status Legend:</p>
                <ul className="text-xs text-gray-500 space-y-1 ml-1">
                    <li className="flex items-center">
                        <span className="w-24 px-2 py-0.5 rounded bg-blue-600 text-white text-center mr-2">Local</span>
                        Dev using local artifacts (localhost).
                    </li>
                    <li className="flex items-center">
                        <span className="w-24 px-2 py-0.5 rounded bg-green-700 text-white text-center mr-2">AZURE Final</span>
                        Azure Service (Real SAP Connected).
                    </li>
                    <li className="flex items-center">
                        <span className="w-24 px-2 py-0.5 rounded bg-purple-600 text-white text-center mr-2">AZURE MOCK</span>
                        Azure Service (SAP Simulation).
                    </li>
                    <li className="flex items-center">
                        <span className="w-24 px-2 py-0.5 rounded bg-amber-500 text-white text-center mr-2">Mock (Client)</span>
                        Service unreachable, client fallback.
                    </li>
                </ul>
            </div>
        </div>
    );
};
