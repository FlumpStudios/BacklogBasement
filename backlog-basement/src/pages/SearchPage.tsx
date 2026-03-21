import { useState } from 'react';
import { SearchInput, EmptyState, useToast } from '../components';
import { GameGrid } from '../features/games';
import { useGameSearch, useAddToCollection, useCollection, useRemoveFromCollection, useUpdateGameStatus } from '../hooks';
import { GameDto, CollectionItemDto } from '../types';
import './SearchPage.css';

export function SearchPage() {
  const [query, setQuery] = useState('');
  const { data: searchResults, isLoading, isError } = useGameSearch(query);
  const { data: collection } = useCollection();
  const addToCollection = useAddToCollection();
  const removeFromCollection = useRemoveFromCollection();
  const updateGameStatus = useUpdateGameStatus();
  const { showToast } = useToast();

  const collectionMap = new Map<string, CollectionItemDto>(
    collection?.map((item) => [item.gameId, item]) ?? []
  );

  const handleAddToCollection = async (gameId: string, gameName: string) => {
    try {
      await addToCollection.mutateAsync(gameId);
      showToast(`Added "${gameName}" to your collection!`, 'success');
    } catch {
      showToast('Failed to add game to collection', 'error');
    }
  };

  const handleRemove = async (gameId: string, gameName: string) => {
    try {
      await removeFromCollection.mutateAsync(gameId);
      showToast(`Removed "${gameName}" from your collection`, 'success');
    } catch {
      showToast('Failed to remove game', 'error');
    }
  };

  const renderActions = (game: GameDto) => {
    const item = collectionMap.get(game.id);

    if (!item) {
      return (
        <button
          onClick={() => handleAddToCollection(game.id, game.name)}
          className="btn btn-primary btn-sm"
          disabled={addToCollection.isPending}
        >
          + Add
        </button>
      );
    }

    return (
      <>
        {!item.status && (
          <button
            onClick={(e) => {
              e.preventDefault();
              updateGameStatus.mutateAsync({ gameId: item.gameId, status: 'backlog' }).then(() =>
                showToast(`Added "${item.gameName}" to your backlog`, 'success')
              ).catch(() => showToast('Failed to add to backlog', 'error'));
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
          Remove
        </button>
      </>
    );
  };

  return (
    <div className="search-page">
      <header className="search-header">
        <h1>Search Games</h1>
        <p className="search-subtitle">
          Find games from the IGDB database and add them to your collection
        </p>
      </header>

      <div className="search-container">
        <SearchInput
          value={query}
          onChange={setQuery}
          placeholder="Search for games..."
          isLoading={isLoading}
          autoFocus
        />
      </div>

      <div className="search-results">
        {isError && (
          <EmptyState
            icon="❌"
            title="Search failed"
            description="Something went wrong. Please try again."
          />
        )}

        {!query && (
          <EmptyState
            icon="🔍"
            title="Start searching"
            description="Type at least 2 characters to search for games"
          />
        )}

        {query && query.length < 2 && (
          <EmptyState
            icon="✍️"
            title="Keep typing..."
            description="Type at least 2 characters to search"
          />
        )}

        {query.length >= 2 && !isLoading && searchResults?.length === 0 && (
          <EmptyState
            icon="🎮"
            title="No games found"
            description={`No results for "${query}". Try a different search term.`}
          />
        )}

        {searchResults && searchResults.length > 0 && (
          <>
            <p className="results-count">
              Found {searchResults.length} game{searchResults.length !== 1 ? 's' : ''}
            </p>
            <GameGrid
              games={searchResults}
              renderActions={(game) => renderActions(game as GameDto)}
            />
          </>
        )}
      </div>
    </div>
  );
}
