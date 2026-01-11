import { GameCard } from '../../components';
import { GameDto, CollectionItemDto } from '../../types';
import './GameGrid.css';

interface GameGridProps {
  games: GameDto[] | CollectionItemDto[];
  showPlaytime?: boolean;
  renderActions?: (game: GameDto | CollectionItemDto) => React.ReactNode;
}

function isCollectionItem(
  item: GameDto | CollectionItemDto
): item is CollectionItemDto {
  return 'gameName' in item;
}

export function GameGrid({ games, showPlaytime = false, renderActions }: GameGridProps) {
  return (
    <div className="game-grid">
      {games.map((item) => {
        // CollectionItemDto has flat structure, GameDto is already a game
        const gameForCard: GameDto = isCollectionItem(item) 
          ? { 
              id: item.gameId, 
              igdbId: 0,
              name: item.gameName, 
              coverUrl: item.coverUrl, 
              releaseDate: item.releaseDate 
            }
          : item;
        const playtime = isCollectionItem(item) ? item.totalPlayTimeMinutes : undefined;

        return (
          <GameCard
            key={isCollectionItem(item) ? item.id : item.id}
            game={gameForCard}
            playtime={playtime}
            showPlaytime={showPlaytime}
            actions={renderActions?.(item)}
          />
        );
      })}
    </div>
  );
}
