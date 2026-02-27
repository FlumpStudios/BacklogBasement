import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useGlobalLeaderboard, useFriendLeaderboard } from '../../hooks';
import { LeaderboardEntryDto } from '../../types';
import './LeaderboardWidget.css';

type Tab = 'global' | 'friends';

function RankBadge({ rank }: { rank: number }) {
  if (rank === 1) return <span className="lb-rank lb-rank--gold">#1</span>;
  if (rank === 2) return <span className="lb-rank lb-rank--silver">#2</span>;
  if (rank === 3) return <span className="lb-rank lb-rank--bronze">#3</span>;
  return <span className="lb-rank">#{rank}</span>;
}

function LeaderboardRow({ entry }: { entry: LeaderboardEntryDto }) {
  const name = entry.username ?? entry.displayName;
  return (
    <div className={`lb-row${entry.isCurrentUser ? ' lb-row--me' : ''}`}>
      <RankBadge rank={entry.rank} />
      <span className="lb-name">
        {entry.username ? (
          <Link to={`/profile/${entry.username}`} className="lb-name-link">{name}</Link>
        ) : (
          name
        )}
        {entry.isCurrentUser && <span className="lb-you-badge">You</span>}
      </span>
      <span className="lb-level" title={entry.levelName}>Lv.{entry.level}</span>
      <span className="lb-xp">{entry.xpTotal.toLocaleString()} XP</span>
    </div>
  );
}

export function LeaderboardWidget() {
  const [tab, setTab] = useState<Tab>('global');
  const [isOpen, setIsOpen] = useState(true);

  const { data: globalEntries, isLoading: globalLoading } = useGlobalLeaderboard();
  const { data: friendEntries, isLoading: friendLoading } = useFriendLeaderboard();

  const entries = tab === 'global' ? globalEntries : friendEntries;
  const isLoading = tab === 'global' ? globalLoading : friendLoading;

  const WIDGET_LIMIT = 5;
  const topEntries = entries?.slice(0, WIDGET_LIMIT) ?? [];
  const currentUserEntry = entries?.find(e => e.isCurrentUser);
  const currentUserInTop = topEntries.some(e => e.isCurrentUser);
  const showCurrentUserBelow = currentUserEntry && !currentUserInTop;

  return (
    <section className="dashboard-section lb-widget-section">
      <button
        className="lb-accordion-header"
        onClick={() => setIsOpen(o => !o)}
        aria-expanded={isOpen}
      >
        <h2>Leaderboard</h2>
        <div className="lb-accordion-meta">
          <div className="lb-tabs" onClick={e => e.stopPropagation()}>
            <button
              className={`lb-tab${tab === 'global' ? ' lb-tab--active' : ''}`}
              onClick={() => setTab('global')}
            >
              Global
            </button>
            <button
              className={`lb-tab${tab === 'friends' ? ' lb-tab--active' : ''}`}
              onClick={() => setTab('friends')}
            >
              Friends
            </button>
          </div>
          <span className="lb-chevron">{isOpen ? '▲' : '▼'}</span>
        </div>
      </button>

      {isOpen && (
        <>
          {isLoading ? (
            <div className="lb-loading"><div className="loading-spinner" /></div>
          ) : tab === 'friends' && (!entries || entries.length <= 1) ? (
            <p className="lb-empty">Add friends to see how you compare!</p>
          ) : entries && entries.length === 0 ? (
            <p className="lb-empty">No entries yet.</p>
          ) : (
            <div className="lb-list">
              {topEntries.map(entry => (
                <LeaderboardRow key={entry.userId.toString()} entry={entry} />
              ))}
              {showCurrentUserBelow && (
                <>
                  <div className="lb-ellipsis">···</div>
                  <LeaderboardRow entry={currentUserEntry} />
                </>
              )}
            </div>
          )}

          <div className="lb-footer">
            <Link to="/leaderboard" className="lb-view-all">View full leaderboard →</Link>
          </div>
        </>
      )}
    </section>
  );
}
