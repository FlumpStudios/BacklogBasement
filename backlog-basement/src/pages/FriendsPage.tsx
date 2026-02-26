import { useAuth } from '../auth';
import { useSteamFriendSuggestions } from '../hooks';
import { FriendButton } from '../components';
import { PlayerSearch, FriendsList } from '../features/friends';
import './FriendsPage.css';

export function FriendsPage() {
  const { user } = useAuth();
  const { data, isFetching, refetch, isFetched } = useSteamFriendSuggestions();

  return (
    <div className="friends-page">
      <h1>Friends</h1>

      <section className="friends-page-search">
        <h2>Find Players</h2>
        <PlayerSearch />
      </section>

      {user?.hasSteamLinked && (
        <section className="friends-page-steam">
          <h2>Find Steam Friends</h2>
          <p className="steam-suggestions-hint">
            See which of your Steam friends are already on Backlog Basement.
          </p>
          <button
            className="btn btn-secondary"
            onClick={() => refetch()}
            disabled={isFetching}
          >
            {isFetching ? 'Loading...' : 'Find Steam Friends'}
          </button>

          {isFetched && !isFetching && data && (
            <div className="steam-suggestions-results">
              {data.isPrivate ? (
                <p className="steam-suggestions-empty">
                  Your Steam friends list is set to private. You can change this in your Steam privacy settings.
                </p>
              ) : data.suggestions.length === 0 ? (
                <p className="steam-suggestions-empty">
                  None of your Steam friends are on Backlog Basement yet.
                </p>
              ) : (
                <ul className="steam-suggestions-list">
                  {data.suggestions.map((player) => (
                    <li key={player.userId} className="steam-suggestion-item">
                      <div className="steam-suggestion-info">
                        <span className="steam-suggestion-name">{player.displayName}</span>
                        <span className="steam-suggestion-username">@{player.username}</span>
                      </div>
                      <FriendButton userId={player.userId} />
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </section>
      )}

      <section className="friends-page-list">
        <FriendsList />
      </section>
    </div>
  );
}
