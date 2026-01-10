import React from 'react';
import { useAuth } from '../context/AuthContext';
import { XCircle } from 'lucide-react';

export const ImpersonationBanner: React.FC = () => {
    const { user, stopImpersonation } = useAuth();

    if (!user?.isImpersonated) return null;

    return (
        <div className="bg-amber-500 text-white px-4 py-2 flex items-center justify-between shadow-md z-50 relative">
            <div className="flex items-center gap-2">
                <span className="font-bold uppercase text-xs tracking-wider border border-white/50 px-2 py-0.5 rounded">Impersonation Mode</span>
                <span className="text-sm">Acting as: <strong>{user.name}</strong> ({user.role})</span>
            </div>
            <button
                onClick={stopImpersonation}
                className="flex items-center gap-1 bg-white/20 hover:bg-white/30 px-3 py-1 rounded transition-colors text-xs font-semibold"
            >
                <XCircle className="h-4 w-4" />
                Stop Impersonating
            </button>
        </div>
    );
};
