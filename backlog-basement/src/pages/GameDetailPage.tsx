import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  useGameDetails,
  useCollection,
  useAddToCollection,
  useRemoveFromCollection,
  usePlaySessions,
  useAddPlaySession,
  useDeletePlaySession,
  useSyncSteamPlaytime,
} from '../hooks';
import { PlaySessionForm, PlaySessionList } from '../features/playtime';
import { Modal, useToast } from '../components';
import { formatPlaytime, formatDate } from '../utils';
import './GameDetailPage.css';

export function GameDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { showToast } = useToast();

  const { data: game, isLoading: gameLoading } = useGameDetails(id!);
  const { data: collection } = useCollection();
  const { data: playSessions } = usePlaySessions(id!);

  const addToCollection = useAddToCollection();
  const removeFromCollection = useRemoveFromCollection();
  const addPlaySession = useAddPlaySession(id!);
  const deletePlaySession = useDeletePlaySession(id!);
  const syncSteamPlaytime = useSyncSteamPlaytime(id!);

  const [showPlaySessionModal, setShowPlaySessionModal] = useState(false);

  const isInCollection = collection?.some((item) => item.gameId === id);
  const collectionItem = collection?.find((item) => item.gameId === id);

  const handleAddToCollection = async () => {
    if (!game) return;
    try {
      await addToCollection.mutateAsync(id!);
      showToast(`Added "${game.name}" to your collection!`, 'success');
    } catch {
      showToast('Failed to add game to collection', 'error');
    }
  };

  const handleRemoveFromCollection = async () => {
    if (!game) return;
    try {
      await removeFromCollection.mutateAsync(id!);
      showToast(`Removed "${game.name}" from your collection`, 'success');
    } catch {
      showToast('Failed to remove game', 'error');
    }
  };

  const handleAddPlaySession = async (session: { durationMinutes: number; datePlayed: string }) => {
    try {
      await addPlaySession.mutateAsync(session);
      setShowPlaySessionModal(false);
      showToast('Play session logged!', 'success');
    } catch {
      showToast('Failed to log play session', 'error');
    }
  };

  const handleDeletePlaySession = async (sessionId: string) => {
    try {
      await deletePlaySession.mutateAsync(sessionId);
      showToast('Play session deleted', 'success');
    } catch {
      showToast('Failed to delete play session', 'error');
    }
  };

  const handleSyncSteamPlaytime = async () => {
    try {
      const result = await syncSteamPlaytime.mutateAsync();
      showToast(`Synced ${formatPlaytime(result.playtimeMinutes)} from Steam`, 'success');
    } catch {
      showToast('Failed to sync playtime from Steam', 'error');
    }
  };

  if (gameLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading game details...</p>
      </div>
    );
  }

  if (!game) {
    return (
      <div className="game-detail-page">
        <div className="not-found">
          <h1>Game not found</h1>
          <button onClick={() => navigate(-1)} className="btn btn-secondary">
            Go Back
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="game-detail-page">
      <button onClick={() => navigate(-1)} className="back-button">
        ← Back
      </button>

      <div className="game-detail-content">
        <div className="game-detail-cover">
          {game.coverUrl ? (
            <img src={game.coverUrl} alt={game.name} className="cover-image" />
          ) : (
            <div className="cover-placeholder">🎮</div>
          )}
        </div>

        <div className="game-detail-info">
          <h1 className="game-title">{game.name}</h1>

          {game.releaseDate && (
            <p className="game-release">
              Released: {formatDate(game.releaseDate)}
            </p>
          )}

          {isInCollection && collectionItem && (
            <div className="game-playtime">
              <span className="playtime-icon">⏱️</span>
              <span className="playtime-value">
                {formatPlaytime(collectionItem.totalPlayTimeMinutes)}
              </span>
              <span className="playtime-label">total playtime</span>
            </div>
          )}

          {game.summary && (
            <div className="game-summary">
              <h2>About</h2>
              <p>{game.summary}</p>
            </div>
          )}

          <div className="game-actions">
            {isInCollection ? (
              <>
                <button
                  onClick={() => setShowPlaySessionModal(true)}
                  className="btn btn-primary"
                >
                  Log Play Session
                </button>
                <button
                  onClick={handleRemoveFromCollection}
                  className="btn btn-danger"
                  disabled={removeFromCollection.isPending}
                >
                  Remove from Collection
                </button>
              </>
            ) : (
              <button
                onClick={handleAddToCollection}
                className="btn btn-primary"
                disabled={addToCollection.isPending}
              >
                + Add to Collection
              </button>
            )}
          </div>

          {isInCollection && playSessions && (
            <div className="game-sessions">
              <div className="sessions-header">
                <h2>Play Sessions</h2>
                {collectionItem?.source === 'steam' && (
                  <button
                    onClick={handleSyncSteamPlaytime}
                    className="btn btn-secondary btn-sm"
                    disabled={syncSteamPlaytime.isPending}
                  >
                    {syncSteamPlaytime.isPending ? 'Syncing...' : 'Sync from Steam'}
                  </button>
                )}
              </div>
              <PlaySessionList
                sessions={playSessions}
                onDelete={handleDeletePlaySession}
                isDeleting={deletePlaySession.isPending}
              />
            </div>
          )}
        </div>
      </div>

      <Modal
        isOpen={showPlaySessionModal}
        onClose={() => setShowPlaySessionModal(false)}
        title="Log Play Session"
      >
        <PlaySessionForm
          onSubmit={handleAddPlaySession}
          isLoading={addPlaySession.isPending}
        />
      </Modal>
    </div>
  );
}
