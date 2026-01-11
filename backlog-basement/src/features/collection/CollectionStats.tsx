import { CollectionItemDto } from '../../types';
import { formatPlaytime } from '../../utils';
import './CollectionStats.css';

interface CollectionStatsProps {
  collection: CollectionItemDto[];
}

export function CollectionStats({ collection }: CollectionStatsProps) {
  const totalGames = collection.length;
  const totalPlaytime = collection.reduce(
    (sum, item) => sum + (item.totalPlayTimeMinutes || 0),
    0
  );
  const gamesPlayed = collection.filter(
    (item) => (item.totalPlayTimeMinutes || 0) > 0
  ).length;

  return (
    <div className="collection-stats">
      <div className="stat-card">
        <span className="stat-value">{totalGames}</span>
        <span className="stat-label">Games</span>
      </div>
      <div className="stat-card">
        <span className="stat-value">{gamesPlayed}</span>
        <span className="stat-label">Played</span>
      </div>
      <div className="stat-card">
        <span className="stat-value">{formatPlaytime(totalPlaytime)}</span>
        <span className="stat-label">Total Time</span>
      </div>
    </div>
  );
}
