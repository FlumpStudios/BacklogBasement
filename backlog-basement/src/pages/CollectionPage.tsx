import { useEffect, useState, useMemo } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useCollection, useRemoveFromCollection } from '../hooks';
import { CollectionStats, CollectionFilters, SortOption, PlayStatusFilter, SourceFilter, GameStatusFilter } from '../features/collection';
import { GameGrid } from '../features/games';
import { EmptyState, useToast, SteamSection } from '../components';
import { CollectionItemDto } from '../types';
import './CollectionPage.css';

export function CollectionPage() {
  const { data: collection, isLoading } = useCollection();
  const removeFromCollection = useRemoveFromCollection();
  const { showToast } = useToast();
  const [searchParams, setSearchParams] = useSearchParams();

  // Filter and sort state
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<SortOption>('name-asc');
  const [playStatus, setPlayStatus] = useState<PlayStatusFilter>('all');
  const [sourceFilter, setSourceFilter] = useState<SourceFilter>('all');
  const [gameStatus, setGameStatus] = useState<GameStatusFilter>('all');

  // Handle Steam linking callback
  useEffect(() => {
    const steamStatus = searchParams.get('steam');
    if (steamStatus === 'linked') {
      showToast('Steam account linked successfully!', 'success');
      searchParams.delete('steam');
      setSearchParams(searchParams, { replace: true });
    } else if (steamStatus === 'error') {
      const message = searchParams.get('message');
      showToast(`Failed to link Steam account: ${message || 'Unknown error'}`, 'error');
      searchParams.delete('steam');
      searchParams.delete('message');
      setSearchParams(searchParams, { replace: true });
    }
  }, [searchParams, setSearchParams, showToast]);

  // Filter and sort the collection
  const filteredAndSortedCollection = useMemo(() => {
    if (!collection) return [];

    let result = [...collection];

    // Apply search filter
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase().trim();
      result = result.filter((item) =>
        item.gameName.toLowerCase().includes(query)
      );
    }

    // Apply play status filter
    if (playStatus === 'played') {
      result = result.filter((item) => (item.totalPlayTimeMinutes || 0) > 0);
    } else if (playStatus === 'unplayed') {
      result = result.filter((item) => (item.totalPlayTimeMinutes || 0) === 0);
    }

    // Apply source filter
    if (sourceFilter === 'steam') {
      result = result.filter((item) => item.source === 'steam');
    } else if (sourceFilter === 'manual') {
      result = result.filter((item) => item.source === 'manual');
    }

    // Apply game status filter
    if (gameStatus === 'none') {
      result = result.filter((item) => !item.status);
    } else if (gameStatus === 'backlog') {
      result = result.filter((item) => item.status === 'backlog');
    } else if (gameStatus === 'playing') {
      result = result.filter((item) => item.status === 'playing');
    } else if (gameStatus === 'completed') {
      result = result.filter((item) => item.status === 'completed');
    }

    // Apply sorting
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
        default:
          return 0;
      }
    });

    return result;
  }, [collection, searchQuery, sortBy, playStatus, sourceFilter, gameStatus]);

  const handleRemove = async (gameId: string, gameName: string) => {
    try {
      await removeFromCollection.mutateAsync(gameId);
      showToast(`Removed "${gameName}" from your collection`, 'success');
    } catch {
      showToast('Failed to remove game', 'error');
    }
  };

  const renderActions = (item: CollectionItemDto) => (
    <button
      onClick={(e) => {
        e.preventDefault();
        handleRemove(item.gameId, item.gameName);
      }}
      className="btn btn-danger btn-sm"
      disabled={removeFromCollection.isPending}
    >
      Remove
    </button>
  );

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading your collection...</p>
      </div>
    );
  }

  return (
    <div className="collection-page">
      <header className="collection-header">
        <h1>My Collection</h1>
        <p className="collection-subtitle">
          All the games in your personal library
        </p>
      </header>

      <SteamSection />

      {collection && collection.length > 0 ? (
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
            <GameGrid
              games={filteredAndSortedCollection}
              showPlaytime
              renderActions={(item) => renderActions(item as CollectionItemDto)}
            />
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
          title="Your collection is empty"
          description="Search for games and add them to start building your collection."
          action={
            <Link to="/search" className="btn btn-primary">
              Search Games
            </Link>
          }
        />
      )}
    </div>
  );
}
