import { useState } from 'react';
import { Link } from 'react-router-dom';
import { usePlayerSearch } from '../../hooks';
import './PlayerSearch.css';

export function PlayerSearch() {
  const [query, setQuery] = useState('');
  const { data: results, isLoading } = usePlayerSearch(query);

  return (
    <div className="player-search">
      <div className="player-search-input-wrapper">
        <input
          type="text"
          className="player-search-input"
          placeholder="Search players by username or display name..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
        {isLoading && <span className="player-search-spinner" />}
      </div>

      {query.length >= 2 && results && results.length > 0 && (
        <ul className="player-search-results">
          {results.map((player) => (
            <li key={player.userId} className="player-search-result">
              <Link to={`/profile/${player.username}`} className="player-search-link">
                <div className="player-search-info">
                  <span className="player-search-display-name">{player.displayName}</span>
                  <span className="player-search-username">@{player.username}</span>
                </div>
                <span className="player-search-games">{player.totalGames} games</span>
              </Link>
            </li>
          ))}
        </ul>
      )}

      {query.length >= 2 && results && results.length === 0 && !isLoading && (
        <div className="player-search-empty">No players found</div>
      )}
    </div>
  );
}
