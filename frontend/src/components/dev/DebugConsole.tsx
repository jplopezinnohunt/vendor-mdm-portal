import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { X, ChevronUp, ChevronDown, Trash2 } from 'lucide-react';

interface LogEntry {
    id: string;
    type: 'error' | 'warn' | 'info';
    message: string;
    stack?: string;
    timestamp: string;
}

export const DebugConsole: React.FC = () => {
    const [logs, setLogs] = useState<LogEntry[]>([]);
    const [isOpen, setIsOpen] = useState(false);
    const [hasNewError, setHasNewError] = useState(false);

    // Use a ref to store logs to avoid dependency cycles in event listeners
    const logsRef = React.useRef<LogEntry[]>([]);

    const addLog = (type: 'error' | 'warn' | 'info', message: string, stack?: string) => {
        const newLog: LogEntry = {
            id: Math.random().toString(36).substring(7),
            type,
            message,
            stack,
            timestamp: new Date().toLocaleTimeString(),
        };

        logsRef.current = [newLog, ...logsRef.current].slice(0, 50); // Keep last 50
        setLogs([...logsRef.current]);

        if (type === 'error') {
            setHasNewError(true);
            setIsOpen(true); // Auto-open on error
        }
    };

    useEffect(() => {
        // 1. Capture console.error
        const originalConsoleError = console.error;
        console.error = (...args) => {
            // Call original
            originalConsoleError.apply(console, args);

            // Format message
            const message = args.map(arg => {
                if (typeof arg === 'object') {
                    try {
                        return JSON.stringify(arg);
                    } catch {
                        return String(arg);
                    }
                }
                return String(arg);
            }).join(' ');

            addLog('error', message);
        };

        // 2. Capture window errors (exceptions)
        const handleWindowError = (event: ErrorEvent) => {
            addLog('error', event.message, event.error?.stack);
        };

        // 3. Capture unhandled promise rejections
        const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
            let message = 'Unhandled Promise Rejection';
            if (typeof event.reason === 'string') message = event.reason;
            else if (event.reason instanceof Error) message = event.reason.message;

            addLog('error', message, event.reason?.stack);
        };

        window.addEventListener('error', handleWindowError);
        window.addEventListener('unhandledrejection', handleUnhandledRejection);

        return () => {
            console.error = originalConsoleError;
            window.removeEventListener('error', handleWindowError);
            window.removeEventListener('unhandledrejection', handleUnhandledRejection);
        };
    }, []);

    // Show if:
    // 1. DEV environment
    // 2. URL has ?debug=true
    // 3. LocalStorage has vendor_debug=true
    const shouldShow = import.meta.env.DEV ||
        window.location.search.includes('debug=true') ||
        localStorage.getItem('vendor_debug') === 'true';

    if (!shouldShow) return null;

    return (
        <div className={`fixed bottom-4 right-4 z-[9999] flex flex-col items-end pointer-events-none`}>
            {/* Toggle Button */}
            <div className="pointer-events-auto shadow-lg">
                <Button
                    variant={hasNewError ? "destructive" : "secondary"}
                    onClick={() => {
                        setIsOpen(!isOpen);
                        if (!isOpen) setHasNewError(false);
                    }}
                    className="rounded-t-md rounded-b-none px-4 py-2 text-xs font-mono font-bold border border-gray-300 dark:border-gray-700"
                >
                    {isOpen ? <ChevronDown className="h-4 w-4 mr-2" /> : <ChevronUp className="h-4 w-4 mr-2" />}
                    Debug Console {logs.length > 0 && `(${logs.length})`}
                </Button>
            </div>

            {/* Console Panel */}
            {isOpen && (
                <div className="pointer-events-auto w-[600px] h-[400px] bg-gray-900 text-gray-100 rounded-tl-md rounded-bl-md shadow-2xl border border-gray-700 flex flex-col font-mono text-xs overflow-hidden">

                    {/* Header */}
                    <div className="flex items-center justify-between px-3 py-2 bg-gray-800 border-b border-gray-700">
                        <span className="font-bold text-gray-300">Console Output</span>
                        <Button variant="ghost" size="sm" onClick={() => { logsRef.current = []; setLogs([]); }} className="h-6 w-6 p-0 hover:bg-gray-700">
                            <Trash2 className="h-3 w-3 text-gray-400" />
                        </Button>
                    </div>

                    {/* Logs */}
                    <div className="flex-1 overflow-y-auto p-2 space-y-2 bg-black/90">
                        {logs.length === 0 && (
                            <div className="text-gray-500 italic text-center mt-10">No logs captured...</div>
                        )}
                        {logs.map(log => (
                            <div key={log.id} className={`p-2 rounded border-l-2 ${log.type === 'error' ? 'bg-red-900/30 border-red-500 text-red-200' : 'bg-gray-800 border-gray-500'
                                }`}>
                                <div className="flex justify-between text-[10px] text-gray-500 mb-1">
                                    <span>{log.timestamp}</span>
                                    <span className="uppercase font-bold">{log.type}</span>
                                </div>
                                <div className="whitespace-pre-wrap break-words">{log.message}</div>
                                {log.stack && (
                                    <details className="mt-1">
                                        <summary className="cursor-pointer text-gray-500 hover:text-gray-300">[Stack Trace]</summary>
                                        <pre className="mt-2 text-[10px] text-gray-400 overflow-x-auto whitespace-pre">{log.stack}</pre>
                                    </details>
                                )}
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
};
