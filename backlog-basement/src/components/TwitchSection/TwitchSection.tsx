import { useState } from 'react';
import { useAuth } from '../../auth';
import { authApi } from '../../api';
import { useQueryClient } from '@tanstack/react-query';
import { AUTH_QUERY_KEY } from '../../auth/AuthContext';
import { useTwitchImport } from '../../hooks';
import { useToast } from '../Toast';
import { Modal } from '../Modal/Modal';
import { TwitchImportResultDto } from '../../types';
import './TwitchSection.css';

export function TwitchSection() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const importMutation = useTwitchImport();

  const [unlinking, setUnlinking] = useState(false);
  const [isOpen, setIsOpen] = useState(!user?.hasTwitchLinked);
  const [showImportModal, setShowImportModal] = useState(false);
  const [importResult, setImportResult] = useState<TwitchImportResultDto | null>(null);

  const handleLink = () => {
    window.location.href = authApi.getTwitchLinkUrl();
  };

  const handleUnlink = async () => {
    if (!window.confirm('Are you sure you want to unlink your Twitch account?')) return;
    setUnlinking(true);
    try {
      await authApi.unlinkTwitch();
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
      showToast('Twitch account unlinked', 'success');
    } catch {
      showToast('Failed to unlink Twitch account', 'error');
    } finally {
      setUnlinking(false);
    }
  };

  const handleImport = async () => {
    setImportResult(null);
    try {
      const result = await importMutation.mutateAsync();
      setImportResult(result);
    } catch {
      showToast('Import failed — please try again', 'error');
    }
  };

  const handleCloseImportModal = () => {
    setShowImportModal(false);
    setImportResult(null);
  };

  const formatStreamTime = (minutes: number) => {
    if (minutes < 60) return `${minutes}m`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
  };

  return (
    <div className="twitch-section">
      <button className="twitch-header" onClick={() => setIsOpen(!isOpen)}>
        <div className="twitch-header-left">
          <h3>Twitch Integration</h3>
          {user?.hasTwitchLinked ? (
            <span className="twitch-header-status twitch-header-linked">
              <span className="twitch-status-icon">&#x2714;</span>
              Linked
            </span>
          ) : (
            <span className="twitch-header-status twitch-header-not-linked">
              Not linked
            </span>
          )}
        </div>
        <span className={`twitch-chevron ${isOpen ? 'open' : ''}`}>&#9662;</span>
      </button>

      <div className={`twitch-body ${isOpen ? 'open' : ''}`}>
        <div className="twitch-body-inner">
          {user?.hasTwitchLinked ? (
            <div className="twitch-linked">
              <p>Your Twitch account is linked. Import your stream history or unlink below.</p>
              <div className="twitch-actions">
                <button
                  className="btn btn-twitch"
                  onClick={() => setShowImportModal(true)}
                >
                  <svg viewBox="0 0 24 24" width="16" height="16">
                    <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714z"/>
                  </svg>
                  Import Stream History
                </button>
                <button
                  className="btn btn-secondary"
                  onClick={handleUnlink}
                  disabled={unlinking}
                >
                  {unlinking ? 'Unlinking...' : 'Unlink Twitch'}
                </button>
              </div>
            </div>
          ) : (
            <div className="twitch-not-linked">
              <p>Link your Twitch account to import your stream history and show live status on your profile.</p>
              <button className="btn btn-twitch" onClick={handleLink}>
                <svg viewBox="0 0 24 24" width="18" height="18">
                  <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714z"/>
                </svg>
                Link Twitch Account
              </button>
            </div>
          )}
        </div>
      </div>

      <Modal
        isOpen={showImportModal}
        onClose={handleCloseImportModal}
        title="Import Twitch Stream History"
      >
        {!importResult ? (
          <div className="import-options">
            <p>
              This will scan your past Twitch broadcasts and add every game you've streamed to your collection,
              with streaming time recorded as playtime.
            </p>
            <p className="import-note">
              Up to 500 past broadcasts will be scanned. Games already in your collection will be skipped. Note: Twitch only retains broadcast history for a limited time, so older streams may not appear (60 days for partners, 14 days for regular streamers).
            </p>
            <div className="modal-actions">
              <button
                className="btn btn-primary"
                onClick={handleImport}
                disabled={importMutation.isPending}
              >
                {importMutation.isPending ? 'Importing...' : 'Start Import'}
              </button>
              <button className="btn btn-secondary" onClick={handleCloseImportModal}>
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <div className="import-results">
            <div className="import-summary">
              <h4>Import Complete</h4>
              <div className="import-stats">
                <div className="stat">
                  <span className="stat-value">{importResult.total}</span>
                  <span className="stat-label">Games Found</span>
                </div>
                <div className="stat stat-success">
                  <span className="stat-value">{importResult.imported}</span>
                  <span className="stat-label">Imported</span>
                </div>
                <div className="stat stat-skipped">
                  <span className="stat-value">{importResult.skipped}</span>
                  <span className="stat-label">Already Owned</span>
                </div>
              </div>
            </div>

            {importResult.importedGames.length > 0 && (
              <div className="import-list">
                <h5>Imported ({importResult.importedGames.length})</h5>
                <ul>
                  {importResult.importedGames.slice(0, 10).map((game) => (
                    <li key={game.igdbId}>
                      {game.name}
                      {game.streamedMinutes > 0 && (
                        <span className="playtime">{formatStreamTime(game.streamedMinutes)} streamed</span>
                      )}
                    </li>
                  ))}
                  {importResult.importedGames.length > 10 && (
                    <li className="more">...and {importResult.importedGames.length - 10} more</li>
                  )}
                </ul>
              </div>
            )}

            <div className="modal-actions">
              <button className="btn btn-primary" onClick={handleCloseImportModal}>Done</button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
