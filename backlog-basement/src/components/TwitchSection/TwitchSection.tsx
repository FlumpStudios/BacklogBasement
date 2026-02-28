import { useState } from 'react';
import { useAuth } from '../../auth';
import { authApi } from '../../api';
import { useQueryClient } from '@tanstack/react-query';
import { AUTH_QUERY_KEY } from '../../auth/AuthContext';
import { useToast } from '../Toast';
import './TwitchSection.css';

export function TwitchSection() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [unlinking, setUnlinking] = useState(false);
  const [isOpen, setIsOpen] = useState(!user?.hasTwitchLinked);

  const handleLink = () => {
    window.location.href = authApi.getTwitchLinkUrl();
  };

  const handleUnlink = async () => {
    if (!window.confirm('Are you sure you want to unlink your Twitch account?')) return;
    setUnlinking(true);
    try {
      await authApi.unlinkTwitch();
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
      showToast('Twitch account unlinked', 'success');
    } catch {
      showToast('Failed to unlink Twitch account', 'error');
    } finally {
      setUnlinking(false);
    }
  };

  return (
    <div className="twitch-section">
      <button className="twitch-header" onClick={() => setIsOpen(!isOpen)}>
        <div className="twitch-header-left">
          <h3>Twitch Integration</h3>
          {user?.hasTwitchLinked ? (
            <span className="twitch-header-status twitch-header-linked">
              <span className="twitch-status-icon">&#x2714;</span>
              Linked
            </span>
          ) : (
            <span className="twitch-header-status twitch-header-not-linked">
              Not linked
            </span>
          )}
        </div>
        <span className={`twitch-chevron ${isOpen ? 'open' : ''}`}>&#9662;</span>
      </button>

      <div className={`twitch-body ${isOpen ? 'open' : ''}`}>
        <div className="twitch-body-inner">
          {user?.hasTwitchLinked ? (
            <div className="twitch-linked">
              <p>Your Twitch account (<strong>{user.twitchId}</strong>) is linked. Live streams for games you view will be shown on game pages.</p>
              <button
                className="btn btn-secondary"
                onClick={handleUnlink}
                disabled={unlinking}
              >
                {unlinking ? 'Unlinking...' : 'Unlink Twitch'}
              </button>
            </div>
          ) : (
            <div className="twitch-not-linked">
              <p>Link your Twitch account to show live streams on game pages.</p>
              <button className="btn btn-twitch" onClick={handleLink}>
                <svg viewBox="0 0 24 24" width="18" height="18">
                  <path fill="currentColor" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714z"/>
                </svg>
                Link Twitch Account
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
