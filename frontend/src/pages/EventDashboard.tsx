import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EventService, EventEntity } from '../services/eventService';
import { Button, Card, Input, Modal, StatusBadge } from '../components/ui/Elements';

export const EventDashboard: React.FC = () => {
    const navigate = useNavigate();
    const [events, setEvents] = useState<EventEntity[]>([]);
    const [loading, setLoading] = useState(true);
    const [isModalOpen, setIsModalOpen] = useState(false);

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
        const data = await EventService.getEvents();
        setEvents(data);
        setLoading(false);
    };

    const handleCreate = async () => {
        if (!formData.title || !formData.eventCode) return;

        await EventService.createEvent({
            ...formData,
            startDate: formData.startDate || new Date().toISOString(),
            endDate: formData.endDate || new Date().toISOString(),
        } as EventEntity);

        setIsModalOpen(false);
        loadEvents();
    };

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

            {loading ? (
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {[1, 2, 3].map(i => <div key={i} className="h-48 bg-gray-100 rounded-lg animate-pulse"></div>)}
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
                onClose={() => setIsModalOpen(false)}
                title="Define New Event"
                footer={(
                    <div className="flex justify-end gap-2 w-full">
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>Cancel</Button>
                        <Button onClick={handleCreate}>Create Event</Button>
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
