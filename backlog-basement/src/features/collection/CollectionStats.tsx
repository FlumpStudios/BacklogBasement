import { Link } from 'react-router-dom';
import { CollectionItemDto, CollectionStatsDto } from '../../types';
import './CollectionStats.css';

interface CollectionStatsProps {
  collection?: CollectionItemDto[];
  stats?: CollectionStatsDto;
  basePath?: string;
}

export function CollectionStats({ collection, stats, basePath }: CollectionStatsProps) {
  const totalGames = stats?.totalGames ?? collection?.length ?? 0;
  const gamesBacklog = stats?.gamesBacklog ?? collection?.filter(i => i.status === 'backlog').length ?? 0;
  const gamesCompleted = stats?.gamesCompleted ?? collection?.filter(i => i.status === 'completed').length ?? 0;

  const cards = [
    { value: totalGames, label: 'Games', to: basePath ? `${basePath}?reset=true` : null },
    { value: gamesBacklog, label: 'Backlog', to: basePath ? `${basePath}?status=backlog` : null },
    { value: gamesCompleted, label: 'Completed', to: basePath ? `${basePath}?status=completed` : null },
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
