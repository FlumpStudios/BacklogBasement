import { useState } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../auth';
import { useDebounce } from '../hooks';
import { useFeaturedGames, useAddFeatured, useRemoveFeatured } from '../hooks/useFeaturedGames';
import { featuredApi } from '../api/featured';
import { useQuery } from '@tanstack/react-query';
import { useToast } from '../components';
import { GameDto } from '../types';
import './AdminPage.css';

export function AdminPage() {
  const { user } = useAuth();
  const { showToast } = useToast();
  const [searchQuery, setSearchQuery] = useState('');
  const debouncedQuery = useDebounce(searchQuery, 300);

  const { data: featured } = useFeaturedGames();
  const addFeatured = useAddFeatured();
  const removeFeatured = useRemoveFeatured();

  const { data: searchResults } = useQuery({
    queryKey: ['admin-game-search', debouncedQuery],
    queryFn: () => featuredApi.searchGames(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
  });

  if (!user?.isAdmin) return <Navigate to="/dashboard" replace />;

  const featuredIds = new Set(featured?.map(g => g.id) ?? []);

  const handleAdd = async (game: GameDto) => {
    try {
      await addFeatured.mutateAsync(game.id);
      showToast(`Added "${game.name}" to Indie Spotlight`, 'success');
    } catch (err: any) {
      showToast(err?.response?.data?.message ?? 'Failed to add game', 'error');
    }
  };

  const handleRemove = async (game: GameDto) => {
    try {
      await removeFeatured.mutateAsync(game.id);
      showToast(`Removed "${game.name}" from Indie Spotlight`, 'success');
    } catch {
      showToast('Failed to remove game', 'error');
    }
  };

  return (
    <div className="admin-page">
      <h1>Admin — Indie Spotlight</h1>
      <p className="admin-subtitle">Manage up to 5 featured games shown at the top of the dashboard.</p>

      <section className="admin-section">
        <h2>Current Spotlight ({featured?.length ?? 0}/5)</h2>
        {featured && featured.length > 0 ? (
          <ul className="admin-featured-list">
            {featured.map((game) => (
              <li key={game.id} className="admin-featured-item">
                {game.coverUrl && (
                  <img src={game.coverUrl} alt={game.name} className="admin-game-cover" />
                )}
                <span className="admin-game-name">{game.name}</span>
                {game.steamAppId && (
                  <span className="admin-game-meta">Steam {game.steamAppId}</span>
                )}
                <button
                  className="btn btn-danger btn-sm"
                  onClick={() => handleRemove(game)}
                  disabled={removeFeatured.isPending}
                >
                  Remove
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="admin-empty">No games featured yet.</p>
        )}
      </section>

      <section className="admin-section">
        <h2>Add a Game</h2>
        <input
          className="admin-search-input"
          type="text"
          placeholder="Search by name..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          autoFocus
        />
        {searchResults && searchResults.length > 0 && (
          <ul className="admin-search-results">
            {searchResults.map((game) => (
              <li key={game.id} className="admin-search-item">
                {game.coverUrl && (
                  <img src={game.coverUrl} alt={game.name} className="admin-game-cover" />
                )}
                <span className="admin-game-name">{game.name}</span>
                {game.steamAppId && (
                  <span className="admin-game-meta">Steam {game.steamAppId}</span>
                )}
                <button
                  className="btn btn-primary btn-sm"
                  onClick={() => handleAdd(game)}
                  disabled={featuredIds.has(game.id) || (featured?.length ?? 0) >= 5 || addFeatured.isPending}
                >
                  {featuredIds.has(game.id) ? 'Featured' : '+ Add'}
                </button>
              </li>
            ))}
          </ul>
        )}
        {debouncedQuery.length >= 2 && searchResults?.length === 0 && (
          <p className="admin-empty">No games found. The game must already be in the database (i.e. someone has searched for it).</p>
        )}
      </section>
    </div>
  );
}
