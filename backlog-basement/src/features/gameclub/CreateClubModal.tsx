import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useCreateClub } from '../../hooks';
import './GameClub.css';

interface CreateClubModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCreated?: (clubId: string) => void;
}

function validateSocialLink(value: string, type: 'discord' | 'whatsapp' | 'reddit' | 'youtube'): string | null {
  if (!value) return null;
  if (type === 'discord' && !value.startsWith('https://discord.gg/') && !value.startsWith('https://discord.com/invite/'))
    return 'Must start with https://discord.gg/ or https://discord.com/invite/';
  if (type === 'whatsapp' && !value.startsWith('https://chat.whatsapp.com/'))
    return 'Must start with https://chat.whatsapp.com/';
  if (type === 'reddit' && !value.startsWith('https://www.reddit.com/r/') && !value.startsWith('https://reddit.com/r/'))
    return 'Must start with https://reddit.com/r/';
  if (type === 'youtube' && !value.startsWith('https://www.youtube.com/') && !value.startsWith('https://youtu.be/'))
    return 'Must start with https://www.youtube.com/ or https://youtu.be/';
  return null;
}

export function CreateClubModal({ isOpen, onClose, onCreated }: CreateClubModalProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(false);
  const [discordLink, setDiscordLink] = useState('');
  const [whatsAppLink, setWhatsAppLink] = useState('');
  const [redditLink, setRedditLink] = useState('');
  const [youTubeLink, setYouTubeLink] = useState('');

  const createClub = useCreateClub();
  const { showToast } = useToast();

  const discordError = validateSocialLink(discordLink, 'discord');
  const whatsAppError = validateSocialLink(whatsAppLink, 'whatsapp');
  const redditError = validateSocialLink(redditLink, 'reddit');
  const youTubeError = validateSocialLink(youTubeLink, 'youtube');
  const hasErrors = !!discordError || !!whatsAppError || !!redditError || !!youTubeError;

  const handleCreate = async () => {
    if (!name.trim() || hasErrors) return;

    try {
      const { data: club } = await createClub.mutateAsync({
        name: name.trim(),
        description: description.trim() || undefined,
        isPublic,
        discordLink: discordLink.trim() || undefined,
        whatsAppLink: whatsAppLink.trim() || undefined,
        redditLink: redditLink.trim() || undefined,
        youTubeLink: youTubeLink.trim() || undefined,
      });
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
    setIsPublic(false);
    setDiscordLink('');
    setWhatsAppLink('');
    setRedditLink('');
    setYouTubeLink('');
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

        <div className="club-form-field">
          <label htmlFor="club-discord">Discord invite link (optional)</label>
          <input
            id="club-discord"
            type="url"
            value={discordLink}
            onChange={(e) => setDiscordLink(e.target.value)}
            placeholder="https://discord.gg/..."
          />
          {discordError && <p className="club-form-error">{discordError}</p>}
        </div>

        <div className="club-form-field">
          <label htmlFor="club-whatsapp">WhatsApp group link (optional)</label>
          <input
            id="club-whatsapp"
            type="url"
            value={whatsAppLink}
            onChange={(e) => setWhatsAppLink(e.target.value)}
            placeholder="https://chat.whatsapp.com/..."
          />
          {whatsAppError && <p className="club-form-error">{whatsAppError}</p>}
        </div>

        <div className="club-form-field">
          <label htmlFor="club-reddit">Reddit community link (optional)</label>
          <input
            id="club-reddit"
            type="url"
            value={redditLink}
            onChange={(e) => setRedditLink(e.target.value)}
            placeholder="https://reddit.com/r/..."
          />
          {redditError && <p className="club-form-error">{redditError}</p>}
        </div>

        <div className="club-form-field">
          <label htmlFor="club-youtube">YouTube link (optional)</label>
          <input
            id="club-youtube"
            type="url"
            value={youTubeLink}
            onChange={(e) => setYouTubeLink(e.target.value)}
            placeholder="https://www.youtube.com/..."
          />
          {youTubeError && <p className="club-form-error">{youTubeError}</p>}
        </div>

        <button
          className="btn btn-primary"
          onClick={handleCreate}
          disabled={!name.trim() || hasErrors || createClub.isPending}
        >
          {createClub.isPending ? 'Creating...' : 'Create Club'}
        </button>
      </div>
    </Modal>
  );
}
