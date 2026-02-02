import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EventService, EventEntity } from '../services/eventService';
import { Button, Card, Input, Modal } from '../components/ui/Elements';

export const EventDashboard: React.FC = () => {
    const navigate = useNavigate();
    const [events, setEvents] = useState<EventEntity[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form State
    const [formData, setFormData] = useState<Partial<EventEntity>>({
        eventType: 'Event',
        parsedAttributes: { financial_coding: {} }
    });

    useEffect(() => {
        loadEvents();
    }, []);

    const loadEvents = async () => {
        setLoading(true);
        setError(null);

        try {
            // Timeout after 10 seconds
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), 10000);

            const data = await EventService.getEvents();
            clearTimeout(timeoutId);
            setEvents(data);
        } catch (err: any) {
            console.error('Failed to load events:', err);
            if (err.name === 'AbortError') {
                setError('Request timed out. Please check your connection and try again.');
            } else {
                setError(err.message || 'Failed to load events. Please try again.');
            }
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = async () => {
        if (!formData.title || !formData.eventCode) {
            setError('Event Code and Title are required');
            return;
        }

        setIsSubmitting(true);
        setError(null);
        setSuccessMessage(null);

        try {
            await EventService.createEvent({
                ...formData,
                startDate: formData.startDate || new Date().toISOString(),
                endDate: formData.endDate || new Date().toISOString(),
            } as EventEntity);

            // Success!
            setSuccessMessage('Event created successfully!');
            setIsModalOpen(false);
            setFormData({ eventType: 'Event', parsedAttributes: { financial_coding: {} } });

            // Reload events
            await loadEvents();

            // Clear success message after 5 seconds
            setTimeout(() => setSuccessMessage(null), 5000);
        } catch (err: any) {
            console.error('Failed to create event:', err);
            setError(err.message || 'Failed to create event. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const dismissError = () => setError(null);
    const dismissSuccess = () => setSuccessMessage(null);

    return (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <div className="md:flex md:items-center md:justify-between mb-8">
                <div className="min-w-0 flex-1">
                    <h2 className="text-2xl font-bold leading-7 text-gray-900 sm:truncate sm:text-3xl sm:tracking-tight">
                        Event Management
                    </h2>
                    <p className="mt-1 text-sm text-gray-500">Manage UN official events and assistance recruitment.</p>
                </div>
                <div className="mt-4 flex md:ml-4 md:mt-0">
                    <Button onClick={() => setIsModalOpen(true)}>
                        <svg className="-ml-0.5 mr-1.5 h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm-1-11a1 1 0 102 0v2h2a1 1 0 100 2h-2v2a1 1 0 10-2 0v-2H6a1 1 0 100-2h2V7z" clipRule="evenodd" />
                        </svg>
                        Create Event
                    </Button>
                </div>
            </div>

            {/* Success Message */}
            {successMessage && (
                <div className="mb-4 rounded-md bg-green-50 p-4">
                    <div className="flex">
                        <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-green-400" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="ml-3">
                            <p className="text-sm font-medium text-green-800">{successMessage}</p>
                        </div>
                        <div className="ml-auto pl-3">
                            <button onClick={dismissSuccess} className="inline-flex rounded-md bg-green-50 p-1.5 text-green-500 hover:bg-green-100">
                                <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Error Message */}
            {error && (
                <div className="mb-4 rounded-md bg-red-50 p-4">
                    <div className="flex">
                        <div className="flex-shrink-0">
                            <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clipRule="evenodd" />
                            </svg>
                        </div>
                        <div className="ml-3 flex-1">
                            <p className="text-sm font-medium text-red-800">{error}</p>
                            <button onClick={dismissError} className="mt-2 text-sm text-red-600 hover:text-red-500 font-medium">Dismiss</button>
                        </div>
                    </div>
                </div>
            )}

            {loading ? (
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {[1, 2, 3].map(i => <div key={i} className="h-48 bg-gray-100 rounded-lg animate-pulse"></div>)}
                </div>
            ) : events.length === 0 ? (
                <div className="text-center py-12">
                    <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                    </svg>
                    <h3 className="mt-2 text-sm font-medium text-gray-900">No events</h3>
                    <p className="mt-1 text-sm text-gray-500">Get started by creating a new event.</p>
                    <div className="mt-6">
                        <Button onClick={() => setIsModalOpen(true)}>
                            <svg className="-ml-0.5 mr-1.5 h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm-1-11a1 1 0 102 0v2h2a1 1 0 100 2h-2v2a1 1 0 10-2 0v-2H6a1 1 0 100-2h2V7z" clipRule="evenodd" />
                            </svg>
                            Create Event
                        </Button>
                    </div>
                </div>
            ) : (
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {events.map((evt) => (
                        <div
                            key={evt.id}
                            onClick={() => navigate(`/events/${evt.id}`)}
                            className="cursor-pointer transition-transform hover:scale-105"
                        >
                            <Card className="h-full border-l-4 border-brand-500 hover:shadow-lg">
                                <div className="flex justify-between items-start">
                                    <div>
                                        <span className="inline-flex items-center rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10 mb-2">
                                            {evt.eventCode}
                                        </span>
                                        <h3 className="text-xl font-semibold text-gray-900">{evt.title}</h3>
                                        <p className="text-sm text-gray-500 mt-1">{evt.eventType}</p>
                                    </div>
                                </div>

                                <div className="mt-4 space-y-2 text-sm text-gray-600">
                                    <div className="flex items-center">
                                        <svg className="mr-1.5 h-5 w-5 text-gray-400" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M5.75 2a.75.75 0 01.75.75V4h7V2.75a.75.75 0 011.5 0V4h.25A2.75 2.75 0 0118 6.75v8.5A2.75 2.75 0 0115.25 18H4.75A2.75 2.75 0 012 15.25v-8.5A2.75 2.75 0 014.75 4H5V2.75A.75.75 0 015.75 2zm-1 5.5c-.69 0-1.25.56-1.25 1.25v6.5c0 .69.56 1.25 1.25 1.25h10.5c.69 0 1.25-.56 1.25-1.25v-6.5c0-.69-.56-1.25-1.25-1.25H4.75z" clipRule="evenodd" />
                                        </svg>
                                        {new Date(evt.startDate).toLocaleDateString()}
                                    </div>
                                </div>
                            </Card>
                        </div>
                    ))}
                </div>
            )}

            {/* Create Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => !isSubmitting && setIsModalOpen(false)}
                title="Define New Event"
                footer={(
                    <div className="flex justify-end gap-2 w-full">
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)} disabled={isSubmitting}>
                            Cancel
                        </Button>
                        <Button onClick={handleCreate} disabled={isSubmitting}>
                            {isSubmitting ? (
                                <>
                                    <svg className="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                    Creating...
                                </>
                            ) : 'Create Event'}
                        </Button>
                    </div>
                )}
            >
                <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <Input label="Event Code (e.g. EVT-2024)" value={formData.eventCode || ''} onChange={e => setFormData({ ...formData, eventCode: e.target.value })} />
                        <div className="w-full">
                            <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                            <select
                                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm px-4 py-2"
                                value={formData.eventType}
                                onChange={e => setFormData({ ...formData, eventType: e.target.value as any })}
                            >
                                <option value="Event">Event</option>
                                <option value="Conference">Conference</option>
                            </select>
                        </div>
                    </div>
                    <Input label="Event Title" value={formData.title || ''} onChange={e => setFormData({ ...formData, title: e.target.value })} />

                    <div className="grid grid-cols-2 gap-4">
                        <Input type="date" label="Start Date" value={formData.startDate?.split('T')[0]} onChange={e => setFormData({ ...formData, startDate: e.target.value })} />
                        <Input type="date" label="End Date" value={formData.endDate?.split('T')[0]} onChange={e => setFormData({ ...formData, endDate: e.target.value })} />
                    </div>

                    <div className="border-t pt-2 mt-2">
                        <h4 className="text-sm font-medium text-gray-900 mb-2">Context & Finance</h4>
                        <div className="grid grid-cols-2 gap-4">
                            <Input label="Sector" value={formData.parsedAttributes?.sector || ''}
                                onChange={e => setFormData({ ...formData, parsedAttributes: { ...formData.parsedAttributes, sector: e.target.value } })} />
                            <Input label="Field Office" value={formData.parsedAttributes?.field_office || ''}
                                onChange={e => setFormData({ ...formData, parsedAttributes: { ...formData.parsedAttributes, field_office: e.target.value } })} />
                        </div>
                        <Input label="SAP Vendor ID (Budget Holder)" value={formData.parsedAttributes?.financial_coding?.sap_vendor_id || ''}
                            onChange={e => setFormData({ ...formData, parsedAttributes: { ...formData.parsedAttributes, financial_coding: { ...formData.parsedAttributes?.financial_coding, sap_vendor_id: e.target.value } } })} />
                    </div>
                </div>
            </Modal>
        </div>
    );
};
