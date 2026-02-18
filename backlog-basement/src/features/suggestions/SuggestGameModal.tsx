import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useGameSearch, useFriends, useSendSuggestion } from '../../hooks';
import { GameDto, FriendDto } from '../../types';
import './SuggestGameModal.css';

interface SuggestGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  mode: 'pick-game' | 'pick-friend';
  friendUserId?: string;
  friendDisplayName?: string;
  gameId?: string;
  gameName?: string;
}

export function SuggestGameModal({
  isOpen,
  onClose,
  mode,
  friendUserId,
  friendDisplayName,
  gameId,
  gameName,
}: SuggestGameModalProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedGame, setSelectedGame] = useState<GameDto | null>(null);
  const [selectedFriend, setSelectedFriend] = useState<FriendDto | null>(null);
  const [message, setMessage] = useState('');

  const { data: searchResults } = useGameSearch(mode === 'pick-game' ? searchQuery : '');
  const { data: friends } = useFriends();
  const sendSuggestion = useSendSuggestion();
  const { showToast } = useToast();

  const handleSend = async () => {
    const targetRecipientId = mode === 'pick-game' ? friendUserId : selectedFriend?.userId;
    const targetGameId = mode === 'pick-friend' ? gameId : selectedGame?.id;

    if (!targetRecipientId || !targetGameId) return;

    try {
      await sendSuggestion.mutateAsync({
        recipientUserId: targetRecipientId,
        gameId: targetGameId,
        message: message.trim() || undefined,
      });
      const recipientName = mode === 'pick-game' ? friendDisplayName : selectedFriend?.displayName;
      const game = mode === 'pick-friend' ? gameName : selectedGame?.name;
      showToast(`Suggested "${game}" to ${recipientName}!`, 'success');
      handleClose();
    } catch {
      showToast('Failed to send suggestion', 'error');
    }
  };

  const handleClose = () => {
    setSearchQuery('');
    setSelectedGame(null);
    setSelectedFriend(null);
    setMessage('');
    onClose();
  };

  const title = mode === 'pick-game'
    ? `Suggest a game to ${friendDisplayName}`
    : `Suggest "${gameName}" to a friend`;

  const canSend = mode === 'pick-game'
    ? !!selectedGame
    : !!selectedFriend;

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={title}>
      <div className="suggest-modal">
        {mode === 'pick-game' && (
          <div className="suggest-search">
            <input
              type="text"
              placeholder="Search for a game..."
              value={searchQuery}
              onChange={(e) => {
                setSearchQuery(e.target.value);
                setSelectedGame(null);
              }}
              className="suggest-search-input"
            />
            {selectedGame ? (
              <div className="suggest-selected">
                <div className="suggest-selected-game">
                  {selectedGame.coverUrl && (
                    <img src={selectedGame.coverUrl} alt="" className="suggest-selected-cover" />
                  )}
                  <span className="suggest-selected-name">{selectedGame.name}</span>
                  <button
                    className="suggest-change-btn"
                    onClick={() => setSelectedGame(null)}
                  >
                    Change
                  </button>
                </div>
              </div>
            ) : searchResults && searchResults.length > 0 && (
              <ul className="suggest-results">
                {searchResults.map((game) => (
                  <li
                    key={game.id}
                    className="suggest-result-item"
                    onClick={() => {
                      setSelectedGame(game);
                      setSearchQuery('');
                    }}
                  >
                    {game.coverUrl && (
                      <img src={game.coverUrl} alt="" className="suggest-result-cover" />
                    )}
                    <span className="suggest-result-name">{game.name}</span>
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}

        {mode === 'pick-friend' && (
          <div className="suggest-friends">
            {selectedFriend ? (
              <div className="suggest-selected">
                <div className="suggest-selected-game">
                  <span className="suggest-selected-name">{selectedFriend.displayName}</span>
                  <span className="suggest-selected-username">@{selectedFriend.username}</span>
                  <button
                    className="suggest-change-btn"
                    onClick={() => setSelectedFriend(null)}
                  >
                    Change
                  </button>
                </div>
              </div>
            ) : friends && friends.length > 0 ? (
              <ul className="suggest-results">
                {friends.map((friend) => (
                  <li
                    key={friend.userId}
                    className="suggest-result-item"
                    onClick={() => setSelectedFriend(friend)}
                  >
                    <span className="suggest-result-name">{friend.displayName}</span>
                    <span className="suggest-result-username">@{friend.username}</span>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="suggest-empty">No friends to suggest to yet.</p>
            )}
          </div>
        )}

        <div className="suggest-message">
          <textarea
            placeholder="Add a message (optional)"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            maxLength={200}
            rows={2}
          />
        </div>

        <button
          className="btn btn-primary suggest-send-btn"
          onClick={handleSend}
          disabled={!canSend || sendSuggestion.isPending}
        >
          {sendSuggestion.isPending ? 'Sending...' : 'Send Suggestion'}
        </button>
      </div>
    </Modal>
  );
}
