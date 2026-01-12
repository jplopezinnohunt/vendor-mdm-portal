import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Card, Button } from '../components/ui/Elements';
import { AlertOctagon, Home, RefreshCw, ChevronDown, ChevronUp } from 'lucide-react';

export const ServerError: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const [showDetails, setShowDetails] = React.useState(false);

    // Get error details passed from the redirect
    const state = location.state as {
        message?: string;
        detail?: string;
        stack?: string;
        status?: number;
        originalError?: any;
    } | null;

    const errorMessage = state?.detail || state?.message || 'An unexpected error occurred on the server.';
    const errorStatus = state?.status || 500;
    const stackTrace = state?.stack;

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-xl">
                <div className="flex justify-center mb-6">
                    <div className="rounded-full bg-red-100 p-4">
                        <AlertOctagon className="h-10 w-10 text-red-600" />
                    </div>
                </div>
                <h2 className="mt-2 text-center text-3xl font-bold tracking-tight text-gray-900">
                    {errorStatus} - Server Error
                </h2>
                <p className="mt-2 text-center text-sm text-gray-600">
                    Something went wrong on our end. We track these errors automatically, but your feedback helps.
                </p>
            </div>

            <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-xl">
                <Card>
                    <div className="space-y-6">
                        <div className="bg-red-50 border-l-4 border-red-500 p-4">
                            <div className="flex">
                                <div className="ml-3">
                                    <h3 className="text-sm font-bold text-red-800 uppercase tracking-wide">Error Details</h3>
                                    <p className="text-sm text-red-700 mt-1 whitespace-pre-wrap font-medium">
                                        {errorMessage}
                                    </p>
                                </div>
                            </div>
                        </div>

                        {stackTrace && (
                            <div className="border rounded-md overflow-hidden">
                                <button
                                    onClick={() => setShowDetails(!showDetails)}
                                    className="w-full flex justify-between items-center px-4 py-2 bg-gray-50 hover:bg-gray-100 text-xs font-bold text-gray-500 uppercase tracking-wider"
                                >
                                    <span>Technical Debug Info</span>
                                    {showDetails ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
                                </button>

                                {showDetails && (
                                    <div className="p-4 bg-gray-900 overflow-x-auto">
                                        <pre className="text-xs text-green-400 font-mono whitespace-pre-wrap break-all">
                                            {stackTrace}
                                        </pre>
                                        {state?.originalError && (
                                            <pre className="mt-4 pt-4 border-t border-gray-700 text-xs text-gray-400 font-mono whitespace-pre-wrap">
                                                Full Payload: {JSON.stringify(state.originalError, null, 2)}
                                            </pre>
                                        )}
                                    </div>
                                )}
                            </div>
                        )}

                        <div className="flex gap-3 justify-center pt-4">
                            <Button
                                variant="secondary"
                                onClick={() => navigate(-1)}
                                className="flex items-center gap-2"
                            >
                                <RefreshCw className="h-4 w-4" />
                                Try Again
                            </Button>
                            <Button
                                variant="primary"
                                onClick={() => navigate('/')}
                                className="flex items-center gap-2"
                            >
                                <Home className="h-4 w-4" />
                                Return Home
                            </Button>
                        </div>
                    </div>
                </Card>
            </div>
        </div>
    );
};
