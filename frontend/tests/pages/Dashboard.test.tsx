import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import '@testing-library/jest-dom';
import { Dashboard } from '../../src/pages/Dashboard';
import { ChangeRequest, ChangeRequestStatus } from '../../src/types';

// Mock navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock AuthContext
vi.mock('../../src/context/AuthContext', () => ({
  useAuth: () => ({
    user: { name: 'Test User', email: 'test@example.com' },
    isAuthenticated: true,
  }),
}));

// Mock VendorService
const mockGetChangeRequests = vi.fn();
vi.mock('../../src/services/vendorService', () => ({
  VendorService: {
    getChangeRequests: () => mockGetChangeRequests(),
  },
}));

const RouterWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>{children}</BrowserRouter>
);

describe('Dashboard Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders welcome message with user name', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    expect(screen.getByText(/Welcome, Test User/i)).toBeInTheDocument();
  });

  it('renders description text', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    expect(screen.getByText(/Manage your vendor profile/i)).toBeInTheDocument();
  });

  it('shows loading state initially', () => {
    mockGetChangeRequests.mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve([]), 1000))
    );

    render(<Dashboard />, { wrapper: RouterWrapper });

    expect(screen.getByText(/Loading requests/i)).toBeInTheDocument();
  });

  it('displays pending requests count', async () => {
    const mockRequests: ChangeRequest[] = [
      {
        id: 'cr-001',
        sapVendorId: '100450',
        requestType: 'Address Change',
        status: ChangeRequestStatus.New,
        createdAt: '2024-01-15',
        items: [],
        attachments: [],
      },
      {
        id: 'cr-002',
        sapVendorId: '100450',
        requestType: 'Bank Change',
        status: ChangeRequestStatus.InReview,
        createdAt: '2024-01-16',
        items: [],
        attachments: [],
      },
    ];
    mockGetChangeRequests.mockResolvedValueOnce(mockRequests);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('2')).toBeInTheDocument(); // Pending count
    });
  });

  it('displays approved requests count', async () => {
    const mockRequests: ChangeRequest[] = [
      {
        id: 'cr-001',
        sapVendorId: '100450',
        requestType: 'Address Change',
        status: ChangeRequestStatus.Approved,
        createdAt: '2024-01-15',
        items: [],
        attachments: [],
      },
      {
        id: 'cr-002',
        sapVendorId: '100450',
        requestType: 'Bank Change',
        status: ChangeRequestStatus.Applied,
        createdAt: '2024-01-16',
        items: [],
        attachments: [],
      },
    ];
    mockGetChangeRequests.mockResolvedValueOnce(mockRequests);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('2')).toBeInTheDocument(); // Approved count
    });
  });

  it('displays "No requests found" when no data', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText(/No requests found/i)).toBeInTheDocument();
    });
  });

  it('renders requests table when data is available', async () => {
    const mockRequests: ChangeRequest[] = [
      {
        id: 'cr-001',
        sapVendorId: '100450',
        requestType: 'Address Change',
        status: ChangeRequestStatus.New,
        createdAt: '2024-01-15',
        items: [],
        attachments: [],
      },
    ];
    mockGetChangeRequests.mockResolvedValueOnce(mockRequests);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('cr-001')).toBeInTheDocument();
      expect(screen.getByText('Address Change')).toBeInTheDocument();
    });
  });

  it('renders New Request button', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    const newRequestButton = screen.getByRole('button', { name: /New Request/i });
    expect(newRequestButton).toBeInTheDocument();
  });

  it('renders View Master Data button', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    const viewMasterDataButton = screen.getByRole('button', { name: /View Master Data/i });
    expect(viewMasterDataButton).toBeInTheDocument();
  });

  it('renders "View all requests" link', async () => {
    mockGetChangeRequests.mockResolvedValueOnce([]);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText(/View all requests/i)).toBeInTheDocument();
    });
  });

  it('limits displayed requests to 5', async () => {
    const mockRequests: ChangeRequest[] = Array.from({ length: 10 }, (_, i) => ({
      id: `cr-${String(i + 1).padStart(3, '0')}`,
      sapVendorId: '100450',
      requestType: 'Test Request',
      status: ChangeRequestStatus.New,
      createdAt: '2024-01-15',
      items: [],
      attachments: [],
    }));
    mockGetChangeRequests.mockResolvedValueOnce(mockRequests);

    render(<Dashboard />, { wrapper: RouterWrapper });

    await waitFor(() => {
      // Only first 5 should be displayed
      expect(screen.getByText('cr-001')).toBeInTheDocument();
      expect(screen.getByText('cr-005')).toBeInTheDocument();
      expect(screen.queryByText('cr-006')).not.toBeInTheDocument();
    });
  });
});
