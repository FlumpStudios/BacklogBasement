import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useGameSearch, usePickGame } from '../../hooks';
import { GameDto } from '../../types';
import './GameClub.css';

interface PickGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  clubId: string;
  roundId: string;
}

export function PickGameModal({ isOpen, onClose, clubId, roundId }: PickGameModalProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedGame, setSelectedGame] = useState<GameDto | null>(null);

  const { data: searchResults } = useGameSearch(searchQuery);
  const pickGame = usePickGame(clubId);
  const { showToast } = useToast();

  const handlePick = async () => {
    if (!selectedGame) return;

    try {
      await pickGame.mutateAsync({ roundId, gameId: selectedGame.id });
      showToast(`"${selectedGame.name}" selected — let's play!`, 'success');
      handleClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to pick game';
      showToast(msg, 'error');
    }
  };

  const handleClose = () => {
    setSearchQuery('');
    setSelectedGame(null);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Pick a Game">
      <div className="suggest-modal">
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

        <button
          className="btn btn-primary suggest-send-btn"
          onClick={handlePick}
          disabled={!selectedGame || pickGame.isPending}
        >
          {pickGame.isPending ? 'Picking...' : 'Pick Game'}
        </button>
      </div>
    </Modal>
  );
}
