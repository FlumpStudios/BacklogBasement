import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useCreateClub } from '../../hooks';
import './GameClub.css';

interface CreateClubModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreated?: (clubId: string) => void;
}

export function CreateClubModal({ isOpen, onClose, onCreated }: CreateClubModalProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(true);

  const createClub = useCreateClub();
  const { showToast } = useToast();

  const handleCreate = async () => {
    if (!name.trim()) return;

    try {
      const club = await createClub.mutateAsync({ name: name.trim(), description: description.trim() || undefined, isPublic });
      showToast(`Club "${club.name}" created!`, 'success');
      handleClose();
      onCreated?.(club.id);
    } catch {
      showToast('Failed to create club', 'error');
    }
  };

  const handleClose = () => {
    setName('');
    setDescription('');
    setIsPublic(true);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Create Game Club">
      <div className="club-modal-form">
        <div className="club-form-field">
          <label htmlFor="club-name">Club Name</label>
          <input
            id="club-name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Enter club name..."
            maxLength={100}
          />
        </div>

        <div className="club-form-field">
          <label htmlFor="club-description">Description (optional)</label>
          <textarea
            id="club-description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="What's this club about?"
            maxLength={500}
            rows={3}
          />
        </div>

        <div className="club-form-field club-form-toggle">
          <label>
            <input
              type="checkbox"
              checked={isPublic}
              onChange={(e) => setIsPublic(e.target.checked)}
            />
            <span>Public club (anyone can join)</span>
          </label>
          {!isPublic && (
            <p className="club-form-hint">Invite-only — members must be invited by an admin.</p>
          )}
        </div>

        <button
          className="btn btn-primary"
          onClick={handleCreate}
          disabled={!name.trim() || createClub.isPending}
        >
          {createClub.isPending ? 'Creating...' : 'Create Club'}
        </button>
      </div>
    </Modal>
  );
}
