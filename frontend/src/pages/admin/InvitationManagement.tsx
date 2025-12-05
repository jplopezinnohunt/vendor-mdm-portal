import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, Card } from '../../components/ui/Elements';
import { Plus, RefreshCw, Mail, CheckCircle, Clock, XCircle, Ban } from 'lucide-react';
import { api } from '../../services/api';

interface Invitation {
    id: string;
    vendorLegalName: string;
    primaryContactEmail: string;
    status: string;
    invitedByName: string;
    createdAt: string;
    expiresAt: string;
    vendorApplicationId?: string;
}

export const InvitationManagement: React.FC = () => {
    const [invitations, setInvitations] = useState<Invitation[]>([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState<string>('');
    const navigate = useNavigate();

    const loadInvitations = async () => {
        try {
            setLoading(true);
            const params = filter ? { status: filter } : {};
            const response = await api.get('/invitation/list', { params });
            setInvitations(response.data.invitations);
        } catch (error) {
            console.error('Failed to load invitations:', error);
            alert('Failed to load invitations');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadInvitations();
    }, [filter]);

    const handleResend = async (id: string) => {
        if (!confirm('Are you sure you want to resend this invitation?')) {
            return;
        }

        try {
            await api.post(`/invitation/resend/${id}`);
            alert('Invitation has been resent successfully!');
            loadInvitations();
        } catch (error) {
            console.error('Failed to resend invitation:', error);
            alert('Failed to resend invitation');
        }
    };

    const getStatusBadge = (status: string) => {
        const badges = {
            Pending: {
                icon: Clock,
                className: 'bg-yellow-100 text-yellow-800',
                label: 'Pending'
            },
            Accepted: {
                icon: Mail,
                className: 'bg-blue-100 text-blue-800',
                label: 'Accepted'
            },
            Completed: {
                icon: CheckCircle,
                className: 'bg-green-100 text-green-800',
                label: 'Completed'
            },
            Expired: {
                icon: XCircle,
                className: 'bg-gray-100 text-gray-800',
                label: 'Expired'
            },
            Cancelled: {
                icon: Ban,
                className: 'bg-red-100 text-red-800',
                label: 'Cancelled'
            }
        };

        const badge = badges[status as keyof typeof badges] || badges.Pending;
        const Icon = badge.icon;

        return (
            <span className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>
                <Icon className="h-3 w-3" />
                {badge.label}
            </span>
        );
    };

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const isExpiringSoon = (expiresAt: string) => {
        const now = new Date();
        const expires = new Date(expiresAt);
        const daysUntilExpiry = (expires.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
        return daysUntilExpiry > 0 && daysUntilExpiry <= 3;
    };

    return (
        <div className="min-h-screen bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-7xl mx-auto">
                {/* Header */}
                <div className="mb-8 flex justify-between items-center">
                    <div>
                        <h1 className="text-3xl font-bold text-gray-900">Vendor Invitations</h1>
                        <p className="mt-2 text-gray-600">
                            Manage and track vendor invitation links
                        </p>
                    </div>
                    <Button
                        onClick={() => navigate('/admin/invite-vendor')}
                        className="flex items-center gap-2"
                        size="lg"
                    >
                        <Plus className="h-5 w-5" />
                        New Invitation
                    </Button>
                </div>

                {/* Filters */}
                <Card className="mb-6">
                    <div className="flex flex-wrap gap-4 items-center">
                        <div className="flex-1 min-w-[200px]">
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Filter by Status
                            </label>
                            <select
                                value={filter}
                                onChange={(e) => setFilter(e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent"
                            >
                                <option value="">All Statuses</option>
                                <option value="Pending">Pending</option>
                                <option value="Accepted">Accepted</option>
                                <option value="Completed">Completed</option>
                                <option value="Expired">Expired</option>
                            </select>
                        </div>
                        <div className="flex items-end">
                            <Button
                                onClick={loadInvitations}
                                variant="secondary"
                                className="flex items-center gap-2"
                            >
                                <RefreshCw className="h-4 w-4" />
                                Refresh
                            </Button>
                        </div>
                    </div>
                </Card>

                {/* Invitations Table */}
                <Card>
                    {loading ? (
                        <div className="flex justify-center py-12">
                            <div className="h-8 w-8 animate-spin rounded-full border-4 border-brand-600 border-t-transparent"></div>
                        </div>
                    ) : invitations.length === 0 ? (
                        <div className="text-center py-12">
                            <Mail className="mx-auto h-12 w-12 text-gray-400" />
                            <h3 className="mt-2 text-sm font-medium text-gray-900">No invitations</h3>
                            <p className="mt-1 text-sm text-gray-500">
                                Get started by creating a new vendor invitation.
                            </p>
                            <div className="mt-6">
                                <Button
                                    onClick={() => navigate('/admin/invite-vendor')}
                                    className="inline-flex items-center gap-2"
                                >
                                    <Plus className="h-5 w-5" />
                                    New Invitation
                                </Button>
                            </div>
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Vendor
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Contact Email
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Status
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Invited By
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Created
                                        </th>
                                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Expires
                                        </th>
                                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                            Actions
                                        </th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-200">
                                    {invitations.map((invitation) => (
                                        <tr key={invitation.id} className="hover:bg-gray-50">
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm font-medium text-gray-900">
                                                    {invitation.vendorLegalName}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm text-gray-500">
                                                    {invitation.primaryContactEmail}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                {getStatusBadge(invitation.status)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {invitation.invitedByName}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {formatDate(invitation.createdAt)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                <div className="text-sm text-gray-500">
                                                    {formatDate(invitation.expiresAt)}
                                                    {isExpiringSoon(invitation.expiresAt) && invitation.status === 'Pending' && (
                                                        <span className="ml-2 text-xs text-orange-600 font-medium">
                                                            Expiring Soon
                                                        </span>
                                                    )}
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                                {invitation.status === 'Pending' && (
                                                    <button
                                                        onClick={() => handleResend(invitation.id)}
                                                        className="text-brand-600 hover:text-brand-900"
                                                    >
                                                        Resend
                                                    </button>
                                                )}
                                                {invitation.status === 'Expired' && (
                                                    <button
                                                        onClick={() => handleResend(invitation.id)}
                                                        className="text-brand-600 hover:text-brand-900"
                                                    >
                                                        Reactivate
                                                    </button>
                                                )}
                                                {invitation.vendorApplicationId && (
                                                    <button
                                                        onClick={() => navigate(`/approver/onboarding/${invitation.vendorApplicationId}`)}
                                                        className="ml-3 text-blue-600 hover:text-blue-900"
                                                    >
                                                        View Application
                                                    </button>
                                                )}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </Card>

                {/* Summary Stats */}
                {!loading && invitations.length > 0 && (
                    <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-4">
                        <Card className="p-4">
                            <div className="text-sm font-medium text-gray-500">Total Invitations</div>
                            <div className="mt-1 text-2xl font-semibold text-gray-900">
                                {invitations.length}
                            </div>
                        </Card>
                        <Card className="p-4">
                            <div className="text-sm font-medium text-gray-500">Pending</div>
                            <div className="mt-1 text-2xl font-semibold text-yellow-600">
                                {invitations.filter(i => i.status === 'Pending').length}
                            </div>
                        </Card>
                        <Card className="p-4">
                            <div className="text-sm font-medium text-gray-500">Completed</div>
                            <div className="mt-1 text-2xl font-semibold text-green-600">
                                {invitations.filter(i => i.status === 'Completed').length}
                            </div>
                        </Card>
                        <Card className="p-4">
                            <div className="text-sm font-medium text-gray-500">Expired</div>
                            <div className="mt-1 text-2xl font-semibold text-gray-600">
                                {invitations.filter(i => i.status === 'Expired').length}
                            </div>
                        </Card>
                    </div>
                )}
            </div>
        </div>
    );
};
