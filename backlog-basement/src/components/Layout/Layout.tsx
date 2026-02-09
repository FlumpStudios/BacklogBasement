import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth';
import { ThemeToggle } from '../ThemeToggle';
import { CookieBanner } from '../CookieBanner';
import { UsernameSetupModal } from '../../features/profile';
import './Layout.css';

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  return (
    <div className="layout">
      <header className="header">
        <div className="header-content">
          <Link to="/" className="logo">
            <span className="logo-icon">🎮</span>
            <span className="logo-text">Backlog Basement</span>
          </Link>

          <nav className="nav">
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="nav-link">
                  Dashboard
                </Link>
                <Link to="/collection" className="nav-link">
                  Collection
                </Link>
                <Link to="/search" className="nav-link">
                  Search
                </Link>
                {user?.username && (
                  <Link to={`/profile/${user.username}`} className="nav-link">
                    My Profile
                  </Link>
                )}
                <ThemeToggle />
                <div className="user-menu">
                  {user?.avatarUrl && (
                    <img
                      src={user.avatarUrl}
                      alt={user.displayName}
                      className="user-avatar"
                    />
                  )}
                  <span className="user-name">{user?.displayName}</span>
                  <button onClick={handleLogout} className="btn btn-secondary btn-sm">
                    Logout
                  </button>
                </div>
              </>
            ) : (
              <>
                <ThemeToggle />
                <Link to="/login" className="btn btn-primary">
                  Sign In
                </Link>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="main">
        <Outlet />
      </main>

      <footer className="footer">
        <p>&copy; {new Date().getFullYear()} Backlog Basement. Powered by IGDB.</p>
        <div className="footer-links">
          <Link to="/privacy" className="footer-link">Privacy Policy</Link>
          <span className="footer-separator">|</span>
          <Link to="/cookies" className="footer-link">Cookie Policy</Link>
        </div>
      </footer>

      <CookieBanner />
      {isAuthenticated && !user?.username && <UsernameSetupModal />}
    </div>
  );
}
