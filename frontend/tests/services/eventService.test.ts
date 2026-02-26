import { describe, it, expect, vi, beforeEach } from 'vitest';
import { EventService, EventEntity, EventParticipant } from '../../src/services/eventService';

// Mock the api module
vi.mock('../../src/services/api', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};
Object.defineProperty(window, 'localStorage', { value: localStorageMock });

import { api } from '../../src/services/api';

describe('EventService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorageMock.getItem.mockReturnValue(null);
    vi.spyOn(console, 'warn').mockImplementation(() => {});
  });

  describe('getEvents', () => {
    it('returns events from API when successful', async () => {
      const mockEvents: EventEntity[] = [
        {
          id: 'evt-1',
          eventCode: 'CONF-2024',
          title: 'Annual Conference',
          eventType: 'Conference',
          startDate: '2024-06-01',
          endDate: '2024-06-05',
        },
      ];
      vi.mocked(api.get).mockResolvedValueOnce({ data: mockEvents });

      const result = await EventService.getEvents();

      expect(api.get).toHaveBeenCalledWith('/events');
      expect(result).toHaveLength(1);
      expect(result[0].eventCode).toBe('CONF-2024');
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await EventService.getEvents();

      // Should return mock events
      expect(Array.isArray(result)).toBe(true);
    });
  });

  describe('getEvent', () => {
    it('returns single event from API when successful', async () => {
      const mockEvent: EventEntity = {
        id: 'evt-1',
        eventCode: 'CONF-2024',
        title: 'Annual Conference',
        eventType: 'Conference',
        startDate: '2024-06-01',
        endDate: '2024-06-05',
        attributes: '{"location":"Paris"}',
      };
      vi.mocked(api.get).mockResolvedValueOnce({ data: mockEvent });

      const result = await EventService.getEvent('evt-1');

      expect(api.get).toHaveBeenCalledWith('/events/evt-1');
      expect(result?.eventCode).toBe('CONF-2024');
      expect(result?.parsedAttributes?.location).toBe('Paris');
    });

    it('parses attributes JSON when present', async () => {
      const mockEvent: EventEntity = {
        id: 'evt-1',
        eventCode: 'CONF-2024',
        title: 'Conference',
        eventType: 'Conference',
        startDate: '2024-06-01',
        endDate: '2024-06-05',
        attributes: '{"sector":"Climate","field_office":"Geneva"}',
      };
      vi.mocked(api.get).mockResolvedValueOnce({ data: mockEvent });

      const result = await EventService.getEvent('evt-1');

      expect(result?.parsedAttributes?.sector).toBe('Climate');
      expect(result?.parsedAttributes?.field_office).toBe('Geneva');
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await EventService.getEvent('evt-1');

      // Should return undefined or mock event
      expect(result === undefined || result?.id === 'evt-1').toBe(true);
    });
  });

  describe('createEvent', () => {
    it('creates event via API when successful', async () => {
      const newEvent: EventEntity = {
        eventCode: 'NEW-2024',
        title: 'New Event',
        eventType: 'Event',
        startDate: '2024-07-01',
        endDate: '2024-07-02',
        parsedAttributes: { location: 'New York' },
      };
      const responseEvent = { ...newEvent, id: 'evt-new-123' };
      vi.mocked(api.post).mockResolvedValueOnce({ data: responseEvent });

      const result = await EventService.createEvent(newEvent);

      expect(api.post).toHaveBeenCalledWith('/events', expect.objectContaining({
        eventCode: 'NEW-2024',
        attributes: '{"location":"New York"}',
      }));
      expect(result.id).toBe('evt-new-123');
    });

    it('falls back to mock creation when API fails', async () => {
      vi.mocked(api.post).mockRejectedValueOnce(new Error('Network error'));

      const newEvent: EventEntity = {
        eventCode: 'NEW-2024',
        title: 'New Event',
        eventType: 'Event',
        startDate: '2024-07-01',
        endDate: '2024-07-02',
      };

      const result = await EventService.createEvent(newEvent);

      expect(result.id).toMatch(/^evt-/);
      expect(result.eventCode).toBe('NEW-2024');
    });
  });

  describe('getParticipants', () => {
    it('returns participants from API when successful', async () => {
      const mockParticipants: EventParticipant[] = [
        {
          id: 'p-1',
          eventId: 'evt-1',
          fullName: 'John Doe',
          email: 'john@example.com',
          tier: 'TIER_1',
          status: 'Pending',
        },
      ];
      vi.mocked(api.get).mockResolvedValueOnce({ data: mockParticipants });

      const result = await EventService.getParticipants('evt-1');

      expect(api.get).toHaveBeenCalledWith('/events/evt-1/participants');
      expect(result).toHaveLength(1);
      expect(result[0].fullName).toBe('John Doe');
    });

    it('falls back to mock data when API fails', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      const result = await EventService.getParticipants('evt-1');

      // Should return mock participants or empty array
      expect(Array.isArray(result)).toBe(true);
    });
  });

  describe('addParticipants', () => {
    it('adds participants via API when successful', async () => {
      const newParticipants: EventParticipant[] = [
        {
          fullName: 'Jane Smith',
          email: 'jane@example.com',
          tier: 'TIER_2',
        },
      ];
      const responseParticipants = [
        { ...newParticipants[0], id: 'p-new-1', eventId: 'evt-1' },
      ];
      vi.mocked(api.post).mockResolvedValueOnce({ data: responseParticipants });

      const result = await EventService.addParticipants('evt-1', newParticipants);

      expect(api.post).toHaveBeenCalledWith('/events/evt-1/participants', newParticipants);
      expect(result).toHaveLength(1);
      expect(result[0].id).toBe('p-new-1');
    });

    it('falls back to mock add when API fails', async () => {
      vi.mocked(api.post).mockRejectedValueOnce(new Error('Network error'));

      const newParticipants: EventParticipant[] = [
        {
          fullName: 'Jane Smith',
          email: 'jane@example.com',
          tier: 'TIER_2',
        },
      ];

      const result = await EventService.addParticipants('evt-1', newParticipants);

      expect(result).toHaveLength(1);
      expect(result[0].fullName).toBe('Jane Smith');
    });
  });

  describe('updateParticipant', () => {
    it('updates participant via API when successful', async () => {
      const participant: EventParticipant = {
        id: 'p-1',
        fullName: 'John Updated',
        email: 'john.updated@example.com',
        tier: 'TIER_3',
      };
      vi.mocked(api.put).mockResolvedValueOnce({ data: participant });

      const result = await EventService.updateParticipant('evt-1', participant);

      expect(api.put).toHaveBeenCalledWith('/events/evt-1/participants/p-1', participant);
      expect(result.fullName).toBe('John Updated');
    });

    it('falls back to mock update when API fails', async () => {
      vi.mocked(api.put).mockRejectedValueOnce(new Error('Network error'));

      const participant: EventParticipant = {
        id: 'p-1',
        fullName: 'John Updated',
        email: 'john.updated@example.com',
        tier: 'TIER_3',
      };

      const result = await EventService.updateParticipant('evt-1', participant);

      expect(result.fullName).toBe('John Updated');
    });
  });

  describe('inviteTier3', () => {
    it('invites tier 3 participants via API when successful', async () => {
      const participantIds = ['p-1', 'p-2'];
      vi.mocked(api.post).mockResolvedValueOnce({ data: { invitedCount: 2 } });

      const result = await EventService.inviteTier3('evt-1', participantIds);

      expect(api.post).toHaveBeenCalledWith('/events/evt-1/invite-tier3', { participantIds });
      expect(result.invitedCount).toBe(2);
    });

    it('falls back to mock invite when API fails', async () => {
      vi.mocked(api.post).mockRejectedValueOnce(new Error('Network error'));

      const participantIds = ['p-1', 'p-2'];

      const result = await EventService.inviteTier3('evt-1', participantIds);

      expect(result.invitedCount).toBe(2);
    });
  });
});
