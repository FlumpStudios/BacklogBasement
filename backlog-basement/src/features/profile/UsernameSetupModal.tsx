import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Modal } from '../../components';
import { useCheckUsername, useSetUsername, useTwitchImport } from '../../hooks';
import { useAuth } from '../../auth';
import { steamApi } from '../../api';
import { TwitchImportResultDto } from '../../types';
import './UsernameSetupModal.css';

const USERNAME_REGEX = /^[a-zA-Z0-9][a-zA-Z0-9_-]*[a-zA-Z0-9]$/;

function getClientValidationError(username: string): string | null {
  if (username.length === 0) return null;
  if (username.length < 3) return 'Username must be at least 3 characters';
  if (username.length > 30) return 'Username must be at most 30 characters';
  if (!USERNAME_REGEX.test(username))
    return 'Only letters, numbers, hyphens, and underscores allowed. Cannot start or end with a hyphen or underscore.';
  return null;
}

interface Props {
  onComplete: () => void;
}

export function UsernameSetupModal({ onComplete }: Props) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [step, setStep] = useState<'username' | 'steam' | 'twitch'>('username');
  const [twitchImportResult, setTwitchImportResult] = useState<TwitchImportResultDto | null>(null);
  const twitchImport = useTwitchImport();
  const clientError = getClientValidationError(username);
  const { data: availability, isFetching: isChecking } = useCheckUsername(
    clientError ? '' : username
  );
  const setUsernameMutation = useSetUsername();

  const isValid =
    username.length >= 3 &&
    !clientError &&
    availability?.available === true &&
    !isChecking;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!isValid) return;
    setUsernameMutation.mutate(username, {
      onSuccess: () => {
        if (user?.hasSteamLinked) {
          localStorage.setItem('backlog_onboarding', 'import');
          onComplete();
        } else if (user?.hasTwitchLinked) {
          setStep('twitch');
        } else {
          setStep('steam');
        }
      },
    });
  };

  const getStatusMessage = () => {
    if (username.length === 0) return null;
    if (clientError) return { text: clientError, type: 'error' as const };
    if (username.length < 3) return null;
    if (isChecking) return { text: 'Checking availability...', type: 'checking' as const };
    if (availability?.available === true) return { text: 'Username is available!', type: 'success' as const };
    if (availability?.available === false) return { text: 'Username is not available', type: 'error' as const };
    return null;
  };

  const status = getStatusMessage();

  return (
    <Modal
      isOpen={true}
      onClose={() => {}}
      title={step === 'username' ? 'Choose your username' : step === 'twitch' ? 'Import your stream history?' : 'Link your Steam account?'}
      dismissible={false}
    >
      {step === 'username' ? (
        <form onSubmit={handleSubmit} className="username-setup-form">
          <p className="username-setup-description">
            Pick a unique username for your profile. This will be used in your
            public profile URL and cannot be changed later.
          </p>

          <div className="username-input-group">
            <span className="username-prefix">backlogbasement.com/profile/</span>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value.trim())}
              placeholder="your-username"
              className="username-input"
              maxLength={30}
              autoFocus
            />
          </div>

          {status && (
            <p className={`username-status username-status--${status.type}`}>
              {status.text}
            </p>
          )}

          {setUsernameMutation.isError && (
            <p className="username-status username-status--error">
              {(setUsernameMutation.error as any)?.response?.data?.message ||
                'Failed to set username. Please try again.'}
            </p>
          )}

          <button
            type="submit"
            className="btn btn-primary username-submit"
            disabled={!isValid || setUsernameMutation.isPending}
          >
            {setUsernameMutation.isPending ? 'Setting username...' : 'Confirm Username'}
          </button>
        </form>
      ) : step === 'steam' ? (
        <div className="steam-step">
          <div className="steam-step-icon">
            <svg viewBox="0 0 24 24" width="56" height="56">
              <path fill="currentColor" d="M12 2a10 10 0 0 0-9.96 9.04l5.35 2.21a2.83 2.83 0 0 1 1.6-.49l2.39-3.47v-.05a3.77 3.77 0 1 1 3.77 3.77h-.09l-3.41 2.43a2.84 2.84 0 0 1-5.65.36l-3.83-1.58A10 10 0 1 0 12 2zm-4.99 15.57l-1.22-.5a2.13 2.13 0 0 0 3.87.57 2.13 2.13 0 0 0-1.14-2.78l1.26.52a1.56 1.56 0 1 1-2.77 2.19zm8.63-5.56a2.51 2.51 0 1 0-2.51-2.51 2.51 2.51 0 0 0 2.51 2.51z"/>
            </svg>
          </div>
          <p className="steam-step-desc">
            Import your game library and sync your playtime automatically.
          </p>
          <p className="steam-step-note">
            Your Steam "Game details" must be set to <strong>Public</strong> in your{' '}
            <a href="https://steamcommunity.com/my/edit/settings" target="_blank" rel="noopener noreferrer">
              Steam privacy settings
            </a>{' '}
            for the import to work.
          </p>
          <button
            className="btn btn-steam"
            onClick={() => {
              localStorage.setItem('backlog_onboarding', 'import');
              window.location.href = steamApi.getLinkUrl();
            }}
          >
            <svg className="steam-icon" viewBox="0 0 24 24" width="20" height="20">
              <path fill="currentColor" d="M12 2a10 10 0 0 0-9.96 9.04l5.35 2.21a2.83 2.83 0 0 1 1.6-.49l2.39-3.47v-.05a3.77 3.77 0 1 1 3.77 3.77h-.09l-3.41 2.43a2.84 2.84 0 0 1-5.65.36l-3.83-1.58A10 10 0 1 0 12 2zm-4.99 15.57l-1.22-.5a2.13 2.13 0 0 0 3.87.57 2.13 2.13 0 0 0-1.14-2.78l1.26.52a1.56 1.56 0 1 1-2.77 2.19zm8.63-5.56a2.51 2.51 0 1 0-2.51-2.51 2.51 2.51 0 0 0 2.51 2.51z"/>
            </svg>
            Link Steam Account
          </button>
          <button className="onboarding-skip" onClick={onComplete}>
            Skip for now
          </button>
        </div>
      ) : step === 'twitch' ? (
        <div className="twitch-step">
          {!twitchImportResult ? (
            <>
              <div className="twitch-step-icon">
                <svg viewBox="0 0 24 24" width="56" height="56">
                  <path fill="#9147ff" d="M11.571 4.714h1.715v5.143H11.57zm4.715 0H18v5.143h-1.714zM6 0L1.714 4.286v15.428h5.143V24l4.286-4.286h3.428L22.286 12V0zm14.571 11.143l-3.428 3.428h-3.429l-3 3v-3H6.857V1.714h13.714z"/>
                </svg>
              </div>
              <p className="twitch-step-desc">
                You're signed in with Twitch! Import your stream history to add every game you've streamed to your collection, with streaming time recorded as playtime.
              </p>
              <p className="twitch-step-note">Up to 500 past broadcasts will be scanned. Note: Twitch only retains broadcast history for a limited time, so older streams may not appear (60 days for partners, 14 days for regular streamers).</p>
              <button
                className="btn btn-twitch"
                onClick={async () => {
                  const result = await twitchImport.mutateAsync();
                  setTwitchImportResult(result);
                }}
                disabled={twitchImport.isPending}
              >
                {twitchImport.isPending ? 'Importing...' : 'Import Stream History'}
              </button>
              <button className="onboarding-skip" onClick={onComplete}>
                Skip for now
              </button>
            </>
          ) : (
            <>
              <h4 className="import-complete-title">Import Complete!</h4>
              <div className="import-stats">
                <div className="stat">
                  <span className="stat-value">{twitchImportResult.total}</span>
                  <span className="stat-label">Games Found</span>
                </div>
                <div className="stat stat-success">
                  <span className="stat-value">{twitchImportResult.imported}</span>
                  <span className="stat-label">Imported</span>
                </div>
                <div className="stat stat-skipped">
                  <span className="stat-value">{twitchImportResult.skipped}</span>
                  <span className="stat-label">Already Owned</span>
                </div>
              </div>
              <div className="modal-actions">
                <button className="btn btn-primary" onClick={() => { navigate('/collection'); onComplete(); }}>
                  Go to Collection
                </button>
              </div>
            </>
          )}
        </div>
      ) : null}
    </Modal>
  );
}
