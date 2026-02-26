import { Link } from 'react-router-dom';
import { useAuth } from '../auth';
import './LandingPage.css';

export function LandingPage() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="landing-page">
      <section className="hero">
        <h1 className="hero-title">
          Your Gaming Life,
          <span className="hero-title-accent">Levelled Up</span>
        </h1>
        <p className="hero-subtitle">
          Manage your backlog, connect with friends, and game together.
          The social hub for every gamer's collection.
        </p>
        <div className="hero-actions">
          {isAuthenticated ? (
            <Link to="/dashboard" className="btn btn-primary btn-lg">
              Go to Dashboard
            </Link>
          ) : (
            <Link to="/login" className="btn btn-primary btn-lg">
              <span>🎮</span> Get Started
            </Link>
          )}
        </div>
      </section>

      <section className="features">
        <div className="feature-card">
          <span className="feature-icon">📚</span>
          <h3 className="feature-title">Manage Your Backlog</h3>
          <p className="feature-description">
            Track every game you own, want to play, or have completed. Log playtime, set statuses, and finally make a dent in that pile.
          </p>
        </div>

        <div className="feature-card">
          <span className="feature-icon">👥</span>
          <h3 className="feature-title">Social 1up</h3>
          <p className="feature-description">
            Add friends, swap game recommendations, and see what your crew is playing. Gaming is better together.
          </p>
        </div>

        <div className="feature-card">
          <span className="feature-icon">🎲</span>
          <h3 className="feature-title">Game Clubs</h3>
          <p className="feature-description">
            Vote on what to play next, track your progress together, and review games as a group.
          </p>
        </div>
      </section>
    </div>
  );
}
