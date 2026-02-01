import { api } from './api';

export interface EventEntity {
    id?: string;
    eventCode: string;
    title: string;
    eventType: 'Event' | 'Conference';
    startDate: string;
    endDate: string;
    createdBy?: string;
    createdAt?: string;
    attributes?: string; // JSON string
    parsedAttributes?: {
        sector?: string;
        field_office?: string;
        location?: string;
        financial_coding?: {
            wbs_element?: string;
            internal_order?: string;
            sap_vendor_id?: string;
        }
    };
}

export interface EventParticipant {
    id?: string;
    eventId?: string;
    email: string;
    fullName: string;
    tier: 'TIER_1' | 'TIER_2' | 'TIER_3' | string;
    status?: string;
    vendorInviteId?: string;
    attributes?: string;
    parsedAttributes?: {
        organization?: string;
        job_title?: string;
        notes?: string;
    };
}

export interface InviteRequest {
    participantIds: string[];
}

export const EventService = {
    getEvents: async (): Promise<EventEntity[]> => {
        try {
            const response = await api.get('/events');
            return response.data;
        } catch (e) {
            console.warn('Backend unreachable, using Mock for Events');
            return MOCK_EVENTS;
        }
    },

    getEvent: async (id: string): Promise<EventEntity | undefined> => {
        try {
            const response = await api.get(`/events/${id}`);
            const evt = response.data;
            if (evt.attributes && typeof evt.attributes === 'string') {
                try {
                    evt.parsedAttributes = JSON.parse(evt.attributes);
                } catch { }
            }
            return evt;
        } catch (e) {
            const mock = MOCK_EVENTS.find(e => e.id === id);
            return mock;
        }
    },

    createEvent: async (event: EventEntity): Promise<EventEntity> => {
        try {
            // Ensure attributes is string
            if (event.parsedAttributes) {
                event.attributes = JSON.stringify(event.parsedAttributes);
            }
            const response = await api.post('/events', event);
            return response.data;
        } catch (e) {
            console.warn('Backend unreachable, mock create');
            return { ...event, id: 'evt-' + Date.now() };
        }
    },

    getParticipants: async (eventId: string): Promise<EventParticipant[]> => {
        try {
            const response = await api.get(`/events/${eventId}/participants`);
            return response.data;
        } catch (e) {
            return MOCK_PARTICIPANTS[eventId] || [];
        }
    },

    addParticipants: async (eventId: string, participants: EventParticipant[]): Promise<EventParticipant[]> => {
        try {
            const response = await api.post(`/events/${eventId}/participants`, participants);
            return response.data;
        } catch (e) {
            console.warn('Backend unreachable, mock add');
            const newParticipants = participants.map(p => ({ ...p, id: p.id || 'part-' + Date.now() }));
            // Mock Persistence: Update the static store
            const currentList = MOCK_PARTICIPANTS[eventId] || [];
            MOCK_PARTICIPANTS[eventId] = [...currentList, ...newParticipants];
            saveMockParticipants(MOCK_PARTICIPANTS);
            return newParticipants;
        }
    },

    updateParticipant: async (eventId: string, participant: EventParticipant): Promise<EventParticipant> => {
        try {
            const response = await api.put(`/events/${eventId}/participants/${participant.id}`, participant);
            return response.data;
        } catch (e) {
            console.warn('Backend unreachable, mock update');
            // Mock Persistence: Update the static store
            const list = MOCK_PARTICIPANTS[eventId] || [];
            const index = list.findIndex(p => p.id === participant.id);
            if (index !== -1) {
                list[index] = { ...participant };
                MOCK_PARTICIPANTS[eventId] = [...list];
                saveMockParticipants(MOCK_PARTICIPANTS);
            }
            return participant;
        }
    },

    inviteTier3: async (eventId: string, participantIds: string[]): Promise<{ invitedCount: number }> => {
        try {
            const response = await api.post(`/events/${eventId}/invite-tier3`, { participantIds });
            return response.data;
        } catch (e) {
            console.warn('Backend unreachable, mock invite');
            // Mock Persistence: Update status to Pending
            const list = MOCK_PARTICIPANTS[eventId] || [];
            list.forEach(p => {
                if (participantIds.includes(p.id!)) {
                    p.status = 'Pending';
                }
            });
            MOCK_PARTICIPANTS[eventId] = [...list];
            saveMockParticipants(MOCK_PARTICIPANTS);
            return { invitedCount: participantIds.length };
        }
    }
};

// MOCK DATA STORAGE
const STORAGE_KEY_EVENTS = 'mock_events_data';
const STORAGE_KEY_PARTICIPANTS = 'mock_participants_data';

const loadMockEvents = (): EventEntity[] => {
    try {
        const stored = localStorage.getItem(STORAGE_KEY_EVENTS);
        return stored ? JSON.parse(stored) : DEFAULT_MOCK_EVENTS;
    } catch { return DEFAULT_MOCK_EVENTS; }
};

const loadMockParticipants = (): Record<string, EventParticipant[]> => {
    try {
        const stored = localStorage.getItem(STORAGE_KEY_PARTICIPANTS);
        return stored ? JSON.parse(stored) : DEFAULT_MOCK_PARTICIPANTS;
    } catch { return DEFAULT_MOCK_PARTICIPANTS; }
};

const saveMockEvents = (events: EventEntity[]) => {
    try { localStorage.setItem(STORAGE_KEY_EVENTS, JSON.stringify(events)); } catch { }
};

const saveMockParticipants = (participants: Record<string, EventParticipant[]>) => {
    try { localStorage.setItem(STORAGE_KEY_PARTICIPANTS, JSON.stringify(participants)); } catch { }
};

// INITIAL DATA
const DEFAULT_MOCK_EVENTS: EventEntity[] = [
    {
        id: 'evt-1',
        eventCode: 'COP-28',
        title: 'Climate Conference 2024',
        eventType: 'Conference',
        startDate: '2024-11-30T09:00:00Z',
        endDate: '2024-12-12T18:00:00Z',
        createdBy: 'admin@un.org',
        attributes: '{"location":"Dubai","sector":"Climate"}',
        parsedAttributes: { location: "Dubai", sector: "Climate" }
    }
];

const DEFAULT_MOCK_PARTICIPANTS: Record<string, EventParticipant[]> = {
    'evt-1': [
        {
            id: 'p-1',
            eventId: 'evt-1',
            fullName: 'Dr. Expert',
            email: 'expert@uni.edu',
            tier: 'TIER_3',
            status: 'Pending',
            attributes: '{"organization":"Uni"}'
        }
    ]
};

// Mutable Stores
let MOCK_EVENTS = loadMockEvents();
let MOCK_PARTICIPANTS = loadMockParticipants();
