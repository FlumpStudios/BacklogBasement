import { Link, useNavigate } from 'react-router-dom';
import {
  useFriends,
  usePendingRequests,
  useAcceptFriendRequest,
  useDeclineFriendRequest,
} from '../../hooks';
import { useToast } from '../../components/Toast';
import './FriendsList.css';

export function FriendsList() {
  const navigate = useNavigate();
  const { data: friends, isLoading: friendsLoading } = useFriends();
  const { data: requests, isLoading: requestsLoading } = usePendingRequests();
  const acceptRequest = useAcceptFriendRequest();
  const declineRequest = useDeclineFriendRequest();
  const { showToast } = useToast();

  const incomingRequests = requests?.filter((r) => r.direction === 'incoming') ?? [];
  const outgoingRequests = requests?.filter((r) => r.direction === 'outgoing') ?? [];

  const handleAccept = (id: string) => {
    acceptRequest.mutate(id, {
      onSuccess: () => showToast('Friend request accepted!', 'success'),
      onError: () => showToast('Failed to accept request', 'error'),
    });
  };

  const handleDecline = (id: string) => {
    declineRequest.mutate(id, {
      onSuccess: () => showToast('Friend request declined', 'info'),
      onError: () => showToast('Failed to decline request', 'error'),
    });
  };

  if (friendsLoading || requestsLoading) {
    return <div className="friends-list-loading">Loading...</div>;
  }

  return (
    <div className="friends-list">
      {(incomingRequests.length > 0 || outgoingRequests.length > 0) && (
        <section className="friends-section">
          <h3>Pending Requests</h3>

          {incomingRequests.length > 0 && (
            <div className="friends-subsection">
              <h4 className="friends-subsection-title">Incoming</h4>
              <ul className="friends-entries">
                {incomingRequests.map((req) => (
                  <li key={req.friendshipId} className="friends-entry">
                    <Link to={`/profile/${req.username}`} className="friends-entry-info">
                      <span className="friends-entry-name">{req.displayName}</span>
                      <span className="friends-entry-username">@{req.username}</span>
                    </Link>
                    <div className="friends-entry-actions">
                      <button
                        className="btn btn-primary btn-sm"
                        onClick={() => handleAccept(req.friendshipId)}
                        disabled={acceptRequest.isPending}
                      >
                        Accept
                      </button>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() => handleDecline(req.friendshipId)}
                        disabled={declineRequest.isPending}
                      >
                        Decline
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {outgoingRequests.length > 0 && (
            <div className="friends-subsection">
              <h4 className="friends-subsection-title">Outgoing</h4>
              <ul className="friends-entries">
                {outgoingRequests.map((req) => (
                  <li key={req.friendshipId} className="friends-entry">
                    <Link to={`/profile/${req.username}`} className="friends-entry-info">
                      <span className="friends-entry-name">{req.displayName}</span>
                      <span className="friends-entry-username">@{req.username}</span>
                    </Link>
                    <span className="friends-entry-pending">Pending</span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      <section className="friends-section">
        <h3>Friends{friends ? ` (${friends.length})` : ''}</h3>
        {!friends || friends.length === 0 ? (
          <p className="friends-empty">No friends yet. Search for players above to add friends!</p>
        ) : (
          <ul className="friends-entries">
            {friends.map((friend) => (
              <li key={friend.userId} className="friends-entry">
                <Link to={`/profile/${friend.username}`} className="friends-entry-info">
                  <span className="friends-entry-name">{friend.displayName}</span>
                  <span className="friends-entry-username">@{friend.username}</span>
                </Link>
                <div className="friends-entry-actions">
                  <button
                    className="btn btn-secondary btn-sm"
                    onClick={() => navigate(`/inbox/${friend.userId}`)}
                  >
                    Message
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
