import { Link } from 'react-router-dom';
import { useAuth } from '../auth';
import { useCollection } from '../hooks';
import { CollectionStats } from '../features/collection';
import { GameGrid } from '../features/games';
import { EmptyState } from '../components';
import './DashboardPage.css';

export function DashboardPage() {
  const { user } = useAuth();
  const { data: collection, isLoading } = useCollection();

  const recentGames = collection?.slice(0, 4) ?? [];

  return (
    <div className="dashboard-page">
      <header className="dashboard-header">
        <h1>Welcome back, {user?.displayName?.split(' ')[0] ?? 'Gamer'}!</h1>
        <p className="dashboard-subtitle">Here's your gaming overview</p>
      </header>

      {isLoading ? (
        <div className="loading-container">
          <div className="loading-spinner" />
          <p>Loading your collection...</p>
        </div>
      ) : collection && collection.length > 0 ? (
        <>
          <CollectionStats collection={collection} />

          <section className="dashboard-section">
            <div className="section-header">
              <h2>Recent Games</h2>
              <Link to="/collection" className="btn btn-secondary btn-sm">
                View All
              </Link>
            </div>
            <GameGrid games={recentGames} showPlaytime />
          </section>
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
