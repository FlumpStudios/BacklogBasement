import { Link } from 'react-router-dom';
import { useCollection, useRemoveFromCollection } from '../hooks';
import { CollectionStats } from '../features/collection';
import { GameGrid } from '../features/games';
import { EmptyState, useToast, SteamSection } from '../components';
import { CollectionItemDto } from '../types';
import './CollectionPage.css';

export function CollectionPage() {
  const { data: collection, isLoading } = useCollection();
  const removeFromCollection = useRemoveFromCollection();
  const { showToast } = useToast();

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
          <GameGrid
            games={collection}
            showPlaytime
            renderActions={(item) => renderActions(item as CollectionItemDto)}
          />
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
