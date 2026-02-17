import { useState, useMemo } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useCollection } from '../hooks';
import { useProfile } from '../hooks';
import { GameGrid } from '../features/games';
import { EmptyState } from '../components';
import { CollectionItemDto } from '../types';
import './CompareCollectionsPage.css';

type Tab = 'common' | 'only-you' | 'only-them';

export function CompareCollectionsPage() {
  const { username } = useParams<{ username: string }>();
  const { data: myCollection, isLoading: loadingMine } = useCollection();
  const { data: profile, isLoading: loadingProfile, isError } = useProfile(username ?? '');

  const [activeTab, setActiveTab] = useState<Tab>('common');
  const [search, setSearch] = useState('');

  const { common, onlyYou, onlyThem } = useMemo(() => {
    if (!myCollection || !profile) {
      return { common: [], onlyYou: [], onlyThem: [] };
    }

    const myGameIds = new Set(myCollection.map((g) => g.gameId));
    const theirGameIds = new Set(profile.collection.map((g) => g.gameId));

    const common: CollectionItemDto[] = [];
    const onlyYou: CollectionItemDto[] = [];
    const onlyThem: CollectionItemDto[] = [];

    for (const game of myCollection) {
      if (theirGameIds.has(game.gameId)) {
        common.push(game);
      } else {
        onlyYou.push(game);
      }
    }

    for (const game of profile.collection) {
      if (!myGameIds.has(game.gameId)) {
        onlyThem.push(game);
      }
    }

    return { common, onlyYou, onlyThem };
  }, [myCollection, profile]);

  const activeGames = useMemo(() => {
    const list = activeTab === 'common' ? common : activeTab === 'only-you' ? onlyYou : onlyThem;
    if (!search.trim()) return list;
    const q = search.toLowerCase();
    return list.filter((g) => g.gameName.toLowerCase().includes(q));
  }, [activeTab, common, onlyYou, onlyThem, search]);

  const isLoading = loadingMine || loadingProfile;

  if (isLoading) {
    return (
      <div className="compare-page">
        <div className="loading-container">
          <div className="loading-spinner" />
          <p>Loading collections...</p>
        </div>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="compare-page">
        <EmptyState
          icon="👤"
          title="Profile not found"
          description="This user doesn't exist or hasn't set up their profile yet."
        />
      </div>
    );
  }

  const displayName = profile.displayName;
  const tabs: { key: Tab; label: string; count: number }[] = [
    { key: 'common', label: 'In Common', count: common.length },
    { key: 'only-you', label: 'Only You', count: onlyYou.length },
    { key: 'only-them', label: `Only ${displayName}`, count: onlyThem.length },
  ];

  return (
    <div className="compare-page">
      <header className="compare-header">
        <Link to={`/profile/${profile.username}`} className="compare-back-link">
          &larr; Back to {displayName}'s profile
        </Link>
        <h1>Comparing with {displayName}</h1>
      </header>

      <div className="compare-summary">
        <div className="compare-stat">
          <span className="compare-stat-value">{common.length}</span>
          <span className="compare-stat-label">In Common</span>
        </div>
        <div className="compare-stat">
          <span className="compare-stat-value">{onlyYou.length}</span>
          <span className="compare-stat-label">Only You</span>
        </div>
        <div className="compare-stat">
          <span className="compare-stat-value">{onlyThem.length}</span>
          <span className="compare-stat-label">Only {displayName}</span>
        </div>
      </div>

      <div className="compare-tabs">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            className={`compare-tab${activeTab === tab.key ? ' active' : ''}`}
            onClick={() => setActiveTab(tab.key)}
          >
            {tab.label} ({tab.count})
          </button>
        ))}
      </div>

      <div className="compare-search">
        <input
          type="text"
          placeholder="Search games..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {activeGames.length > 0 ? (
        <GameGrid games={activeGames} showPlaytime />
      ) : (
        <EmptyState
          icon="🎮"
          title="No games found"
          description={
            search.trim()
              ? 'No games match your search in this tab.'
              : activeTab === 'common'
                ? "You don't have any games in common yet."
                : activeTab === 'only-you'
                  ? `All your games are also in ${displayName}'s collection.`
                  : `${displayName} doesn't have any unique games.`
          }
        />
      )}
    </div>
  );
}
