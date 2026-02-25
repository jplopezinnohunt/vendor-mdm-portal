import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import '@testing-library/jest-dom';
import { VendorProfile } from '../../src/pages/VendorProfile';
import { VendorMasterData } from '../../src/types';

// Mock navigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock VendorService
const mockGetCurrentVendor = vi.fn();
vi.mock('../../src/services/vendorService', () => ({
  VendorService: {
    getCurrentVendor: () => mockGetCurrentVendor(),
  },
}));

const RouterWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>{children}</BrowserRouter>
);

const mockVendorData: VendorMasterData = {
  sapVendorId: '100450',
  name: 'Acme Corporation',
  legalForm: 'LLC',
  taxNumber1: 'US123456789',
  email: 'acme@example.com',
  phone: '+1-555-0100',
  address: {
    street: '123 Main Street',
    city: 'New York',
    postalCode: '10001',
    country: 'US',
  },
  banks: [
    {
      id: 'bank-1',
      bankCountry: 'US',
      bankKey: 'BOFA',
      bankAccount: '****1234',
      accountHolder: 'Acme Corporation',
      iban: 'US12345678901234',
    },
  ],
};

describe('VendorProfile Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading state initially', () => {
    mockGetCurrentVendor.mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve(mockVendorData), 1000))
    );

    render(<VendorProfile />, { wrapper: RouterWrapper });

    expect(screen.getByText(/Loading master data from SAP/i)).toBeInTheDocument();
  });

  it('renders page title after loading', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText(/Current Master Data/i)).toBeInTheDocument();
    });
  });

  it('displays vendor name', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('Acme Corporation')).toBeInTheDocument();
    });
  });

  it('displays SAP vendor ID', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('100450')).toBeInTheDocument();
    });
  });

  it('displays legal form', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('LLC')).toBeInTheDocument();
    });
  });

  it('displays tax ID', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('US123456789')).toBeInTheDocument();
    });
  });

  it('displays address information', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('123 Main Street')).toBeInTheDocument();
      expect(screen.getByText('New York')).toBeInTheDocument();
      expect(screen.getByText('10001')).toBeInTheDocument();
    });
  });

  it('displays bank details section', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText(/Bank Details/i)).toBeInTheDocument();
      expect(screen.getByText('BOFA')).toBeInTheDocument();
      expect(screen.getByText('****1234')).toBeInTheDocument();
      // Note: 'US' appears multiple times (address country and bank country)
      const usTexts = screen.getAllByText('US');
      expect(usTexts.length).toBeGreaterThanOrEqual(1);
    });
  });

  it('renders Request Change button', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      const button = screen.getByRole('button', { name: /Request Change/i });
      expect(button).toBeInTheDocument();
    });
  });

  it('displays read-only notice', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(
        screen.getByText(/This data is retrieved directly from the SAP ERP system/i)
      ).toBeInTheDocument();
    });
  });

  it('displays dash for missing optional fields', async () => {
    const vendorWithMissingFields: VendorMasterData = {
      ...mockVendorData,
      legalForm: undefined,
      taxNumber1: undefined,
    };
    mockGetCurrentVendor.mockResolvedValueOnce(vendorWithMissingFields);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      const dashes = screen.getAllByText('-');
      expect(dashes.length).toBeGreaterThanOrEqual(1);
    });
  });

  it('displays multiple bank accounts', async () => {
    const vendorWithMultipleBanks: VendorMasterData = {
      ...mockVendorData,
      banks: [
        { id: 'bank-1', bankCountry: 'US', bankKey: 'BOFA', bankAccount: '****1234', accountHolder: 'Acme Corporation', iban: 'US12345678901234' },
        { id: 'bank-2', bankCountry: 'DE', bankKey: 'DEUTDE', bankAccount: '****5678', accountHolder: 'Acme GmbH', iban: 'DE89370400440532013000' },
      ],
    };
    mockGetCurrentVendor.mockResolvedValueOnce(vendorWithMultipleBanks);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('****1234')).toBeInTheDocument();
      expect(screen.getByText('****5678')).toBeInTheDocument();
    });
  });

  it('shows error state when data fails to load', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(null);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText(/Failed to load data/i)).toBeInTheDocument();
    });
  });

  it('renders General Information card', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('General Information')).toBeInTheDocument();
    });
  });

  it('renders Address card', async () => {
    mockGetCurrentVendor.mockResolvedValueOnce(mockVendorData);

    render(<VendorProfile />, { wrapper: RouterWrapper });

    await waitFor(() => {
      expect(screen.getByText('Address')).toBeInTheDocument();
    });
  });
});
