import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AccessibleModal, useModal } from '../../src/components/ui/AccessibleModal';
import { renderHook, act } from '@testing-library/react';

describe('AccessibleModal', () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Rendering', () => {
    it('renders nothing when isOpen is false', () => {
      const { container } = render(
        <AccessibleModal isOpen={false} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(container.innerHTML).toBe('');
    });

    it('renders modal when isOpen is true', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Modal Content</p>
        </AccessibleModal>
      );

      expect(screen.getByRole('dialog')).toBeInTheDocument();
      expect(screen.getByText('Test Modal')).toBeInTheDocument();
      expect(screen.getByText('Modal Content')).toBeInTheDocument();
    });

    it('renders close button by default', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.getByLabelText('Close modal')).toBeInTheDocument();
    });

    it('hides close button when showCloseButton is false', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal" showCloseButton={false}>
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.queryByLabelText('Close modal')).not.toBeInTheDocument();
    });

    it('renders footer when provided', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          footer={<button>Save</button>}
        >
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.getByText('Save')).toBeInTheDocument();
    });

    it('renders header icon when provided', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          headerIcon={<span data-testid="custom-icon">Icon</span>}
        >
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.getByTestId('custom-icon')).toBeInTheDocument();
    });
  });

  describe('Accessibility Attributes', () => {
    it('has role="dialog"', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    it('has aria-modal="true"', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(screen.getByRole('dialog')).toHaveAttribute('aria-modal', 'true');
    });

    it('has aria-labelledby pointing to title', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Accessible Title">
          <p>Content</p>
        </AccessibleModal>
      );

      const dialog = screen.getByRole('dialog');
      const labelledBy = dialog.getAttribute('aria-labelledby');
      expect(labelledBy).toBeTruthy();

      const titleElement = document.getElementById(labelledBy!);
      expect(titleElement?.textContent).toBe('Accessible Title');
    });

    it('has aria-describedby when description is provided', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          description="This is a description for screen readers"
        >
          <p>Content</p>
        </AccessibleModal>
      );

      const dialog = screen.getByRole('dialog');
      const describedBy = dialog.getAttribute('aria-describedby');
      expect(describedBy).toBeTruthy();

      const descriptionElement = document.getElementById(describedBy!);
      expect(descriptionElement?.textContent).toBe('This is a description for screen readers');
    });

    it('does not have aria-describedby when no description', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      const dialog = screen.getByRole('dialog');
      expect(dialog.getAttribute('aria-describedby')).toBeNull();
    });

    it('close button has accessible label', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      const closeButton = screen.getByLabelText('Close modal');
      expect(closeButton).toHaveAttribute('type', 'button');
    });
  });

  describe('Keyboard Interactions', () => {
    it('closes on Escape key press', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      fireEvent.keyDown(document, { key: 'Escape' });

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('close button closes modal on click', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      fireEvent.click(screen.getByLabelText('Close modal'));

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('closes when clicking backdrop', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      // Click on the backdrop
      const backdrop = screen.getByTestId('modal-backdrop');
      fireEvent.click(backdrop);

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('does not close when clicking modal content', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      fireEvent.click(screen.getByRole('dialog'));

      expect(mockOnClose).not.toHaveBeenCalled();
    });
  });

  describe('Focus Management', () => {
    it('focuses first focusable element when opened', async () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <button>First Button</button>
          <button>Second Button</button>
        </AccessibleModal>
      );

      await waitFor(() => {
        // The close button is the first focusable element
        expect(document.activeElement).toHaveAttribute('aria-label', 'Close modal');
      });
    });
  });

  describe('Body Scroll Prevention', () => {
    it('prevents body scroll when open', () => {
      render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(document.body.style.overflow).toBe('hidden');
    });

    it('restores body scroll when closed', () => {
      const { rerender } = render(
        <AccessibleModal isOpen={true} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      rerender(
        <AccessibleModal isOpen={false} onClose={mockOnClose} title="Test Modal">
          <p>Content</p>
        </AccessibleModal>
      );

      expect(document.body.style.overflow).not.toBe('hidden');
    });
  });

  describe('Custom Styling', () => {
    it('applies custom header className', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          headerClassName="bg-blue-500"
        >
          <p>Content</p>
        </AccessibleModal>
      );

      // The title is inside a nested div structure, we need to go up to the header
      const titleElement = screen.getByText('Test Modal');
      const header = titleElement.closest('div')?.parentElement;
      expect(header).toHaveClass('bg-blue-500');
    });

    it('applies custom maxWidth className', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          maxWidthClassName="max-w-5xl"
        >
          <p>Content</p>
        </AccessibleModal>
      );

      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveClass('max-w-5xl');
    });

    it('applies custom zIndex className', () => {
      render(
        <AccessibleModal
          isOpen={true}
          onClose={mockOnClose}
          title="Test Modal"
          zIndexClassName="z-[60]"
        >
          <p>Content</p>
        </AccessibleModal>
      );

      const backdrop = screen.getByTestId('modal-backdrop');
      expect(backdrop).toHaveClass('z-[60]');
    });
  });
});

describe('useModal hook', () => {
  it('initializes with default closed state', () => {
    const { result } = renderHook(() => useModal());

    expect(result.current.isOpen).toBe(false);
  });

  it('initializes with custom initial state', () => {
    const { result } = renderHook(() => useModal(true));

    expect(result.current.isOpen).toBe(true);
  });

  it('opens modal with open()', () => {
    const { result } = renderHook(() => useModal());

    act(() => {
      result.current.open();
    });

    expect(result.current.isOpen).toBe(true);
  });

  it('closes modal with close()', () => {
    const { result } = renderHook(() => useModal(true));

    act(() => {
      result.current.close();
    });

    expect(result.current.isOpen).toBe(false);
  });

  it('toggles modal state with toggle()', () => {
    const { result } = renderHook(() => useModal());

    act(() => {
      result.current.toggle();
    });
    expect(result.current.isOpen).toBe(true);

    act(() => {
      result.current.toggle();
    });
    expect(result.current.isOpen).toBe(false);
  });
});
