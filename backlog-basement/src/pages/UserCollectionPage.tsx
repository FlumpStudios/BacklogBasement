import { useState, useMemo } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useProfile } from '../hooks';
import { CollectionStats, CollectionFilters, SortOption, PlayStatusFilter, SourceFilter, GameStatusFilter } from '../features/collection';
import { GameGrid } from '../features/games';
import { EmptyState } from '../components';
import './CollectionPage.css';

export function UserCollectionPage() {
  const { username } = useParams<{ username: string }>();
  const { data: profile, isLoading, isError } = useProfile(username ?? '');

  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<SortOption>('name-asc');
  const [playStatus, setPlayStatus] = useState<PlayStatusFilter>('all');
  const [sourceFilter, setSourceFilter] = useState<SourceFilter>('all');
  const [gameStatus, setGameStatus] = useState<GameStatusFilter>('all');

  const collection = profile?.collection ?? [];

  const filteredAndSortedCollection = useMemo(() => {
    let result = [...collection];

    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase().trim();
      result = result.filter((item) =>
        item.gameName.toLowerCase().includes(query)
      );
    }

    if (playStatus === 'played') {
      result = result.filter((item) => (item.totalPlayTimeMinutes || 0) > 0);
    } else if (playStatus === 'unplayed') {
      result = result.filter((item) => (item.totalPlayTimeMinutes || 0) === 0);
    }

    if (sourceFilter === 'steam') {
      result = result.filter((item) => item.source === 'steam');
    } else if (sourceFilter === 'manual') {
      result = result.filter((item) => item.source === 'manual');
    }

    if (gameStatus === 'none') {
      result = result.filter((item) => !item.status);
    } else if (gameStatus !== 'all') {
      result = result.filter((item) => item.status === gameStatus);
    }

    result.sort((a, b) => {
      switch (sortBy) {
        case 'name-asc':
          return a.gameName.localeCompare(b.gameName);
        case 'name-desc':
          return b.gameName.localeCompare(a.gameName);
        case 'release-desc': {
          const dateA = a.releaseDate ? new Date(a.releaseDate).getTime() : 0;
          const dateB = b.releaseDate ? new Date(b.releaseDate).getTime() : 0;
          return dateB - dateA;
        }
        case 'release-asc': {
          const dateA = a.releaseDate ? new Date(a.releaseDate).getTime() : Infinity;
          const dateB = b.releaseDate ? new Date(b.releaseDate).getTime() : Infinity;
          return dateA - dateB;
        }
        case 'added-desc':
          return new Date(b.dateAdded).getTime() - new Date(a.dateAdded).getTime();
        case 'added-asc':
          return new Date(a.dateAdded).getTime() - new Date(b.dateAdded).getTime();
        case 'playtime-desc':
          return (b.totalPlayTimeMinutes || 0) - (a.totalPlayTimeMinutes || 0);
        case 'playtime-asc':
          return (a.totalPlayTimeMinutes || 0) - (b.totalPlayTimeMinutes || 0);
        case 'score-desc': {
          const scoreA = a.criticScore ?? -1;
          const scoreB = b.criticScore ?? -1;
          return scoreB - scoreA;
        }
        case 'score-asc': {
          const scoreA = a.criticScore ?? 101;
          const scoreB = b.criticScore ?? 101;
          return scoreA - scoreB;
        }
        default:
          return 0;
      }
    });

    return result;
  }, [collection, searchQuery, sortBy, playStatus, sourceFilter, gameStatus]);

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading collection...</p>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="collection-page">
        <EmptyState
          icon="👤"
          title="Profile not found"
          description="This user doesn't exist or hasn't set up their profile yet."
        />
      </div>
    );
  }

  return (
    <div className="collection-page">
      <header className="collection-header">
        <h1>{profile.displayName}'s Collection</h1>
        <p className="collection-subtitle">
          <Link to={`/profile/${profile.username}`}>Back to profile</Link>
        </p>
      </header>

      {collection.length > 0 ? (
        <>
          <CollectionStats collection={collection} />
          <CollectionFilters
            searchQuery={searchQuery}
            onSearchChange={setSearchQuery}
            sortBy={sortBy}
            onSortChange={setSortBy}
            playStatus={playStatus}
            onPlayStatusChange={setPlayStatus}
            sourceFilter={sourceFilter}
            onSourceFilterChange={setSourceFilter}
            gameStatus={gameStatus}
            onGameStatusChange={setGameStatus}
            resultCount={filteredAndSortedCollection.length}
            totalCount={collection.length}
          />
          {filteredAndSortedCollection.length > 0 ? (
            <GameGrid games={filteredAndSortedCollection} showPlaytime />
          ) : (
            <div className="no-results">
              <p>No games match your filters.</p>
              <button
                className="btn btn-secondary"
                onClick={() => {
                  setSearchQuery('');
                  setPlayStatus('all');
                  setSourceFilter('all');
                  setGameStatus('all');
                }}
              >
                Clear Filters
              </button>
            </div>
          )}
        </>
      ) : (
        <EmptyState
          icon="📚"
          title="No games yet"
          description={`${profile.displayName} hasn't added any games to their collection yet.`}
        />
      )}
    </div>
  );
}
