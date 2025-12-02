import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Button } from '../components/ui/Elements';

// Declare global variables for Jest to fix missing type errors
declare var describe: any;
declare var test: any;
declare var expect: any;
declare var jest: any;

describe('Button Component', () => {
  test('renders button with text', () => {
    render(<Button>Click Me</Button>);
    expect(screen.getByText('Click Me')).toBeInTheDocument();
  });

  test('shows loading spinner when isLoading is true', () => {
    render(<Button isLoading>Submit</Button>);
    // Tailwind spinner implementation creates an svg
    expect(document.querySelector('svg')).toBeInTheDocument(); 
    expect(screen.getByRole('button')).toBeDisabled();
  });

  test('calls onClick handler when clicked', () => {
    const handleClick = jest.fn();
    render(<Button onClick={handleClick}>Action</Button>);
    fireEvent.click(screen.getByText('Action'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });
});