import { useEffect, useState } from 'react';
import { AlertCircle, Mail, MailCheck, Server } from 'lucide-react';

interface EmailServiceStatus {
    mode: string;
    status: string;
    message: string;
    environment: string;
    configured: boolean;
}

export default function EmailServiceBanner() {
    const [emailStatus, setEmailStatus] = useState<EmailServiceStatus | null>(null);
    const [loading, setLoading] = useState(true);
    const [dismissed, setDismissed] = useState(false);

    useEffect(() => {
        // Check if banner was previously dismissed in this session
        const wasDismissed = sessionStorage.getItem('emailBannerDismissed') === 'true';
        if (wasDismissed) {
            setDismissed(true);
            setLoading(false);
            return;
        }

        // Fetch from backend API (localhost:5001)
        fetch('http://localhost:5001/api/health/email-service')
            .then((res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then((data) => {
                console.log('Email service status:', data);
                setEmailStatus(data.emailService);
                setLoading(false);
            })
            .catch((err) => {
                console.error('Failed to fetch email service status:', err);
                // Show banner anyway in fallback mode if API is unreachable
                setEmailStatus({
                    mode: 'console-logging',
                    status: 'fallback',
                    message: 'Email service status unavailable',
                    environment: 'local',
                    configured: false
                });
                setLoading(false);
            });
    }, []);

    const handleDismiss = () => {
        setDismissed(true);
        sessionStorage.setItem('emailBannerDismissed', 'true');
    };

    if (loading || dismissed || !emailStatus) {
        return null;
    }

    // Only show banner if email is not fully configured or in console logging mode
    const shouldShow =
        emailStatus.status === 'fallback' ||
        emailStatus.status === 'not-configured' ||
        (emailStatus.environment === 'local' && emailStatus.mode === 'console-logging');

    if (!shouldShow) {
        return null;
    }

    const getIcon = () => {
        if (emailStatus.status === 'not-configured') {
            return <AlertCircle className="w-5 h-5" />;
        }
        if (emailStatus.mode === 'console-logging') {
            return <Server className="w-5 h-5" />;
        }
        return <Mail className="w-5 h-5" />;
    };

    const getBannerStyle = () => {
        if (emailStatus.status === 'not-configured') {
            return 'bg-red-50 border-red-200 text-red-800';
        }
        if (emailStatus.status === 'fallback') {
            return 'bg-yellow-50 border-yellow-200 text-yellow-800';
        }
        return 'bg-blue-50 border-blue-200 text-blue-800';
    };

    return (
        <div className={`border-l-4 p-4 mb-6 rounded-r-md ${getBannerStyle()}`}>
            <div className="flex items-start justify-between">
                <div className="flex items-start gap-3">
                    <div className="mt-0.5">
                        {getIcon()}
                    </div>
                    <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                            <h3 className="font-semibold text-sm">
                                {emailStatus.status === 'not-configured' && 'Email Service Not Configured'}
                                {emailStatus.status === 'fallback' && 'Email Service Running in Development Mode'}
                                {emailStatus.status === 'configured' && 'Email Service Status'}
                            </h3>
                            <span className="text-xs px-2 py-0.5 rounded-full bg-white bg-opacity-50">
                                {emailStatus.environment === 'local' ? 'Local' : 'Azure'}
                            </span>
                        </div>
                        <p className="text-sm">
                            {emailStatus.message}
                        </p>
                        {emailStatus.mode === 'console-logging' && (
                            <p className="text-xs mt-2 opacity-80">
                                💡 Invitation emails are being logged to the backend console.
                                <button
                                    onClick={() => window.open('/docs/troubleshooting/email-service-guide.md', '_blank')}
                                    className="underline ml-1 hover:opacity-70"
                                >
                                    Configure SMTP
                                </button> to send real emails.
                            </p>
                        )}
                        {emailStatus.status === 'not-configured' && (
                            <p className="text-xs mt-2 opacity-80">
                                ⚠️ Email notifications will not be sent. Please configure email service in Azure Portal or appsettings.
                            </p>
                        )}
                    </div>
                </div>
                <button
                    onClick={handleDismiss}
                    className="text-current opacity-50 hover:opacity-100 ml-4"
                    aria-label="Dismiss"
                >
                    ✕
                </button>
            </div>
        </div>
    );
}
