import { useState } from 'react';
import { Modal } from '../../components';
import { useCheckUsername, useSetUsername } from '../../hooks';
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

export function UsernameSetupModal() {
  const [username, setUsername] = useState('');
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
    setUsernameMutation.mutate(username);
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
      title="Choose your username"
      dismissible={false}
    >
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
    </Modal>
  );
}
