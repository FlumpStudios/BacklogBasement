import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../auth';
import { useProfile } from '../hooks';
import { GameGrid } from '../features/games';
import { EmptyState } from '../components';
import { formatPlaytime } from '../utils';
import './ProfilePage.css';

export function ProfilePage() {
  const { username } = useParams<{ username: string }>();
  const { user } = useAuth();
  const { data: profile, isLoading, isError } = useProfile(username ?? '');

  const isOwnProfile = user?.username === profile?.username;
  const completedGames = profile?.collection.filter(g => g.status === 'completed') ?? [];

  if (isLoading) {
    return (
      <div className="profile-page">
        <div className="loading-container">
          <div className="loading-spinner" />
          <p>Loading profile...</p>
        </div>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="profile-page">
        <EmptyState
          icon="👤"
          title="Profile not found"
          description="This user doesn't exist or hasn't set up their profile yet."
        />
      </div>
    );
  }

  return (
    <div className="profile-page">
      <header className="profile-header">
        <div className="profile-identity">
          <h1 className="profile-display-name">{profile.displayName}</h1>
          <span className="profile-username">@{profile.username}</span>
        </div>
        <span className="profile-member-since">
          Member since {new Date(profile.memberSince).toLocaleDateString(undefined, { year: 'numeric', month: 'long' })}
        </span>
        {isOwnProfile && (
          <span className="profile-own-badge">This is your profile</span>
        )}
      </header>

      <div className="profile-stats">
        <div className="stat-card">
          <span className="stat-value">{profile.stats.totalGames}</span>
          <span className="stat-label">Games</span>
        </div>
        <div className="stat-card">
          <span className="stat-value">{formatPlaytime(profile.stats.totalPlayTimeMinutes)}</span>
          <span className="stat-label">Total Time</span>
        </div>
        <div className="stat-card">
          <span className="stat-value">{profile.stats.playingCount}</span>
          <span className="stat-label">Playing</span>
        </div>
        <div className="stat-card">
          <span className="stat-value">{profile.stats.backlogCount}</span>
          <span className="stat-label">Backlog</span>
        </div>
        <div className="stat-card">
          <span className="stat-value">{profile.stats.completedCount}</span>
          <span className="stat-label">Completed</span>
        </div>
      </div>

      {profile.currentlyPlaying.length > 0 && (
        <section className="profile-section">
          <h2>Currently Playing</h2>
          <GameGrid games={profile.currentlyPlaying} showPlaytime />
        </section>
      )}

      {profile.backlog.length > 0 && (
        <section className="profile-section">
          <h2>Backlog</h2>
          <GameGrid games={profile.backlog} showPlaytime />
        </section>
      )}

      {completedGames.length > 0 && (
        <section className="profile-section">
          <h2>Completed</h2>
          <GameGrid games={completedGames} showPlaytime />
        </section>
      )}

      {profile.collection.length > 0 && (
        <section className="profile-section">
          <Link to="/collection" className="btn btn-secondary">
            View full collection
          </Link>
        </section>
      )}
    </div>
  );
}
