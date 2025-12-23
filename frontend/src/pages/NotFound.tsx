import React from 'react';
import { Link } from 'react-router-dom';
import { AlertTriangle, Home } from 'lucide-react';

export const NotFound: React.FC = () => {
    return (
        <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-md">
                <div className="flex justify-center">
                    <AlertTriangle className="h-16 w-16 text-yellow-500" />
                </div>
                <h2 className="mt-6 text-center text-3xl font-bold tracking-tight text-gray-900">
                    Page Not Found
                </h2>
                <p className="mt-2 text-center text-sm text-gray-600">
                    The page you are looking for does not exist or has been moved.
                </p>
            </div>

            <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
                <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10 text-center">
                    <p className="mb-6 text-gray-700">
                        If you followed a link, it may be broken or expired.
                    </p>
                    <Link to="/">
                        <button className="inline-flex items-center rounded-md border border-transparent bg-brand-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-brand-700 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2">
                            <Home className="mr-2 h-4 w-4" />
                            Go to Home
                        </button>
                    </Link>
                </div>
            </div>
        </div>
    );
};
