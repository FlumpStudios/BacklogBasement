import { useState, useCallback, useEffect } from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../auth';
import { useMyClubs, useSteamAutoSync } from '../../hooks';
import { NotificationBell } from '../NotificationBell';
import { InboxBell } from '../InboxBell';
import { CookieBanner } from '../CookieBanner';
import { Avatar } from '../Avatar';
import { SteamImportPrompt } from '../SteamImportPrompt/SteamImportPrompt';
import { UsernameSetupModal } from '../../features/profile';
import { useTheme } from '../../contexts/ThemeContext';
import { useToast } from '../Toast/ToastContext';
import './Layout.css';


export function Layout() {
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const { retroMode, cycleRetro } = useTheme();
  const { showToast } = useToast();
  const retroLabel = retroMode === 'c64' ? 'CRT: C64' : retroMode === 'bbc' ? 'CRT: BBC' : 'CRT: Spectrum';

  const handleCycleRetro = () => {
    cycleRetro();
    const next = retroMode === 'c64' ? 'BBC Micro' : retroMode === 'bbc' ? 'ZX Spectrum' : 'C64';
    showToast(`📺 Switched to ${next}`, 'info');
  };
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const { data: myClubs } = useMyClubs(isAuthenticated);
  const hasClubAction = myClubs?.some(c => {
    const r = c.currentRound;
    if (!r) return false;
    return (r.status === 'voting' && !r.userHasVoted) ||
      (r.status === 'reviewing' && !r.userHasReviewed) ||
      (r.status === 'nominating' && !r.userHasNominated);
  }) ?? false;
  const [onboardingActive, setOnboardingActive] = useState(false);
  const [showImportPrompt, setShowImportPrompt] = useState(false);

  useSteamAutoSync(
    !!user?.hasSteamLinked,
    !!user?.username,
    onboardingActive || showImportPrompt,
  );

  useEffect(() => {
    if (isAuthenticated && !isLoading && !user?.username) {
      localStorage.removeItem('backlog_onboarding');
      setOnboardingActive(true);
    }
  }, [isAuthenticated, isLoading, user?.username]);

  useEffect(() => {
    if (user?.username && localStorage.getItem('backlog_onboarding') === 'import') {
      localStorage.removeItem('backlog_onboarding');
      setShowImportPrompt(true);
    }
  }, [user?.username]);

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
                  <Link to="/friends" className="nav-link" onClick={closeMenu}>
                    Friends
                  </Link>
                  <span className="nav-link-wrapper">
                    <Link to="/clubs" className="nav-link" onClick={closeMenu}>
                      Clubs
                    </Link>
                    {hasClubAction && <span className="nav-badge-dot" />}
                  </span>
                  <button
                    className={`drawer-retro-toggle${retroMode !== 'off' ? ` retro-active retro-${retroMode}` : ''}`}
                    onClick={handleCycleRetro}
                  >
                    📺 {retroLabel}
                  </button>
                  <div className="user-menu">
                    {user?.username ? (
                      <Link to={`/profile/${user.username}`} className="nav-avatar-link" onClick={closeMenu}>
                        <Avatar avatarUrl={user.avatarUrl} displayName={user.displayName} userId={user.id} size="sm" />
                      </Link>
                    ) : (
                      <Avatar avatarUrl={user?.avatarUrl} displayName={user?.displayName ?? ''} userId={user?.id} size="sm" />
                    )}
                    {user?.username ? (
                      <Link to={`/profile/${user.username}`} className="user-name" onClick={closeMenu}>
                        {user.username}
                        {user?.xpInfo && (
                          <span className="nav-level-badge" title={user.xpInfo.levelName}>
                            Lv.{user.xpInfo.level}
                          </span>
                        )}
                      </Link>
                    ) : (
                      <span className="user-name">{user?.displayName}</span>
                    )}
                    <button onClick={handleLogout} className="btn btn-secondary btn-sm">
                      Logout
                    </button>
                  </div>
                </div>
                {menuOpen && <div className="drawer-backdrop" onClick={closeMenu} />}
                <button
                  className={`retro-toggle retro-active retro-${retroMode}`}
                  onClick={handleCycleRetro}
                  title={retroLabel}
                  aria-label={retroLabel}
                >
                  📺
                </button>
                <InboxBell />
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
                <button
                  className={`retro-toggle retro-active retro-${retroMode}`}
                  onClick={handleCycleRetro}
                  title={retroLabel}
                  aria-label={retroLabel}
                >
                  📺
                </button>
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
      {onboardingActive && <UsernameSetupModal onComplete={() => {
        setOnboardingActive(false);
        if (localStorage.getItem('backlog_onboarding') === 'import') {
          localStorage.removeItem('backlog_onboarding');
          setShowImportPrompt(true);
        }
      }} />}
      {showImportPrompt && <SteamImportPrompt onClose={() => setShowImportPrompt(false)} />}
    </div>
  );
}
