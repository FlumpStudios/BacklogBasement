import { useEffect } from 'react';
import './Modal.css';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  dismissible?: boolean;
}

export function Modal({ isOpen, onClose, title, children, dismissible = true }: ModalProps) {
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && dismissible) {
        onClose();
      }
    };

    if (isOpen) {
      document.addEventListener('keydown', handleEscape);
      document.body.style.overflow = 'hidden';
    }

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = '';
    };
  }, [isOpen, onClose, dismissible]);

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={dismissible ? onClose : undefined}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2 className="modal-title">{title}</h2>
          {dismissible && (
            <button onClick={onClose} className="modal-close" aria-label="Close">
              ✕
            </button>
          )}
        </div>
        <div className="modal-content">{children}</div>
      </div>
    </div>
  );
}
