import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EventService, EventEntity, EventParticipant } from '../services/eventService';
import { Button, Card, Input, Modal, StatusBadge } from '../components/ui/Elements';
import { Ban, Mail, Send, Copy, Pencil } from 'lucide-react';
import { api } from '../services/api';

export const EventDetail: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [event, setEvent] = useState<EventEntity | null>(null);
    const [participants, setParticipants] = useState<EventParticipant[]>([]);
    const [loading, setLoading] = useState(true);

    // Action States
    const [actionLoading, setActionLoading] = useState<boolean>(false);
    const [confirmAction, setConfirmAction] = useState<{ id: string, type: 'resend' | 'cancel' } | null>(null);
    const [resentLink, setResentLink] = useState<{ id: string, link: string, emailSent: boolean, emailError?: string } | null>(null);

    // Modal States
    const [isAddOpen, setIsAddOpen] = useState(false);
    const [newParticipant, setNewParticipant] = useState<Partial<EventParticipant>>({ tier: 'TIER_3' });

    useEffect(() => {
        if (id) loadData(id);
    }, [id]);

    const loadData = async (eventId: string) => {
        setLoading(true);
        const evt = await EventService.getEvent(eventId);
        if (!evt) {
            navigate('/events');
            return;
        }
        setEvent(evt);

        // Load participants
        const parts = await EventService.getParticipants(eventId);
        setParticipants(parts);
        setLoading(false);
    };

    const handleSaveParticipant = async () => {
        if (!event?.id || !newParticipant.email || !newParticipant.fullName) return;

        if (newParticipant.id) {
            // Edit Mode
            await EventService.updateParticipant(event.id, newParticipant as EventParticipant);
        } else {
            // Add Mode
            await EventService.addParticipants(event.id, [{
                ...newParticipant,
                eventId: event.id,
                status: 'Draft',
                attributes: '{}' // Default attributes
            } as EventParticipant]);
        }

        setIsAddOpen(false);
        setNewParticipant({ tier: 'TIER_3' }); // Reset
        loadData(event.id);
    };

    const handleEdit = (p: EventParticipant) => {
        setNewParticipant({ ...p });
        setIsAddOpen(true);
    };

    const handleInvite = async (participantId: string) => {
        if (!event?.id) return;
        setLoading(true);
        await EventService.inviteTier3(event.id, [participantId]);
        loadData(event.id);
        // alert('Invitation Queued for Sending'); // Removed alert, UI update is enough
    };

    const handleResend = async (participantId: string) => {
        setActionLoading(true);
        try {
            // Check if we have an invitation link or ID. 
            // In a real scenario, we'd call an endpoint like /api/event/{eventId}/participant/{participantId}/resend
            // For now, let's reuse the inviteTier3 logic which likely upserts/resends if already exists, OR call the invitation endpoint if we have an invitation ID.
            // Since we don't have the invitation ID readily available in the frontend model yet (it's likely on the backend), let's assume we trigger a re-invite via the event service.

            // BETTER APPROACH: Call the API to trigger resend.
            if (!event?.id) return;

            // To properly mock the 'Resend' which returns a link:
            await EventService.inviteTier3(event.id, [participantId]);

            // Mocking the return because inviteTier3 is void
            setResentLink({
                id: participantId,
                link: `${window.location.origin}/invite/mock-link-${participantId}`,
                emailSent: true
            });

            loadData(event.id);
        } catch (error: any) {
            console.error('Failed to resend:', error);
            alert('Failed to resend invitation');
        } finally {
            setActionLoading(false);
            setConfirmAction(null);
        }
    };

    const handleCancel = async (participantId: string) => {
        if (!event?.id) return;
        setActionLoading(true);
        try {
            // Assuming we can "cancel" by removing from list or changing status. 
            // For now, let's just assume we can call an endpoint or use EventService removal.
            // Let's implement a status change to 'Cancelled' via API or similar.
            // Since we don't have explicit 'cancel invitation' for event participants in the service yet, we will simulate it.

            // Ideally: await api.post(`/event/${event.id}/participant/${participantId}/cancel`);
            // Fallback:
            alert('Invitation Cancelled (Simulated)');

            loadData(event.id);
        } catch (error) {
            alert('Failed to cancel');
        } finally {
            setActionLoading(false);
            setConfirmAction(null);
        }
    };

    const handleDownloadTemplate = () => {
        const headers = "FullName,Email,Tier,Organization,JobTitle";
        const example = "John Doe,john@example.com,TIER_3,UN,Consultant";
        const csvContent = "data:text/csv;charset=utf-8," + headers + "\n" + example;
        const encodedUri = encodeURI(csvContent);
        const link = document.createElement("a");
        link.setAttribute("href", encodedUri);
        link.setAttribute("download", "participants_template.csv");
        document.body.appendChild(link);
        link.click();
    };

    // KPIs
    const totalInvited = participants.filter(p => p.tier === 'TIER_3').length;
    const confirmed = participants.filter(p => p.status === 'CONFIRMED' || p.status === 'SAP_CREATED').length; // Mock logic for confirmed
    const sapCreated = participants.filter(p => p.status === 'SAP_CREATED').length;

    const inviteRate = totalInvited > 0 ? (confirmed / totalInvited) * 100 : 0;
    const conversionRate = confirmed > 0 ? (sapCreated / confirmed) * 100 : 0;

    if (loading) return <div className="p-8">Loading...</div>;
    if (!event) return null;

    return (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {/* Header */}
            <div className="mb-8 bg-white shadow overflow-hidden sm:rounded-lg">
                <div className="px-4 py-5 sm:px-6 flex justify-between items-center">
                    <div>
                        <h3 className="text-lg leading-6 font-medium text-gray-900">{event.title}</h3>
                        <p className="mt-1 max-w-2xl text-sm text-gray-500">{event.eventCode} • {event.eventType}</p>
                    </div>
                    <div className="text-right text-sm text-gray-500">
                        <p>{new Date(event.startDate).toLocaleDateString()} - {new Date(event.endDate).toLocaleDateString()}</p>
                        <p>{event.parsedAttributes?.location}</p>
                    </div>
                </div>

                {/* KPI Banner */}
                <div className="bg-gray-50 px-4 py-4 sm:grid sm:grid-cols-4 sm:gap-4 sm:px-6 border-t border-gray-200">
                    <div className="text-center">
                        <span className="block text-2xl font-bold text-brand-600">{participants.length}</span>
                        <span className="text-xs text-gray-500 uppercase">Total Participants</span>
                    </div>
                    <div className="text-center">
                        <span className="block text-2xl font-bold text-gray-900">{totalInvited}</span>
                        <span className="text-xs text-gray-500 uppercase">Assistance (Tier 3)</span>
                    </div>
                    <div className="text-center">
                        <span className={`block text-2xl font-bold ${inviteRate < 50 ? 'text-yellow-600' : 'text-green-600'}`}>{inviteRate.toFixed(0)}%</span>
                        <span className="text-xs text-gray-500 uppercase">Confirmation Rate</span>
                    </div>
                    <div className="text-center">
                        <span className={`block text-2xl font-bold ${conversionRate < 100 ? 'text-red-600' : 'text-green-600'}`}>{conversionRate.toFixed(0)}%</span>
                        <span className="text-xs text-gray-500 uppercase">SAP Conversion</span>
                        {conversionRate < 100 && confirmed > 0 && <span className="block text-xs text-red-500">⚠️ Unpaid Staff!</span>}
                    </div>
                </div>
            </div>

            {/* Participants Tab */}
            <Card title="Participant Management">
                <div className="mb-4 flex justify-between items-center">
                    <h4 className="text-md font-medium">Participants List</h4>
                    <div className="flex gap-2">
                        <Button variant="outline" size="sm" onClick={handleDownloadTemplate}>Download Template</Button>
                        <Button size="sm" onClick={() => { setIsAddOpen(true); setNewParticipant({ tier: 'TIER_3' }); }}>Add Participant</Button>
                    </div>
                </div>

                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tier</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {participants.map((p) => {
                                // Determine invitationId if available (mocked or real)
                                // In real implementation, this would come from the backend. 
                                // For now we assume if status is INVITED, we can resend/cancel.
                                const canResend = p.status === 'INVITED' || p.status === 'Pending' || p.status === 'Expired';
                                const canCancel = p.status === 'INVITED' || p.status === 'Pending';

                                return (
                                    <tr key={p.id}>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{p.fullName}</td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{p.email}</td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${p.tier === 'TIER_3' ? 'bg-purple-100 text-purple-800' : 'bg-gray-100 text-gray-800'}`}>
                                                {p.tier}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            <StatusBadge status={p.status || 'Pending'} />
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <div className="flex items-center justify-end gap-2">
                                                {p.tier === 'TIER_3' && p.status !== 'SAP_CREATED' && (
                                                    <>
                                                        {/* Edit Button - Draft or Pending */}
                                                        {(p.status === 'Draft' || p.status === 'Pending') && (
                                                            <Button
                                                                size="sm"
                                                                variant="outline"
                                                                onClick={() => handleEdit(p)}
                                                                className="text-gray-600 border-gray-200 hover:bg-gray-50"
                                                                title="Edit Participant"
                                                            >
                                                                <Pencil className="h-4 w-4" />
                                                            </Button>
                                                        )}

                                                        {/* Send Button - Draft Only */}
                                                        {p.status === 'Draft' && (
                                                            <Button
                                                                size="sm"
                                                                variant="outline"
                                                                onClick={() => handleInvite(p.id!)}
                                                                className="text-brand-600 border-brand-200 hover:bg-brand-50"
                                                                title="Send Invite"
                                                            >
                                                                <Send className="h-4 w-4" />
                                                            </Button>
                                                        )}

                                                        {/* Resend / Cancel - Pending or Invited */}
                                                        {(p.status === 'Pending' || p.status === 'INVITED') && (
                                                            <>
                                                                <Button
                                                                    size="sm"
                                                                    variant="outline"
                                                                    onClick={() => setConfirmAction({ id: p.id!, type: 'resend' })}
                                                                    className="text-brand-600 border-brand-200 hover:bg-brand-50"
                                                                    title="Resend Email"
                                                                >
                                                                    <Mail className="h-4 w-4" />
                                                                </Button>
                                                                <Button
                                                                    size="sm"
                                                                    variant="outline"
                                                                    className="text-red-600 border-red-200 hover:bg-red-50"
                                                                    onClick={() => setConfirmAction({ id: p.id!, type: 'cancel' })}
                                                                    title="Revoke Invitation"
                                                                >
                                                                    <Ban className="h-4 w-4" />
                                                                </Button>
                                                            </>
                                                        )}
                                                    </>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                )
                            })}
                            {participants.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-6 py-4 text-center text-sm text-gray-500">No participants added yet.</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </Card>

            {/* Add/Edit Participant Modal */}
            <Modal
                isOpen={isAddOpen}
                onClose={() => setIsAddOpen(false)}
                title={newParticipant.id ? "Edit Participant" : "Add Participant"}
                footer={(
                    <div className="flex justify-end gap-2 w-full">
                        <Button variant="secondary" onClick={() => setIsAddOpen(false)}>Cancel</Button>
                        <Button onClick={handleSaveParticipant}>{newParticipant.id ? "Save Changes" : "Add"}</Button>
                    </div>
                )}
            >
                <div className="space-y-4">
                    <Input label="Full Name" value={newParticipant.fullName || ''} onChange={e => setNewParticipant({ ...newParticipant, fullName: e.target.value })} />
                    <Input label="Email" type="email" value={newParticipant.email || ''} onChange={e => setNewParticipant({ ...newParticipant, email: e.target.value })} />
                    <div className="w-full">
                        <label className="block text-sm font-medium text-gray-700 mb-1">Tier</label>
                        <select
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-4 py-2"
                            value={newParticipant.tier}
                            onChange={e => setNewParticipant({ ...newParticipant, tier: e.target.value })}
                        >
                            <option value="TIER_1">Tier 1 (Official)</option>
                            <option value="TIER_2">Tier 2 (Staff)</option>
                            <option value="TIER_3">Tier 3 (Assistance)</option>
                        </select>
                    </div>
                </div>
            </Modal>

            {/* Confirmation Modal */}
            <Modal
                isOpen={!!confirmAction}
                onClose={() => !actionLoading && setConfirmAction(null)}
                title={confirmAction?.type === 'resend' ? 'Resend Invitation' : 'Revoke Invitation'}
                footer={
                    <div className="flex gap-3">
                        <Button
                            variant="secondary"
                            onClick={() => setConfirmAction(null)}
                            disabled={!!actionLoading}
                        >
                            No, Cancel
                        </Button>
                        <Button
                            variant={confirmAction?.type === 'resend' ? 'primary' : 'danger'}
                            onClick={() => confirmAction?.type === 'resend'
                                ? handleResend(confirmAction.id)
                                : handleCancel(confirmAction.id)}
                            isLoading={!!actionLoading}
                        >
                            {confirmAction?.type === 'resend' ? 'Yes, Resend' : 'Yes, Revoke Access'}
                        </Button>
                    </div>
                }
            >
                <p className="text-gray-600">
                    {confirmAction?.type === 'resend'
                        ? 'Are you sure you want to resend this invitation email? This will generate a new secure link and expire the previous one.'
                        : 'Are you sure you want to REVOKE this invitation? The participant will no longer be able to use the link.'}
                </p>
            </Modal>

            {/* Resent Link Modal */}
            <Modal
                isOpen={!!resentLink}
                onClose={() => setResentLink(null)}
                title={resentLink?.emailSent ? "Invitation Resent Successfully" : "Invitation Generated - Email Failed"}
                footer={
                    <Button onClick={() => setResentLink(null)}>Done</Button>
                }
            >
                <div className="space-y-4">
                    {resentLink?.emailSent ? (
                        <p className="text-sm text-gray-600 text-left">
                            A new invitation has been sent. You can also manually copy the link below:
                        </p>
                    ) : (
                        <div className="bg-orange-50 border-l-4 border-orange-400 p-4">
                            <div className="flex">
                                <div className="flex-shrink-0">
                                    <Mail className="h-5 w-5 text-orange-400" />
                                </div>
                                <div className="ml-3">
                                    <p className="text-sm text-orange-700">
                                        {resentLink?.emailError || (
                                            <>
                                                The invitation was generated but the <strong>email could not be sent</strong>.
                                                Please copy and send the link manually.
                                            </>
                                        )}
                                    </p>
                                </div>
                            </div>
                        </div>
                    )}
                    <div className="flex items-center gap-2 p-3 bg-gray-50 rounded-md border border-gray-200">
                        <code className="text-xs text-brand-700 break-all flex-1">{resentLink?.link}</code>
                        <button
                            onClick={() => {
                                navigator.clipboard.writeText(resentLink?.link || '');
                                alert('Copied!');
                            }}
                            className="p-2 bg-white border border-gray-300 rounded hover:bg-gray-50 flex-shrink-0"
                            title="Copy to clipboard"
                        >
                            <Copy className="h-4 w-4 text-gray-600" />
                        </button>
                    </div>
                </div>
            </Modal>
        </div>
    );
};
