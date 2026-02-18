import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../auth';
import { useProfile, useFriendshipStatus } from '../hooks';
import { GameGrid } from '../features/games';
import { SuggestGameModal } from '../features/suggestions';
import { EmptyState, FriendButton } from '../components';
import { formatPlaytime } from '../utils';
import './ProfilePage.css';

export function ProfilePage() {
  const { username } = useParams<{ username: string }>();
  const { user, isAuthenticated } = useAuth();
  const { data: profile, isLoading, isError } = useProfile(username ?? '');

  const [showSuggestModal, setShowSuggestModal] = useState(false);
  const { data: friendshipStatus } = useFriendshipStatus(
    !isAuthenticated || user?.username === profile?.username ? undefined : profile?.userId
  );

  const isOwnProfile = user?.username === profile?.username;
  const isFriend = friendshipStatus?.status === 'friends';
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
        {!isOwnProfile && isAuthenticated && profile.userId && (
          <FriendButton userId={profile.userId} />
        )}
        {!isOwnProfile && isAuthenticated && (
          <Link to={`/profile/${profile.username}/compare`} className="btn btn-secondary">
            Compare collections
          </Link>
        )}
        {!isOwnProfile && isAuthenticated && isFriend && (
          <button className="btn btn-secondary" onClick={() => setShowSuggestModal(true)}>
            Suggest a game
          </button>
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
        <div className="stat-card">
          <span className="stat-value">{profile.stats.friendCount}</span>
          <span className="stat-label">Friends</span>
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

      {profile.friends && profile.friends.length > 0 && (
        <section className="profile-section">
          <h2>Friends</h2>
          <div className="profile-friends-list">
            {profile.friends.map((friend) => (
              <Link key={friend.userId} to={`/profile/${friend.username}`} className="profile-friend-card">
                <span className="profile-friend-name">{friend.displayName}</span>
                <span className="profile-friend-username">@{friend.username}</span>
              </Link>
            ))}
          </div>
        </section>
      )}

      {profile.collection.length > 0 && (
        <section className="profile-section">
          <Link to={isOwnProfile ? '/collection' : `/profile/${profile.username}/collection`} className="btn btn-secondary">
            View full collection
          </Link>
        </section>
      )}

      {profile && (
        <SuggestGameModal
          isOpen={showSuggestModal}
          onClose={() => setShowSuggestModal(false)}
          mode="pick-game"
          friendUserId={profile.userId}
          friendDisplayName={profile.displayName}
        />
      )}
    </div>
  );
}
