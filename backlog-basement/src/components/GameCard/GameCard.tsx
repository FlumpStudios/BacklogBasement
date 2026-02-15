import { useState } from 'react';
import { Link } from 'react-router-dom';
import { GameDto } from '../../types';
import { formatPlaytime, getYear } from '../../utils';
import './GameCard.css';

interface GameCardProps {
  game: GameDto;
  playtime?: number;
  showPlaytime?: boolean;
  criticScore?: number | null;
  actions?: React.ReactNode;
}

function getScoreColor(score: number): string {
  if (score >= 75) return 'score-green';
  if (score >= 50) return 'score-yellow';
  return 'score-red';
}

// Check if a cover URL is valid (not empty or malformed)
function isValidCoverUrl(url?: string | null): boolean {
  if (!url) return false;
  // Check for malformed IGDB URLs like "...t_cover_big/.jpg"
  return !url.endsWith('/.jpg') && !url.endsWith('/.png');
}

export function GameCard({ game, playtime, showPlaytime = false, criticScore, actions }: GameCardProps) {
  const releaseYear = getYear(game.releaseDate ?? undefined);
  const hasCover = isValidCoverUrl(game.coverUrl);
  const [imageError, setImageError] = useState(false);

  const showPlaceholder = !hasCover || imageError;

  return (
    <div className="game-card">
      <Link to={`/games/${game.id}`} className="game-card-link">
        <div className="game-card-cover">
          {!showPlaceholder ? (
            <img
              src={game.coverUrl!}
              alt={game.name}
              loading="lazy"
              className="game-card-image"
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="game-card-placeholder">
              <span>🎮</span>
            </div>
          )}
          {criticScore != null && (
            <span className={`game-card-score ${getScoreColor(criticScore)}`}>
              {criticScore}
            </span>
          )}
        </div>
        <div className="game-card-info">
          <h3 className="game-card-title">{game.name}</h3>
          {releaseYear && (
            <span className="game-card-year">{releaseYear}</span>
          )}
          {showPlaytime && playtime !== undefined && playtime > 0 && (
            <span className="game-card-playtime">
              ⏱️ {formatPlaytime(playtime)}
            </span>
          )}
        </div>
      </Link>
      {actions && <div className="game-card-actions">{actions}</div>}
    </div>
  );
}
