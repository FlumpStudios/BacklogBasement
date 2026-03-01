import { useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../auth';
import { useCollection, useUpdateGameStatus, useMyClubs, useTwitchSync } from '../hooks';
import { CollectionStats } from '../features/collection';
import { GameGrid } from '../features/games';
import { SuggestionsSection } from '../features/suggestions';
import { EmptyState, useToast, DailyPoll, DailyQuiz, LeaderboardWidget } from '../components';
import { CollectionItemDto, GameClubDto } from '../types';
import './DashboardPage.css';

function getClubCta(club: GameClubDto) {
  const r = club.currentRound;
  if (!r) return null;
  if (r.status === 'voting' && !r.userHasVoted) return { type: 'vote', label: '🗳️ Vote Now' };
  if (r.status === 'reviewing' && !r.userHasReviewed) return { type: 'review', label: '✍️ Write Review' };
  if (r.status === 'nominating' && !r.userHasNominated) return { type: 'nominate', label: '🎮 Nominate a Game' };
  if (r.status === 'playing') return { type: 'playing', label: r.gameName ?? 'Currently Playing' };
  return null;
}

export function DashboardPage() {
  const { user } = useAuth();
  const { data: collection, isLoading } = useCollection();
  const { data: myClubs } = useMyClubs();
  const updateGameStatus = useUpdateGameStatus();
  const { showToast } = useToast();
  const twitchSync = useTwitchSync();
  const syncedRef = useRef(false);

  // Auto-sync Twitch live status once per dashboard visit
  useEffect(() => {
    if (!user?.hasTwitchLinked || syncedRef.current) return;
    syncedRef.current = true;
    twitchSync.mutate(undefined, {
      onSuccess: (data) => {
        if (data.updatedPlayingStatus && data.gameName) {
          showToast(`You're live on Twitch — marked "${data.gameName}" as Playing`, 'success');
        }
      },
    });
  }, [user?.hasTwitchLinked]);

  const completedGames = collection?.filter(g => g.status === 'completed').slice(0, 4) ?? [];
  const currentlyPlaying = collection?.filter(g => g.status === 'playing') ?? [];
  const backlogGames = collection?.filter(g => g.status === 'backlog')
    .sort((a, b) => new Date(a.dateAdded).getTime() - new Date(b.dateAdded).getTime())
    .slice(0, 4) ?? [];

  // Games with no status, sorted by unplayed first, then by date added (oldest first)
  const recommendedForBacklog = collection
    ?.filter(g => !g.status)
    .sort((a, b) => {
      // Critic score first (highest first, games with scores before those without)
      const aScore = a.criticScore ?? -1;
      const bScore = b.criticScore ?? -1;
      if (aScore !== bScore) return bScore - aScore;
      // Then unplayed games first
      const aUnplayed = (a.totalPlayTimeMinutes || 0) === 0;
      const bUnplayed = (b.totalPlayTimeMinutes || 0) === 0;
      if (aUnplayed && !bUnplayed) return -1;
      if (!aUnplayed && bUnplayed) return 1;
      // Then by date added (oldest first - they've been waiting longest)
      return new Date(a.dateAdded).getTime() - new Date(b.dateAdded).getTime();
    })
    .slice(0, 4) ?? [];

  const handleAddToBacklog = async (game: CollectionItemDto) => {
    try {
      await updateGameStatus.mutateAsync({ gameId: game.gameId, status: 'backlog' });
      showToast(`Added "${game.gameName}" to your backlog`, 'success');
    } catch {
      showToast('Failed to update status', 'error');
    }
  };

  const handleMarkCompleted = async (game: CollectionItemDto) => {
    try {
      await updateGameStatus.mutateAsync({ gameId: game.gameId, status: 'completed' });
      showToast(`Marked "${game.gameName}" as completed`, 'success');
    } catch {
      showToast('Failed to update status', 'error');
    }
  };

  const handleStartPlaying = async (game: CollectionItemDto) => {
    try {
      await updateGameStatus.mutateAsync({ gameId: game.gameId, status: 'playing' });
      showToast(`Started playing "${game.gameName}"`, 'success');
    } catch {
      showToast('Failed to update status', 'error');
    }
  };

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <h1>Welcome back, {user?.username ?? user?.displayName?.split(' ')[0] ?? 'Gamer'}!</h1>
        <p className="dashboard-subtitle">Here's your gaming overview</p>
      </header>

      <div className="community-widgets">
        <DailyPoll />
        <DailyQuiz />
      </div>

      <LeaderboardWidget />

      {isLoading ? (
        <div className="loading-container">
          <div className="loading-spinner" />
          <p>Loading your collection...</p>
        </div>
      ) : collection && collection.length > 0 ? (
        <>
          <CollectionStats collection={collection} basePath="/collection" />

          <SuggestionsSection />

          {myClubs && myClubs.length > 0 && (
            <section className="dashboard-section">
              <div className="section-header">
                <h2>Your Clubs</h2>
                <Link to="/clubs" className="btn btn-secondary btn-sm">
                  View All Clubs →
                </Link>
              </div>
              <div className="club-dashboard-cards">
                {myClubs.slice(0, 3).map(club => {
                  const cta = getClubCta(club);
                  return (
                    <Link key={club.id} to={`/clubs/${club.id}`} className="club-dashboard-card">
                      <span className="club-dashboard-card-name">{club.name}</span>
                      {cta ? (
                        <span className={`club-dashboard-card-action club-action-${cta.type}`}>
                          {cta.label}{cta.type === 'playing' ? ' · Currently Playing' : ''}
                        </span>
                      ) : (
                        <span className="club-dashboard-card-action club-action-view">
                          View Club →
                        </span>
                      )}
                    </Link>
                  );
                })}
              </div>
            </section>
          )}

          {currentlyPlaying.length > 0 && (
            <section className="dashboard-section">
              <div className="section-header">
                <h2>Currently Playing</h2>
                <Link to="/collection?status=playing" className="btn btn-secondary btn-sm">
                  View All
                </Link>
              </div>
              <GameGrid
                games={currentlyPlaying}
                showPlaytime
                renderActions={(item) => (
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      handleMarkCompleted(item as CollectionItemDto);
                    }}
                    className="btn btn-secondary btn-sm"
                    disabled={updateGameStatus.isPending}
                  >
                    Mark Completed
                  </button>
                )}
              />
            </section>
          )}

          {backlogGames.length > 0 && (
            <section className="dashboard-section">
              <div className="section-header">
                <h2>Your Backlog</h2>
                <Link to="/collection?status=backlog&sort=added-asc" className="btn btn-secondary btn-sm">
                  View All ({collection?.filter(g => g.status === 'backlog').length})
                </Link>
              </div>
              <GameGrid
                games={backlogGames}
                showPlaytime
                renderActions={(item) => (
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      handleStartPlaying(item as CollectionItemDto);
                    }}
                    className="btn btn-secondary btn-sm"
                    disabled={updateGameStatus.isPending}
                  >
                    Start Playing
                  </button>
                )}
              />
            </section>
          )}

          {completedGames.length > 0 && (
            <section className="dashboard-section">
              <div className="section-header">
                <h2>Completed Games</h2>
                <Link to="/collection?status=completed" className="btn btn-secondary btn-sm">
                  View All ({collection?.filter(g => g.status === 'completed').length})
                </Link>
              </div>
              <GameGrid games={completedGames} showPlaytime />
            </section>
          )}

          {recommendedForBacklog.length > 0 && (
            <section className="dashboard-section">
              <div className="section-header">
                <h2>Add to Your Backlog?</h2>
                <Link to="/collection?playStatus=unplayed" className="btn btn-secondary btn-sm">
                  View All ({collection?.filter(g => (g.totalPlayTimeMinutes || 0) === 0).length})
                </Link>
              </div>
              <p className="section-description">
                Games with no play time
              </p>
              <GameGrid
                games={recommendedForBacklog}
                showPlaytime
                renderActions={(item) => (
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      handleAddToBacklog(item as CollectionItemDto);
                    }}
                    className="btn btn-secondary btn-sm"
                    disabled={updateGameStatus.isPending}
                  >
                    + Add to Backlog
                  </button>
                )}
              />
            </section>
          )}
        </>
      ) : (
        <EmptyState
          icon="🎮"
          title="Your collection is empty"
          description="Start by searching for games and adding them to your collection."
          action={
            <Link to="/search" className="btn btn-primary">
              Search Games
            </Link>
          }
        />
      )}

      <section className="dashboard-section">
        <h2>Quick Actions</h2>
        <div className="quick-actions">
          <Link to="/search" className="quick-action-card">
            <span className="quick-action-icon">🔍</span>
            <span className="quick-action-label">Search Games</span>
          </Link>
          <Link to="/collection" className="quick-action-card">
            <span className="quick-action-icon">📚</span>
            <span className="quick-action-label">My Collection</span>
          </Link>
        </div>
      </section>
    </div>
  );
}
