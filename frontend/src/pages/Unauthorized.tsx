import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { ShieldAlert, Home, LogIn, ChevronDown, ChevronUp } from 'lucide-react';
import { Button, Card } from '../components/ui/Elements';
import { useAuth } from '../context/AuthContext';

export const Unauthorized: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { logout, user } = useAuth();
    const [showDetails, setShowDetails] = useState(false);

    const errorMessage = location.state?.message || "You do not have the required permissions to access this specific resource.";
    const errorDetails = location.state?.details;
    const correlationId = location.state?.correlationId || new Date().getTime().toString(36);

    // Determine where "Back" should go based on role
    const handleBack = () => {
        if (!user) {
            navigate('/login');
            return;
        }

        if (user.role === 'Admin') {
            navigate('/admin/dashboard');
        } else if (['Requestor', 'VendorUnit', 'BFM', 'Approver'].includes(user.role)) {
            navigate('/approver/worklist');
        } else {
            navigate('/dashboard');
        }
    };

    const handleLoginDifferentUser = () => {
        logout();
        navigate('/login');
    };

    return (
        <div className="min-h-[80vh] flex items-center justify-center p-4 bg-gray-50">
            <div className="max-w-md w-full">
                <Card className="border-t-4 border-t-red-500 shadow-lg">
                    <div className="flex flex-col items-center text-center p-6">
                        <div className="h-20 w-20 bg-red-100 rounded-full flex items-center justify-center mb-6">
                            <ShieldAlert className="h-10 w-10 text-red-600" />
                        </div>

                        <h1 className="text-2xl font-bold text-gray-900 mb-2">{location.state?.title || "Access Denied"}</h1>
                        <h2 className="text-4xl font-black text-gray-200 mb-6 tracking-widest">{location.state?.status || "401"}</h2>

                        <div className="bg-red-50 border border-red-100 rounded-lg p-4 mb-6 w-full text-left">
                            <p className="text-sm text-red-800 font-medium mb-2">
                                {errorMessage}
                            </p>
                            {user && (
                                <p className="text-xs text-red-600">
                                    Current Role: <span className="font-bold uppercase">{user.role}</span>
                                </p>
                            )}
                        </div>

                        {errorDetails && (
                            <div className="w-full mb-6">
                                <button
                                    onClick={() => setShowDetails(!showDetails)}
                                    className="flex items-center text-xs text-gray-500 hover:text-gray-700 mx-auto mb-2 focus:outline-none"
                                >
                                    {showDetails ? <ChevronUp className="h-3 w-3 mr-1" /> : <ChevronDown className="h-3 w-3 mr-1" />}
                                    {showDetails ? "Hide Technical Details" : "Show Technical Details"}
                                </button>

                                {showDetails && (
                                    <div className="bg-gray-100 p-3 rounded text-xs font-mono text-gray-600 text-left overflow-x-auto border border-gray-200">
                                        {typeof errorDetails === 'object' ? JSON.stringify(errorDetails, null, 2) : errorDetails}
                                    </div>
                                )}
                            </div>
                        )}

                        <div className="flex flex-col gap-3 w-full">
                            <Button
                                onClick={handleBack}
                                variant="primary"
                                className="w-full justify-center flex items-center gap-2"
                            >
                                <Home className="h-4 w-4" /> Go to Dashboard
                            </Button>

                            <Button
                                onClick={handleLoginDifferentUser}
                                variant="secondary"
                                className="w-full justify-center flex items-center gap-2"
                            >
                                <LogIn className="h-4 w-4" /> Login as Different User
                            </Button>
                        </div>
                    </div>
                </Card>
                <div className="text-center mt-6">
                    <p className="text-xs text-gray-400">
                        If you believe this is an error, please contact IT Support.<br />
                        Correlation ID: {correlationId}
                    </p>
                </div>
            </div>
        </div>
    );
};
