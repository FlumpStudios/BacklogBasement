import { Link } from 'react-router-dom';
import { GameSuggestionDto } from '../../types';
import { useDismissSuggestion } from '../../hooks';
import { useToast } from '../../components';
import './SuggestionCard.css';

interface SuggestionCardProps {
  suggestion: GameSuggestionDto;
}

export function SuggestionCard({ suggestion }: SuggestionCardProps) {
  const dismiss = useDismissSuggestion();
  const { showToast } = useToast();

  const handleDismiss = (e: React.MouseEvent) => {
    e.preventDefault();
    dismiss.mutate(suggestion.id, {
      onSuccess: () => showToast('Suggestion dismissed', 'info'),
      onError: () => showToast('Failed to dismiss suggestion', 'error'),
    });
  };

  return (
    <div className="suggestion-card">
      <Link to={`/game/${suggestion.gameId}`} className="suggestion-card-link">
        {suggestion.coverUrl ? (
          <img src={suggestion.coverUrl} alt="" className="suggestion-cover" />
        ) : (
          <div className="suggestion-cover-placeholder">🎮</div>
        )}
        <div className="suggestion-info">
          <span className="suggestion-game-name">{suggestion.gameName}</span>
          <span className="suggestion-from">
            from{' '}
            <Link
              to={`/profile/${suggestion.senderUsername}`}
              className="suggestion-sender-link"
              onClick={(e) => e.stopPropagation()}
            >
              {suggestion.senderDisplayName}
            </Link>
          </span>
          {suggestion.message && (
            <span className="suggestion-message">"{suggestion.message}"</span>
          )}
        </div>
      </Link>
      <button
        className="suggestion-dismiss"
        onClick={handleDismiss}
        disabled={dismiss.isPending}
        aria-label="Dismiss suggestion"
      >
        ✕
      </button>
    </div>
  );
}
