import { Link } from 'react-router-dom';
import { useAuth } from '../auth';
import './LandingPage.css';

export function LandingPage() {
  const { isAuthenticated, login } = useAuth();

  return (
    <div className="landing-page">
      <section className="hero">
        <h1 className="hero-title">
          Your Gaming Backlog,
          <span className="hero-title-accent">Organized</span>
        </h1>
        <p className="hero-subtitle">
          Track your game collection, log your playtime, and finally tackle that
          backlog. Powered by the IGDB database.
        </p>
        <div className="hero-actions">
          {isAuthenticated ? (
            <Link to="/dashboard" className="btn btn-primary btn-lg">
              Go to Dashboard
            </Link>
          ) : (
            <button onClick={login} className="btn btn-primary btn-lg">
              <span>🎮</span> Get Started with Google
            </button>
          )}
        </div>
      </section>

      <section className="features">
        <div className="feature-card">
          <span className="feature-icon">🔍</span>
          <h3 className="feature-title">Discover Games</h3>
          <p className="feature-description">
            Search through thousands of games using the IGDB database. Find that
            hidden gem you've been looking for.
          </p>
        </div>

        <div className="feature-card">
          <span className="feature-icon">📚</span>
          <h3 className="feature-title">Build Your Collection</h3>
          <p className="feature-description">
            Keep track of all the games you own or want to play. Your personal
            gaming library, always accessible.
          </p>
        </div>

        <div className="feature-card">
          <span className="feature-icon">⏱️</span>
          <h3 className="feature-title">Log Playtime</h3>
          <p className="feature-description">
            Record your gaming sessions and see how much time you've invested in
            each title.
          </p>
        </div>
      </section>
    </div>
  );
}
