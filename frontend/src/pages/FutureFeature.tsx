import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Construction, ArrowLeft, Rocket } from 'lucide-react';
import { Button, Card } from '../components/ui/Elements';

export const FutureFeature: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();

    // Allow passing a specific feature name via state
    const featureName = location.state?.featureName || "This Feature";
    const description = location.state?.description || "We are currently building this functionality. Check back soon for updates!";

    return (
        <div className="min-h-[80vh] flex items-center justify-center p-4 bg-gray-50">
            <div className="max-w-md w-full">
                <Card className="border-t-4 border-t-blue-500 shadow-lg">
                    <div className="flex flex-col items-center text-center p-8">
                        <div className="h-24 w-24 bg-blue-50 rounded-full flex items-center justify-center mb-6 relative overflow-hidden">
                            <Construction className="h-10 w-10 text-blue-600 relative z-10" />
                            <div className="absolute inset-0 bg-blue-100 opacity-30 animate-pulse"></div>
                        </div>

                        <div className="bg-blue-600 text-white text-xs font-bold px-3 py-1 rounded-full mb-4 tracking-wide uppercase">
                            Rx Roadmap
                        </div>

                        <h1 className="text-2xl font-bold text-gray-900 mb-3">Coming Soon</h1>

                        <p className="text-gray-600 mb-2 font-medium">
                            {featureName} is on our roadmap.
                        </p>

                        <p className="text-sm text-gray-500 mb-8 max-w-xs mx-auto leading-relaxed">
                            {description}
                        </p>

                        <div className="w-full bg-gray-100 rounded-lg p-4 mb-8">
                            <div className="flex items-center gap-3 text-left">
                                <div className="p-2 bg-white rounded shadow-sm">
                                    <Rocket className="h-5 w-5 text-indigo-500" />
                                </div>
                                <div>
                                    <h3 className="text-xs font-bold text-gray-900">Planned Release</h3>
                                    <p className="text-xs text-gray-500">Q2 2026 - Sprint 4</p>
                                </div>
                            </div>
                        </div>

                        <Button
                            onClick={() => navigate(-1)}
                            variant="primary"
                            className="w-full justify-center flex items-center gap-2"
                        >
                            <ArrowLeft className="h-4 w-4" /> Go Back
                        </Button>
                    </div>
                </Card>
            </div>
        </div>
    );
};
