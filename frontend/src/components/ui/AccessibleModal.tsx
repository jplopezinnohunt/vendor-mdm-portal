import React, { useEffect, useRef, useCallback, ReactNode, useId } from 'react';
import { X } from 'lucide-react';

/**
 * AccessibleModal - WCAG 2.1 AA Compliant Modal Component
 *
 * Accessibility Features (Brain v1.2.0 Pattern 29 Compliance):
 * - role="dialog" with aria-modal="true"
 * - aria-labelledby for modal title
 * - aria-describedby for modal description (optional)
 * - Focus trap within modal
 * - Escape key closes modal
 * - Focus returns to trigger element on close
 * - Prevents body scroll when open
 */

interface AccessibleModalProps {
  /** Controls modal visibility */
  isOpen: boolean;
  /** Callback when modal should close */
  onClose: () => void;
  /** Modal title displayed in header */
  title: string;
  /** Optional description for aria-describedby */
  description?: string;
  /** Modal content */
  children: ReactNode;
  /** Optional header icon */
  headerIcon?: ReactNode;
  /** Header background class (default: gradient purple) */
  headerClassName?: string;
  /** Max width class (default: max-w-3xl) */
  maxWidthClassName?: string;
  /** Whether to show close button (default: true) */
  showCloseButton?: boolean;
  /** Footer content (buttons, etc.) */
  footer?: ReactNode;
  /** Z-index class (default: z-50) */
  zIndexClassName?: string;
}

export const AccessibleModal: React.FC<AccessibleModalProps> = ({
  isOpen,
  onClose,
  title,
  description,
  children,
  headerIcon,
  headerClassName = 'bg-gradient-to-r from-purple-600 to-purple-700',
  maxWidthClassName = 'max-w-3xl',
  showCloseButton = true,
  footer,
  zIndexClassName = 'z-50',
}) => {
  const modalRef = useRef<HTMLDivElement>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const titleId = useId();
  const descriptionId = useId();

  // Store the element that had focus before modal opened
  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement as HTMLElement;
    }
  }, [isOpen]);

  // Focus trap and keyboard handling
  useEffect(() => {
    if (!isOpen || !modalRef.current) return;

    const modal = modalRef.current;

    // Get all focusable elements within modal
    const getFocusableElements = () => {
      return modal.querySelectorAll<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
    };

    // Focus first focusable element
    const focusableElements = getFocusableElements();
    if (focusableElements.length > 0) {
      focusableElements[0].focus();
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      // Close on Escape
      if (event.key === 'Escape') {
        event.preventDefault();
        onClose();
        return;
      }

      // Focus trap on Tab
      if (event.key === 'Tab') {
        const focusable = getFocusableElements();
        if (focusable.length === 0) return;

        const firstElement = focusable[0];
        const lastElement = focusable[focusable.length - 1];

        if (event.shiftKey) {
          // Shift + Tab
          if (document.activeElement === firstElement) {
            event.preventDefault();
            lastElement.focus();
          }
        } else {
          // Tab
          if (document.activeElement === lastElement) {
            event.preventDefault();
            firstElement.focus();
          }
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, onClose]);

  // Restore focus when modal closes
  useEffect(() => {
    if (!isOpen && previousFocusRef.current) {
      previousFocusRef.current.focus();
      previousFocusRef.current = null;
    }
  }, [isOpen]);

  // Prevent body scroll when modal is open
  useEffect(() => {
    if (isOpen) {
      const originalOverflow = document.body.style.overflow;
      document.body.style.overflow = 'hidden';
      return () => {
        document.body.style.overflow = originalOverflow;
      };
    }
  }, [isOpen]);

  // Handle backdrop click
  const handleBackdropClick = useCallback(
    (event: React.MouseEvent<HTMLDivElement>) => {
      if (event.target === event.currentTarget) {
        onClose();
      }
    },
    [onClose]
  );

  if (!isOpen) return null;

  return (
    <div
      className={`fixed inset-0 ${zIndexClassName} flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-300`}
      onClick={handleBackdropClick}
      data-testid="modal-backdrop"
    >
      <div
        ref={modalRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={description ? descriptionId : undefined}
        className={`bg-white rounded-xl shadow-2xl w-full ${maxWidthClassName} max-h-[90vh] overflow-hidden animate-in zoom-in-95 duration-300`}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className={`${headerClassName} text-white px-6 py-4 flex items-center justify-between`}>
          <div className="flex items-center gap-3">
            {headerIcon}
            <h2 id={titleId} className="text-xl font-semibold">
              {title}
            </h2>
          </div>
          {showCloseButton && (
            <button
              type="button"
              onClick={onClose}
              className="p-1 hover:bg-white/20 rounded-full transition-colors"
              aria-label="Close modal"
            >
              <X className="w-6 h-6" />
            </button>
          )}
        </div>

        {/* Description (for screen readers) */}
        {description && (
          <div id={descriptionId} className="sr-only">
            {description}
          </div>
        )}

        {/* Content */}
        <div className="p-6 overflow-y-auto max-h-[calc(90vh-140px)]">
          {children}
        </div>

        {/* Footer */}
        {footer && (
          <div className="px-6 py-4 bg-gray-50 border-t border-gray-200 flex justify-end gap-3">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
};

/**
 * Hook to manage modal state with focus restoration
 */
export const useModal = (initialState = false) => {
  const [isOpen, setIsOpen] = React.useState(initialState);

  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);
  const toggle = useCallback(() => setIsOpen((prev) => !prev), []);

  return { isOpen, open, close, toggle };
};

export default AccessibleModal;
