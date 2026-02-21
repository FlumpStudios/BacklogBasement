import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useGameSearch, useNominateGame } from '../../hooks';
import { GameDto } from '../../types';
import './GameClub.css';

interface NominateGameModalProps {
  isOpen: boolean;
  onClose: () => void;
  clubId: string;
  roundId: string;
}

export function NominateGameModal({ isOpen, onClose, clubId, roundId }: NominateGameModalProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedGame, setSelectedGame] = useState<GameDto | null>(null);

  const { data: searchResults } = useGameSearch(searchQuery);
  const nominateGame = useNominateGame(clubId);
  const { showToast } = useToast();

  const handleNominate = async () => {
    if (!selectedGame) return;

    try {
      await nominateGame.mutateAsync({ roundId, gameId: selectedGame.id });
      showToast(`Nominated "${selectedGame.name}"!`, 'success');
      handleClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to nominate game';
      showToast(msg, 'error');
    }
  };

  const handleClose = () => {
    setSearchQuery('');
    setSelectedGame(null);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Nominate a Game">
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
          onClick={handleNominate}
          disabled={!selectedGame || nominateGame.isPending}
        >
          {nominateGame.isPending ? 'Nominating...' : 'Nominate Game'}
        </button>
      </div>
    </Modal>
  );
}
