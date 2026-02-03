import React, { useState, useEffect } from 'react';
import { formatDistanceToNow } from 'date-fns';
import './AuditLogViewer.css';

interface AuditLog {
    id: string;
    entityType: string;
    entityId: string;
    action: string;
    changedBy: string;
    changedByUserId: string;
    changedAt: string;
    oldValues: string | null;
    newValues: string | null;
    reason: string | null;
    ipAddress: string | null;
    userAgent: string | null;
    tenantId: string | null;
}

interface AuditLogViewerProps {
    entityType: string;
    entityId: string;
}

export const AuditLogViewer: React.FC<AuditLogViewerProps> = ({ entityType, entityId }) => {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [expandedLog, setExpandedLog] = useState<string | null>(null);

    useEffect(() => {
        loadAuditLogs();
    }, [entityType, entityId]);

    const loadAuditLogs = async () => {
        setLoading(true);
        setError(null);

        try {
            // ✅ NEW RULE: 10-second timeout
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 10000);

            const response = await fetch(
                `/api/auditlog/${encodeURIComponent(entityType)}/${entityId}`,
                {
                    signal: controller.signal,
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                    }
                }
            );

            clearTimeout(timeoutId);

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Failed to load audit logs' }));
                throw new Error(errorData.message || `HTTP ${response.status}`);
            }

            const data = await response.json();
            setLogs(data);
        } catch (err: any) {
            // ✅ NEW RULE: Proper error handling
            if (err.name === 'AbortError') {
                setError('Request timed out. Please try again.');
            } else {
                setError(err.message || 'Failed to load audit logs');
            }
            console.error('Error loading audit logs:', err);
        } finally {
            setLoading(false);
        }
    };

    const toggleExpand = (logId: string) => {
        setExpandedLog(expandedLog === logId ? null : logId);
    };

    const parseJsonSafe = (json: string | null): any => {
        if (!json) return null;
        try {
            return JSON.parse(json);
        } catch {
            return null;
        }
    };

    const getActionIcon = (action: string): string => {
        switch (action.toLowerCase()) {
            case 'created': return '➕';
            case 'updated': return '✏️';
            case 'deleted': return '🗑️';
            case 'approved': return '✅';
            case 'rejected': return '❌';
            case 'completed': return '🎉';
            case 'resent': return '📧';
            case 'cancelled': return '🚫';
            default: return '📝';
        }
    };

    const getActionColor = (action: string): string => {
        switch (action.toLowerCase()) {
            case 'created': return 'var(--color-success)';
            case 'updated': return 'var(--color-info)';
            case 'deleted': return 'var(--color-danger)';
            case 'approved': return 'var(--color-success)';
            case 'rejected': return 'var(--color-danger)';
            case 'completed': return 'var(--color-success)';
            case 'resent': return 'var(--color-warning)';
            case 'cancelled': return 'var(--color-danger)';
            default: return 'var(--color-text-secondary)';
        }
    };

    // ✅ NEW RULE: Loading state
    if (loading) {
        return (
            <div className="audit-log-viewer">
                <div className="audit-log-loading">
                    <div className="spinner"></div>
                    <p>Loading audit history...</p>
                </div>
            </div>
        );
    }

    // ✅ NEW RULE: Error state
    if (error) {
        return (
            <div className="audit-log-viewer">
                <div className="audit-log-error">
                    <span className="error-icon">⚠️</span>
                    <p>{error}</p>
                    <button onClick={loadAuditLogs} className="btn-retry">
                        Try Again
                    </button>
                </div>
            </div>
        );
    }

    // ✅ NEW RULE: Empty state
    if (logs.length === 0) {
        return (
            <div className="audit-log-viewer">
                <div className="audit-log-empty">
                    <span className="empty-icon">📋</span>
                    <p>No audit history available</p>
                    <span className="empty-subtitle">Changes will appear here once actions are performed</span>
                </div>
            </div>
        );
    }

    return (
        <div className="audit-log-viewer">
            <div className="audit-log-header">
                <h3>📋 Audit History</h3>
                <span className="audit-log-count">{logs.length} {logs.length === 1 ? 'entry' : 'entries'}</span>
            </div>

            <div className="audit-log-timeline">
                {logs.map((log, index) => {
                    const isExpanded = expandedLog === log.id;
                    const oldValues = parseJsonSafe(log.oldValues);
                    const newValues = parseJsonSafe(log.newValues);
                    const hasDetails = oldValues || newValues;

                    return (
                        <div key={log.id} className="audit-log-entry">
                            <div className="audit-log-connector">
                                {index !== logs.length - 1 && <div className="connector-line"></div>}
                            </div>

                            <div
                                className="audit-log-icon"
                                style={{ backgroundColor: getActionColor(log.action) }}
                            >
                                {getActionIcon(log.action)}
                            </div>

                            <div className="audit-log-content">
                                <div className="audit-log-main">
                                    <div className="audit-log-action">
                                        <strong>{log.action}</strong>
                                        {log.reason && <span className="audit-log-reason"> — {log.reason}</span>}
                                    </div>

                                    <div className="audit-log-meta">
                                        <span className="audit-log-user">👤 {log.changedBy}</span>
                                        <span className="audit-log-time">
                                            🕒 {formatDistanceToNow(new Date(log.changedAt), { addSuffix: true })}
                                        </span>
                                        {log.ipAddress && (
                                            <span className="audit-log-ip">🌐 {log.ipAddress}</span>
                                        )}
                                    </div>

                                    {hasDetails && (
                                        <button
                                            className="audit-log-toggle"
                                            onClick={() => toggleExpand(log.id)}
                                        >
                                            {isExpanded ? '▼ Hide Details' : '▶ Show Details'}
                                        </button>
                                    )}
                                </div>

                                {isExpanded && hasDetails && (
                                    <div className="audit-log-details">
                                        {oldValues && (
                                            <div className="audit-log-values">
                                                <h4>Old Values:</h4>
                                                <pre>{JSON.stringify(oldValues, null, 2)}</pre>
                                            </div>
                                        )}

                                        {newValues && (
                                            <div className="audit-log-values">
                                                <h4>New Values:</h4>
                                                <pre>{JSON.stringify(newValues, null, 2)}</pre>
                                            </div>
                                        )}

                                        {log.userAgent && (
                                            <div className="audit-log-technical">
                                                <strong>User Agent:</strong> {log.userAgent}
                                            </div>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};
