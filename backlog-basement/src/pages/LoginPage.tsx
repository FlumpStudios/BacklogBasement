import { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../auth';
import './LoginPage.css';

export function LoginPage() {
  const { isAuthenticated, isLoading, login, loginWithSteam, loginWithTwitch } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const from = (location.state as { from?: Location })?.from?.pathname || '/dashboard';

  useEffect(() => {
    if (isAuthenticated && !isLoading) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate, from]);

  if (isLoading) {
    return (
      <div className="login-page">
        <div className="loading-spinner" />
        <p>Checking authentication...</p>
      </div>
    );
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <span className="login-icon">🎮</span>
          <h1 className="login-title">Welcome to Backlog Basement</h1>
          <p className="login-subtitle">
            Sign in to start managing your game collection
          </p>
        </div>

        <button onClick={loginWithSteam} className="btn btn-steam">
          <svg viewBox="0 0 24 24" width="20" height="20">
            <path fill="currentColor" d="M12 2a10 10 0 0 0-9.96 9.04l5.35 2.21a2.83 2.83 0 0 1 1.6-.49l2.39-3.47v-.05a3.77 3.77 0 1 1 3.77 3.77h-.09l-3.41 2.43a2.84 2.84 0 0 1-5.65.36l-3.83-1.58A10 10 0 1 0 12 2zm-4.99 15.57l-1.22-.5a2.13 2.13 0 0 0 3.87.57 2.13 2.13 0 0 0-1.14-2.78l1.26.52a1.56 1.56 0 1 1-2.77 2.19zm8.63-5.56a2.51 2.51 0 1 0-2.51-2.51 2.51 2.51 0 0 0 2.51 2.51z"/>
          </svg>
          Sign in with Steam
        </button>

        <button onClick={loginWithTwitch} className="btn btn-twitch">
          <svg viewBox="0 0 24 24" width="20" height="20">
            <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714z"/>
          </svg>
          Sign in with Twitch
        </button>

        <div className="login-divider">or</div>

        <button onClick={login} className="btn btn-google">
          <svg viewBox="0 0 24 24" className="google-icon">
            <path
              fill="#4285F4"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            />
            <path
              fill="#34A853"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="#FBBC05"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="#EA4335"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
          Sign in with Google
        </button>

        <p className="login-disclaimer">
          By signing in, you agree to let us store your game collection data.
        </p>
      </div>
    </div>
  );
}
