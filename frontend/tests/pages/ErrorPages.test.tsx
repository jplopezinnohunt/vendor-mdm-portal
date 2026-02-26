import React from 'react';
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import '@testing-library/jest-dom';
import { NotFound } from '../../src/pages/NotFound';
import { ServerError } from '../../src/pages/ServerError';
import { Unauthorized } from '../../src/pages/Unauthorized';

// Mock AuthContext
vi.mock('../../src/context/AuthContext', () => ({
  useAuth: () => ({
    user: null,
    logout: vi.fn(),
    isAuthenticated: false,
    login: vi.fn(),
    isLoading: false,
  }),
  AuthProvider: ({ children }: { children: React.ReactNode }) => children,
}));

// Wrapper for components that use React Router
const RouterWrapper = ({ children }: { children: React.ReactNode }) => (
  <BrowserRouter>{children}</BrowserRouter>
);

describe('NotFound Page', () => {
  it('renders page not found message', () => {
    render(<NotFound />, { wrapper: RouterWrapper });
    expect(screen.getByText('Page Not Found')).toBeInTheDocument();
  });

  it('renders helpful message about broken links', () => {
    render(<NotFound />, { wrapper: RouterWrapper });
    expect(screen.getByText(/link, it may be broken/i)).toBeInTheDocument();
  });

  it('renders link to home page', () => {
    render(<NotFound />, { wrapper: RouterWrapper });
    expect(screen.getByRole('link', { name: /go to home/i })).toHaveAttribute('href', '/');
  });
});

describe('ServerError Page', () => {
  it('renders server error message', () => {
    render(<ServerError />, { wrapper: RouterWrapper });
    expect(screen.getByText(/Server Error/i)).toBeInTheDocument();
  });

  it('renders explanation message', () => {
    render(<ServerError />, { wrapper: RouterWrapper });
    expect(screen.getByText(/Something went wrong on our end/i)).toBeInTheDocument();
  });

  it('renders try again and home buttons', () => {
    render(<ServerError />, { wrapper: RouterWrapper });
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /return home/i })).toBeInTheDocument();
  });
});

describe('Unauthorized Page', () => {
  it('renders access denied message', () => {
    render(<Unauthorized />, { wrapper: RouterWrapper });
    expect(screen.getByText(/Access Denied/i)).toBeInTheDocument();
  });

  it('renders 401 status code', () => {
    render(<Unauthorized />, { wrapper: RouterWrapper });
    expect(screen.getByText('401')).toBeInTheDocument();
  });

  it('renders dashboard and login buttons', () => {
    render(<Unauthorized />, { wrapper: RouterWrapper });
    expect(screen.getByRole('button', { name: /go to dashboard/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /login as different user/i })).toBeInTheDocument();
  });

  it('renders contact support message', () => {
    render(<Unauthorized />, { wrapper: RouterWrapper });
    expect(screen.getByText(/contact IT Support/i)).toBeInTheDocument();
  });
});
