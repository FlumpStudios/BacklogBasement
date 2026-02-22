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
  useUpdateGameStatus,
  useClubScoreForGame,
} from '../hooks';
import { GameStatus } from '../types';
import { PlaySessionForm, PlaySessionList } from '../features/playtime';
import { SuggestGameModal } from '../features/suggestions';
import { GameClubReviewsSection } from '../features/gameclub';
import { Modal, useToast } from '../components';
import { useAuth } from '../auth';
import { formatPlaytime, formatDate } from '../utils';
import './GameDetailPage.css';

export function GameDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { showToast } = useToast();

  const { data: game, isLoading: gameLoading } = useGameDetails(id!);
  const { data: collection } = useCollection();
  const { data: playSessions } = usePlaySessions(id!);
  const { data: clubScore } = useClubScoreForGame(id);

  const addToCollection = useAddToCollection();
  const removeFromCollection = useRemoveFromCollection();
  const addPlaySession = useAddPlaySession(id!);
  const deletePlaySession = useDeletePlaySession(id!);
  const syncSteamPlaytime = useSyncSteamPlaytime(id!);
  const updateGameStatus = useUpdateGameStatus();

  const { isAuthenticated } = useAuth();
  const [showPlaySessionModal, setShowPlaySessionModal] = useState(false);
  const [showSuggestModal, setShowSuggestModal] = useState(false);

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
      showToast('Failed to sync playtime from Steam. Is this game in your Steam library?', 'error');
    }
  };

  const handleUpdateStatus = async (status: GameStatus) => {
    try {
      await updateGameStatus.mutateAsync({ gameId: id!, status });
      const statusLabels: Record<string, string> = {
        backlog: 'Added to Backlog',
        playing: 'Marked as Playing',
        completed: 'Marked as Completed',
      };
      showToast(status ? statusLabels[status] : 'Status cleared', 'success');
    } catch {
      showToast('Failed to update status', 'error');
    }
  };

  const getStatusLabel = (status: GameStatus) => {
    switch (status) {
      case 'backlog': return 'Backlog';
      case 'playing': return 'Playing';
      case 'completed': return 'Completed';
      default: return null;
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

          {game.criticScore != null && (
            <div className={`game-critic-score ${game.criticScore >= 75 ? 'critic-green' : game.criticScore >= 50 ? 'critic-yellow' : 'critic-red'}`}>
              <span className="critic-score-value">{game.criticScore}</span>
              <span className="critic-score-label">Critic Score</span>
            </div>
          )}

          {clubScore && (
            <div className="game-club-score">
              <span className="club-score-value">{clubScore.averageScore.toFixed(1)}</span>
              <div className="club-score-info">
                <span className="club-score-label">Club Score</span>
                <span className="club-score-meta">
                  {clubScore.reviewCount} {clubScore.reviewCount === 1 ? 'review' : 'reviews'} across {clubScore.roundCount} {clubScore.roundCount === 1 ? 'round' : 'rounds'}
                </span>
              </div>
            </div>
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
            {isAuthenticated && (
              <button
                onClick={() => setShowSuggestModal(true)}
                className="btn btn-secondary"
              >
                Suggest to Friend
              </button>
            )}
          </div>

          {isInCollection && (
            <div className="game-status-section">
              <h3>Status</h3>
              {collectionItem?.status && (
                <span className={`status-badge status-${collectionItem.status}`}>
                  {getStatusLabel(collectionItem.status)}
                </span>
              )}
              <div className="status-actions">
                {!collectionItem?.status && (
                  <button
                    onClick={() => handleUpdateStatus('backlog')}
                    className="btn btn-secondary btn-sm"
                    disabled={updateGameStatus.isPending}
                  >
                    Add to Backlog
                  </button>
                )}
                {collectionItem?.status === 'backlog' && (
                  <>
                    <button
                      onClick={() => handleUpdateStatus('playing')}
                      className="btn btn-primary btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Start Playing
                    </button>
                    <button
                      onClick={() => handleUpdateStatus('completed')}
                      className="btn btn-success btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Mark Completed
                    </button>
                    <button
                      onClick={() => handleUpdateStatus(null)}
                      className="btn btn-ghost btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Remove from Backlog
                    </button>
                  </>
                )}
                {collectionItem?.status === 'playing' && (
                  <>
                    <button
                      onClick={() => handleUpdateStatus('completed')}
                      className="btn btn-success btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Mark Completed
                    </button>
                    <button
                      onClick={() => handleUpdateStatus('backlog')}
                      className="btn btn-secondary btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Move to Backlog
                    </button>
                    <button
                      onClick={() => handleUpdateStatus(null)}
                      className="btn btn-ghost btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Clear Status
                    </button>
                  </>
                )}
                {collectionItem?.status === 'completed' && (
                  <>
                    <button
                      onClick={() => handleUpdateStatus('playing')}
                      className="btn btn-primary btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Play Again
                    </button>
                    <button
                      onClick={() => handleUpdateStatus('backlog')}
                      className="btn btn-secondary btn-sm"
                      disabled={updateGameStatus.isPending}
                    >
                      Move to Backlog
                    </button>
                  </>
                )}
              </div>
            </div>
          )}

          {id && <GameClubReviewsSection gameId={id} />}

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

      {game && (
        <SuggestGameModal
          isOpen={showSuggestModal}
          onClose={() => setShowSuggestModal(false)}
          mode="pick-friend"
          gameId={id}
          gameName={game.name}
        />
      )}
    </div>
  );
}
