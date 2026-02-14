import { useState, useCallback } from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth';
import { ThemeToggle } from '../ThemeToggle';
import { NotificationBell } from '../NotificationBell';
import { CookieBanner } from '../CookieBanner';
import { UsernameSetupModal } from '../../features/profile';
import './Layout.css';

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);

  const closeMenu = useCallback(() => setMenuOpen(false), []);

  const handleLogout = async () => {
    closeMenu();
    await logout();
    navigate('/');
  };

  return (
    <div className="layout">
      <header className="header">
        <div className="header-content">
          <Link to="/" className="logo" onClick={closeMenu}>
            <span className="logo-icon">🎮</span>
            <span className="logo-text">Backlog Basement</span>
          </Link>

          <nav className="nav">
            {isAuthenticated ? (
              <>
                <div className={`mobile-drawer ${menuOpen ? 'open' : ''}`}>
                  <Link to="/dashboard" className="nav-link" onClick={closeMenu}>
                    Dashboard
                  </Link>
                  <Link to="/collection" className="nav-link" onClick={closeMenu}>
                    Collection
                  </Link>
                  <Link to="/search" className="nav-link" onClick={closeMenu}>
                    Search
                  </Link>
                  {user?.username && (
                    <Link to={`/profile/${user.username}`} className="nav-link" onClick={closeMenu}>
                      My Profile
                    </Link>
                  )}
                  <Link to="/friends" className="nav-link" onClick={closeMenu}>
                    Friends
                  </Link>
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
                </div>
                {menuOpen && <div className="drawer-backdrop" onClick={closeMenu} />}
                <ThemeToggle />
                <NotificationBell />
                <button
                  className="burger-btn"
                  onClick={() => setMenuOpen(!menuOpen)}
                  aria-label={menuOpen ? 'Close menu' : 'Open menu'}
                >
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
                    {menuOpen ? (
                      <>
                        <line x1="6" y1="6" x2="18" y2="18" />
                        <line x1="6" y1="18" x2="18" y2="6" />
                      </>
                    ) : (
                      <>
                        <line x1="3" y1="6" x2="21" y2="6" />
                        <line x1="3" y1="12" x2="21" y2="12" />
                        <line x1="3" y1="18" x2="21" y2="18" />
                      </>
                    )}
                  </svg>
                </button>
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
