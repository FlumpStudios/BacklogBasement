import {
  useFriendshipStatus,
  useSendFriendRequest,
  useAcceptFriendRequest,
  useDeclineFriendRequest,
  useRemoveFriend,
} from '../../hooks';
import { useToast } from '../Toast';
import './FriendButton.css';

interface FriendButtonProps {
  userId: string;
}

export function FriendButton({ userId }: FriendButtonProps) {
  const { data: statusData, isLoading } = useFriendshipStatus(userId);
  const sendRequest = useSendFriendRequest();
  const acceptRequest = useAcceptFriendRequest();
  const declineRequest = useDeclineFriendRequest();
  const removeFriend = useRemoveFriend();
  const { showToast } = useToast();

  if (isLoading || !statusData) return null;

  const { status, friendshipId } = statusData;

  const handleSendRequest = () => {
    sendRequest.mutate(userId, {
      onSuccess: () => showToast('Friend request sent!', 'success'),
      onError: () => showToast('Failed to send friend request', 'error'),
    });
  };

  const handleAccept = () => {
    if (!friendshipId) return;
    acceptRequest.mutate(friendshipId, {
      onSuccess: () => showToast('Friend request accepted!', 'success'),
      onError: () => showToast('Failed to accept request', 'error'),
    });
  };

  const handleDecline = () => {
    if (!friendshipId) return;
    declineRequest.mutate(friendshipId, {
      onSuccess: () => showToast('Friend request declined', 'info'),
      onError: () => showToast('Failed to decline request', 'error'),
    });
  };

  const handleRemove = () => {
    if (!friendshipId) return;
    removeFriend.mutate(friendshipId, {
      onSuccess: () => showToast('Friend removed', 'info'),
      onError: () => showToast('Failed to remove friend', 'error'),
    });
  };

  switch (status) {
    case 'none':
      return (
        <button
          className="btn btn-primary friend-btn"
          onClick={handleSendRequest}
          disabled={sendRequest.isPending}
        >
          {sendRequest.isPending ? 'Sending...' : 'Add Friend'}
        </button>
      );

    case 'pending_outgoing':
      return (
        <button className="btn btn-secondary friend-btn" disabled>
          Request Sent
        </button>
      );

    case 'pending_incoming':
      return (
        <div className="friend-btn-group">
          <button
            className="btn btn-primary friend-btn"
            onClick={handleAccept}
            disabled={acceptRequest.isPending}
          >
            Accept
          </button>
          <button
            className="btn btn-secondary friend-btn"
            onClick={handleDecline}
            disabled={declineRequest.isPending}
          >
            Decline
          </button>
        </div>
      );

    case 'friends':
      return (
        <div className="friend-btn-group">
          <span className="friend-label">Friends</span>
          <button
            className="btn btn-secondary btn-sm friend-remove-btn"
            onClick={handleRemove}
            disabled={removeFriend.isPending}
          >
            Remove
          </button>
        </div>
      );

    default:
      return null;
  }
}
