import { Link } from 'react-router-dom';
import { CollectionItemDto } from '../../types';
import { formatPlaytime } from '../../utils';
import './CollectionStats.css';

interface CollectionStatsProps {
  collection: CollectionItemDto[];
  basePath?: string;
}

export function CollectionStats({ collection, basePath }: CollectionStatsProps) {
  const totalGames = collection.length;
  const totalPlaytime = collection.reduce(
    (sum, item) => sum + (item.totalPlayTimeMinutes || 0),
    0
  );
  const gamesPlayed = collection.filter(
    (item) => (item.totalPlayTimeMinutes || 0) > 0
  ).length;

  const cards = [
    { value: totalGames, label: 'Games', to: basePath ? `${basePath}?reset=true` : null },
    { value: gamesPlayed, label: 'Played', to: basePath ? `${basePath}?playStatus=played` : null },
    { value: formatPlaytime(totalPlaytime), label: 'Total Time', to: basePath ? `${basePath}?sort=playtime-desc` : null },
  ];

  return (
    <div className="collection-stats">
      {cards.map(({ value, label, to }) =>
        to ? (
          <Link key={label} to={to} className="stat-card stat-card-link">
            <span className="stat-value">{value}</span>
            <span className="stat-label">{label}</span>
          </Link>
        ) : (
          <div key={label} className="stat-card">
            <span className="stat-value">{value}</span>
            <span className="stat-label">{label}</span>
          </div>
        )
      )}
    </div>
  );
}
