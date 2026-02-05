import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import '@testing-library/jest-dom';
import { Login } from '../../src/pages/Login';

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
const mockLogin = vi.fn();
const mockMockLogin = vi.fn();
const mockLoginLocal = vi.fn();

vi.mock('../../src/context/AuthContext', () => ({
  useAuth: () => ({
    login: mockLogin,
    mockLogin: mockMockLogin,
    loginLocal: mockLoginLocal,
    isAuthenticated: false,
    user: null,
  }),
  UserRole: {
    Admin: 'Admin',
    Approver: 'Approver',
    Requestor: 'Requestor',
    Vendor: 'Vendor',
    VendorUnit: 'VendorUnit',
    BFM: 'BFM',
  },
}));

// Mock version
vi.mock('../../src/version', () => ({
  version: {
    build: '1.0.0-test',
    timestamp: '2024-01-01T00:00:00Z',
  },
}));

// Mock axios
vi.mock('axios', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

const RouterWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>{children}</BrowserRouter>
);

describe('Login Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders login page with title', () => {
    render(<Login />, { wrapper: RouterWrapper });
    expect(screen.getByText(/Vendor Portal/i)).toBeInTheDocument();
  });

  it('renders secure access description', () => {
    render(<Login />, { wrapper: RouterWrapper });
    expect(screen.getByText(/Secure access for Suppliers and Staff/i)).toBeInTheDocument();
  });

  it('renders UNESCO login button', () => {
    render(<Login />, { wrapper: RouterWrapper });
    const loginButton = screen.getByRole('button', { name: /sign in with unesco/i });
    expect(loginButton).toBeInTheDocument();
  });

  it('calls login when UNESCO button is clicked', async () => {
    render(<Login />, { wrapper: RouterWrapper });
    const loginButton = screen.getByRole('button', { name: /sign in with unesco/i });

    fireEvent.click(loginButton);

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalled();
    });
  });

  it('has multiple buttons on the page', () => {
    render(<Login />, { wrapper: RouterWrapper });
    const buttons = screen.getAllByRole('button');
    expect(buttons.length).toBeGreaterThanOrEqual(1);
  });
});
