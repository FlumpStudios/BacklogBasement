import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useActivityFeed } from '../../hooks';
import { ActivityEventDto } from '../../types';
import { Avatar } from '../Avatar';
import './ActivityFeed.css';

const PAGE_SIZE = 10;

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const minutes = Math.floor(diff / 60_000);
  if (minutes < 1) return 'just now';
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(dateStr).toLocaleDateString();
}

function UserLink({ username, displayName }: { username: string; displayName: string }) {
  const name = username || displayName;
  if (username) {
    return <Link to={`/profile/${username}`} className="af-link">{name}</Link>;
  }
  return <strong>{name}</strong>;
}

function GameLink({ gameId, gameName }: { gameId?: string | null; gameName?: string | null }) {
  if (!gameId || !gameName) return null;
  return <Link to={`/games/${gameId}`} className="af-link">{gameName}</Link>;
}

function ClubLink({ clubId, clubName }: { clubId?: string | null; clubName?: string | null }) {
  if (!clubId || !clubName) return null;
  return <Link to={`/clubs/${clubId}`} className="af-link">{clubName}</Link>;
}

function FeedItemText({ event }: { event: ActivityEventDto }) {
  const user = <UserLink username={event.username} displayName={event.displayName} />;
  const game = <GameLink gameId={event.gameId} gameName={event.gameName} />;
  const club = <ClubLink clubId={event.clubId} clubName={event.clubName} />;

  switch (event.eventType) {
    case 'game_added':
      return <span>{user} added {game} to their collection</span>;
    case 'game_started':
      return <span>{user} started playing {game}</span>;
    case 'game_completed':
      return <span>{user} completed {game} ✓</span>;
    case 'club_round_started':
      return <span>{club} started a new round: {game}</span>;
    case 'club_review_posted':
      return <span>{user} posted a review for {game}</span>;
    case 'level_up':
      return <span>{user} reached <strong>Level {event.intValue}</strong> 🎉</span>;
    case 'poll_voted':
      return <span>{user} voted in today's poll</span>;
    case 'quiz_answered':
      return event.intValue === 1
        ? <span>{user} got today's quiz right! ✓</span>
        : <span>{user} answered today's quiz</span>;
    default:
      return <span>{user} did something</span>;
  }
}

function FeedItem({ event }: { event: ActivityEventDto }) {
  const hasCover = event.gameCoverUrl && event.gameId;
  return (
    <div className="af-item">
      {event.username ? (
        <Link to={`/profile/${event.username}`} className="af-avatar-link">
          <Avatar avatarUrl={event.userAvatarUrl} displayName={event.displayName} userId={event.userId} size="sm" />
        </Link>
      ) : (
        <Avatar avatarUrl={event.userAvatarUrl} displayName={event.displayName} userId={event.userId} size="sm" />
      )}
      {hasCover ? (
        <Link to={`/games/${event.gameId}`} className="af-cover-link">
          <img src={event.gameCoverUrl!} alt={event.gameName ?? ''} className="af-cover" />
        </Link>
      ) : (
        <div className="af-cover-placeholder">
          {event.eventType === 'level_up' ? '🏆'
            : event.eventType === 'poll_voted' ? '🗳️'
            : event.eventType === 'quiz_answered' ? '🧠'
            : '🎮'}
        </div>
      )}
      <div className="af-content">
        <p className="af-text"><FeedItemText event={event} /></p>
        <time className="af-time" dateTime={event.createdAt}>{timeAgo(event.createdAt)}</time>
      </div>
    </div>
  );
}

export function ActivityFeed() {
  const [showAll, setShowAll] = useState(false);
  const { data: events, isLoading } = useActivityFeed();

  const visible = showAll ? (events ?? []) : (events ?? []).slice(0, PAGE_SIZE);
  const hasMore = (events?.length ?? 0) > PAGE_SIZE;

  return (
    <section className="dashboard-section af-section">
      <div className="section-header">
        <h2>Community Activity</h2>
      </div>

      {isLoading ? (
        <div className="af-loading"><div className="loading-spinner" /></div>
      ) : !events || events.length === 0 ? (
        <p className="af-empty">No activity yet — add some games and connect with people!</p>
      ) : (
        <>
          <div className="af-list">
            {visible.map(event => (
              <FeedItem key={event.id} event={event} />
            ))}
          </div>
          {hasMore && (
            <button
              className="af-show-more"
              onClick={() => setShowAll(s => !s)}
            >
              {showAll ? 'Show less' : `Show ${events.length - PAGE_SIZE} more`}
            </button>
          )}
        </>
      )}
    </section>
  );
}
