import { useEffect, useRef, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useInfiniteCollection, useCollectionStats, useRemoveFromCollection, useUpdateGameStatus } from '../hooks';
import { CollectionStats, CollectionFilters, SortOption, PlayStatusFilter, SourceFilter, GameStatusFilter } from '../features/collection';
import { GameGrid } from '../features/games';
import { EmptyState, useToast, SteamSection, RetroArchSection, TwitchSection } from '../components';
import { CollectionItemDto } from '../types';
import { useDebounce } from '../hooks';
import './CollectionPage.css';

function sortOptionToParams(sort: SortOption): { sortBy: string; sortDir: string } {
  const lastDash = sort.lastIndexOf('-');
  return {
    sortBy: sort.slice(0, lastDash),
    sortDir: sort.slice(lastDash + 1),
  };
}

export function CollectionPage() {
  const removeFromCollection = useRemoveFromCollection();
  const updateGameStatus = useUpdateGameStatus();
  const { showToast } = useToast();
  const [searchParams, setSearchParams] = useSearchParams();

  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<SortOption>('added-desc');
  const [playStatus, setPlayStatus] = useState<PlayStatusFilter>('all');
  const [sourceFilter, setSourceFilter] = useState<SourceFilter>('all');
  const [gameStatus, setGameStatus] = useState<GameStatusFilter>('all');

  const debouncedSearch = useDebounce(searchQuery, 300);
  const { sortBy: serverSortBy, sortDir } = sortOptionToParams(sortBy);

  const filters = {
    search: debouncedSearch || undefined,
    status: gameStatus === 'all' ? undefined : gameStatus,
    source: sourceFilter === 'all' ? undefined : sourceFilter,
    playStatus: playStatus === 'all' ? undefined : playStatus,
    sortBy: serverSortBy,
    sortDir,
  };

  const {
    data,
    isLoading,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
  } = useInfiniteCollection(filters);

  const { data: stats } = useCollectionStats();

  const sentinelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
          fetchNextPage();
        }
      },
      { rootMargin: '200px' }
    );
    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

  // Handle URL params (Steam/Twitch linking callbacks, stat card nav links)
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

    const twitchStatus = searchParams.get('twitch');
    if (twitchStatus === 'linked') {
      showToast('Twitch account linked successfully!', 'success');
      searchParams.delete('twitch');
      setSearchParams(searchParams, { replace: true });
    } else if (twitchStatus === 'error') {
      const message = searchParams.get('message');
      showToast(`Failed to link Twitch account: ${message || 'Unknown error'}`, 'error');
      searchParams.delete('twitch');
      searchParams.delete('message');
      setSearchParams(searchParams, { replace: true });
    }

    const statusParam = searchParams.get('status');
    if (statusParam && ['none', 'backlog', 'playing', 'completed'].includes(statusParam)) {
      setGameStatus(statusParam as GameStatusFilter);
      searchParams.delete('status');
      setSearchParams(searchParams, { replace: true });
    }

    const resetParam = searchParams.get('reset');
    if (resetParam === 'true') {
      setSearchQuery('');
      setSortBy('added-desc');
      setPlayStatus('all');
      setSourceFilter('all');
      setGameStatus('all');
      searchParams.delete('reset');
      setSearchParams(searchParams, { replace: true });
    }

    const sortParam = searchParams.get('sort');
    const validSorts: SortOption[] = ['name-asc', 'name-desc', 'release-desc', 'release-asc', 'added-desc', 'added-asc', 'playtime-desc', 'playtime-asc', 'score-desc', 'score-asc'];
    if (sortParam && validSorts.includes(sortParam as SortOption)) {
      setSearchQuery('');
      setSortBy(sortParam as SortOption);
      setSourceFilter('all');
      setGameStatus('all');
      searchParams.delete('sort');
      setSearchParams(searchParams, { replace: true });
    }
  }, [searchParams, setSearchParams, showToast]);

  const allItems = data?.pages.flatMap(p => p.items) ?? [];
  const totalFiltered = data?.pages[0]?.total ?? 0;
  const totalGames = stats?.totalGames ?? 0;

  const handleRemove = async (gameId: string, gameName: string) => {
    try {
      await removeFromCollection.mutateAsync(gameId);
      showToast(`Removed "${gameName}" from your collection`, 'success');
    } catch {
      showToast('Failed to remove game', 'error');
    }
  };

  const handleAddToBacklog = async (gameId: string, gameName: string) => {
    try {
      await updateGameStatus.mutateAsync({ gameId, status: 'backlog' });
      showToast(`Added "${gameName}" to your backlog`, 'success');
    } catch {
      showToast('Failed to add to backlog', 'error');
    }
  };

  const renderActions = (item: CollectionItemDto) => (
    <>
      {!item.status && (
        <button
          onClick={(e) => {
            e.preventDefault();
            handleAddToBacklog(item.gameId, item.gameName);
          }}
          className="btn btn-secondary btn-sm"
          disabled={updateGameStatus.isPending}
        >
          Add to Backlog
        </button>
      )}
      {item.status === 'backlog' && (
        <button
          onClick={(e) => {
            e.preventDefault();
            updateGameStatus.mutateAsync({ gameId: item.gameId, status: 'playing' }).then(() =>
              showToast(`Started playing "${item.gameName}"`, 'success')
            ).catch(() => showToast('Failed to update status', 'error'));
          }}
          className="btn btn-secondary btn-sm"
          disabled={updateGameStatus.isPending}
        >
          Start Playing
        </button>
      )}
      {item.status === 'playing' && (
        <button
          onClick={(e) => {
            e.preventDefault();
            updateGameStatus.mutateAsync({ gameId: item.gameId, status: 'completed' }).then(() =>
              showToast(`Marked "${item.gameName}" as completed`, 'success')
            ).catch(() => showToast('Failed to update status', 'error'));
          }}
          className="btn btn-secondary btn-sm"
          disabled={updateGameStatus.isPending}
        >
          Mark Completed
        </button>
      )}
      <button
        onClick={(e) => {
          e.preventDefault();
          handleRemove(item.gameId, item.gameName);
        }}
        className="btn btn-danger btn-sm"
        disabled={removeFromCollection.isPending}
      >
        Remove from Collection
      </button>
    </>
  );

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading your collection...</p>
      </div>
    );
  }

  const isFiltered = !!debouncedSearch || playStatus !== 'all' || sourceFilter !== 'all' || gameStatus !== 'all';

  return (
    <div className="collection-page">
      <header className="collection-header">
        <h1>My Collection</h1>
        <p className="collection-subtitle">
          All the games in your personal library
        </p>
      </header>

      <SteamSection />
      <TwitchSection />
      <RetroArchSection />

      {totalGames > 0 ? (
        <>
          <CollectionStats stats={stats} basePath="/collection" />
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
            resultCount={isFiltered ? totalFiltered : totalGames}
            totalCount={totalGames}
          />
          {allItems.length > 0 ? (
            <>
              <GameGrid
                games={allItems}
                showPlaytime
                renderActions={(item) => renderActions(item as CollectionItemDto)}
              />
              <div ref={sentinelRef} style={{ height: 1 }} />
              {isFetchingNextPage && (
                <div className="loading-container" style={{ padding: '1rem' }}>
                  <div className="loading-spinner" />
                </div>
              )}
            </>
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
