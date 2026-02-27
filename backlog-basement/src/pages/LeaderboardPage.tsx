import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useGlobalLeaderboard, useFriendLeaderboard } from '../hooks';
import { LeaderboardEntryDto } from '../types';
import './LeaderboardPage.css';

type Tab = 'global' | 'friends';

function RankBadge({ rank }: { rank: number }) {
  if (rank === 1) return <span className="lb-page-rank lb-page-rank--gold">#1</span>;
  if (rank === 2) return <span className="lb-page-rank lb-page-rank--silver">#2</span>;
  if (rank === 3) return <span className="lb-page-rank lb-page-rank--bronze">#3</span>;
  return <span className="lb-page-rank">#{rank}</span>;
}

function LeaderboardRow({ entry }: { entry: LeaderboardEntryDto }) {
  const name = entry.username ?? entry.displayName;
  return (
    <div className={`lb-page-row${entry.isCurrentUser ? ' lb-page-row--me' : ''}`}>
      <RankBadge rank={entry.rank} />
      <span className="lb-page-name">
        {entry.username ? (
          <Link to={`/profile/${entry.username}`} className="lb-page-name-link">{name}</Link>
        ) : (
          name
        )}
        {entry.isCurrentUser && <span className="lb-page-you-badge">You</span>}
      </span>
      <span className="lb-page-level" title={entry.levelName}>Lv.{entry.level}</span>
      <span className="lb-page-xp">{entry.xpTotal.toLocaleString()} XP</span>
    </div>
  );
}

export function LeaderboardPage() {
  const [tab, setTab] = useState<Tab>('global');
  const { data: globalEntries, isLoading: globalLoading } = useGlobalLeaderboard();
  const { data: friendEntries, isLoading: friendLoading } = useFriendLeaderboard();

  const entries = tab === 'global' ? globalEntries : friendEntries;
  const isLoading = tab === 'global' ? globalLoading : friendLoading;

  const PAGE_LIMIT = 100;
  const topEntries = tab === 'global' ? (entries?.slice(0, PAGE_LIMIT) ?? []) : (entries ?? []);
  const currentUserEntry = entries?.find(e => e.isCurrentUser);
  const currentUserInTop = topEntries.some(e => e.isCurrentUser);
  const showCurrentUserBelow = tab === 'global' && currentUserEntry && !currentUserInTop;

  return (
    <div className="lb-page">
      <header className="lb-page-header">
        <h1>Leaderboard</h1>
        <p className="lb-page-subtitle">See who's earned the most XP</p>
      </header>

      <div className="lb-page-tabs">
        <button
          className={`lb-page-tab${tab === 'global' ? ' lb-page-tab--active' : ''}`}
          onClick={() => setTab('global')}
        >
          Global
        </button>
        <button
          className={`lb-page-tab${tab === 'friends' ? ' lb-page-tab--active' : ''}`}
          onClick={() => setTab('friends')}
        >
          Friends
        </button>
      </div>

      {isLoading ? (
        <div className="lb-page-loading">
          <div className="loading-spinner" />
        </div>
      ) : tab === 'friends' && (!entries || entries.length <= 1) ? (
        <div className="lb-page-empty">
          <p>Add friends to see how you compare!</p>
          <Link to="/friends" className="btn btn-primary">Find Friends</Link>
        </div>
      ) : entries && entries.length === 0 ? (
        <p className="lb-page-empty-text">No entries yet.</p>
      ) : (
        <div className="lb-page-list">
          {topEntries.map(entry => (
            <LeaderboardRow key={entry.userId.toString()} entry={entry} />
          ))}
          {showCurrentUserBelow && (
            <>
              <div className="lb-page-ellipsis">···</div>
              <LeaderboardRow entry={currentUserEntry} />
            </>
          )}
        </div>
      )}
    </div>
  );
}
