import { useFeaturedGames } from '../../hooks/useFeaturedGames';
import { GameCard } from '../GameCard/GameCard';
import './IndieSpotlight.css';

export function IndieSpotlight() {
  const { data: games } = useFeaturedGames();

  if (!games || games.length === 0) return null;

  return (
    <section className="indie-spotlight">
      <div className="indie-spotlight-header">
        <div>
          <h2 className="indie-spotlight-title">✨ Indie Spotlight</h2>
          <p className="indie-spotlight-subtitle">Hand-picked indie games worth your time</p>
        </div>
      </div>
      <div className="indie-spotlight-grid">
        {games.map((game) => (
          <GameCard key={game.id} game={game} criticScore={game.criticScore} />
        ))}
      </div>
    </section>
  );
}
