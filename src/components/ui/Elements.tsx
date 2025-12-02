import React from 'react';
import { ChangeRequestStatus } from '../../types';

// Button Component
interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
}

export const Button: React.FC<ButtonProps> = ({ 
  children, 
  variant = 'primary', 
  size = 'md', 
  isLoading, 
  className = '', 
  disabled,
  ...props 
}) => {
  const baseStyles = "inline-flex items-center justify-center rounded-md font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:pointer-events-none";
  
  const variants = {
    primary: "bg-brand-600 text-white hover:bg-brand-700 focus:ring-brand-500",
    secondary: "bg-gray-100 text-gray-900 hover:bg-gray-200 focus:ring-gray-500",
    outline: "border border-gray-300 bg-transparent text-gray-700 hover:bg-gray-50 focus:ring-brand-500",
    danger: "bg-red-600 text-white hover:bg-red-700 focus:ring-red-500",
  };

  const sizes = {
    sm: "h-8 px-3 text-xs",
    md: "h-10 px-4 py-2 text-sm",
    lg: "h-12 px-6 text-base",
  };

  return (
    <button 
      className={`${baseStyles} ${variants[variant]} ${sizes[size]} ${className}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <svg className="mr-2 h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
      ) : null}
      {children}
    </button>
  );
};

// Card Component
export const Card: React.FC<{ children: React.ReactNode; className?: string; title?: string }> = ({ children, className = '', title }) => (
  <div className={`overflow-hidden rounded-lg bg-white shadow ${className}`}>
    {title && (
      <div className="border-b border-gray-200 px-4 py-4 sm:px-6">
        <h3 className="text-lg font-medium leading-6 text-gray-900">{title}</h3>
      </div>
    )}
    <div className="px-4 py-5 sm:p-6">{children}</div>
  </div>
);

// Input Component
interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(({ label, error, className = '', ...props }, ref) => (
  <div className="w-full">
    <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
    <input
      ref={ref}
      className={`block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm ${
        error ? 'border-red-300 focus:border-red-500 focus:ring-red-500' : ''
      } ${className}`}
      {...props}
    />
    {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
  </div>
));
Input.displayName = 'Input';

// Status Badge
export const StatusBadge: React.FC<{ status: ChangeRequestStatus | string }> = ({ status }) => {
  const styles: Record<string, string> = {
    [ChangeRequestStatus.New]: 'bg-blue-100 text-blue-800',
    [ChangeRequestStatus.InReview]: 'bg-yellow-100 text-yellow-800',
    [ChangeRequestStatus.Approved]: 'bg-green-100 text-green-800',
    [ChangeRequestStatus.Applied]: 'bg-green-100 text-green-800',
    [ChangeRequestStatus.Rejected]: 'bg-red-100 text-red-800',
    [ChangeRequestStatus.Error]: 'bg-red-100 text-red-800',
  };

  const style = styles[status] || 'bg-gray-100 text-gray-800';

  return (
    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${style}`}>
      {status.replace('_', ' ')}
    </span>
  );
};