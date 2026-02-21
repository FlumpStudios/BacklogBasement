import { useToast } from '../../components';
import { useAdvanceRound } from '../../hooks';
import { GameClubRoundDto } from '../../types';
import './GameClub.css';

interface RoundStatusBannerProps {
  clubId: string;
  round: GameClubRoundDto;
  currentUserRole?: string | null;
  onNominate?: () => void;
  onReview?: () => void;
}

const STATUS_DESCRIPTIONS: Record<string, string> = {
  nominating: 'Members can nominate games for the club to play.',
  voting: 'Vote for your favourite nomination.',
  playing: 'The game has been selected — time to play!',
  reviewing: 'Submit your score and thoughts on the game.',
  completed: 'Round complete. Check out the results below.',
};

const ADVANCE_LABELS: Record<string, string> = {
  nominating: 'Close Nominations & Open Voting',
  voting: 'Close Voting & Select Game',
  playing: 'Open Reviews',
  reviewing: 'Complete Round',
};

function formatDeadline(date: string | null | undefined): string | null {
  if (!date) return null;
  return new Date(date).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
}

export function RoundStatusBanner({ clubId, round, currentUserRole, onNominate, onReview }: RoundStatusBannerProps) {
  const advanceRound = useAdvanceRound(clubId);
  const { showToast } = useToast();

  const isAdmin = currentUserRole === 'owner' || currentUserRole === 'admin';
  const canAdvance = isAdmin && round.status !== 'completed';

  const handleAdvance = async () => {
    try {
      await advanceRound.mutateAsync(round.id);
      showToast('Round advanced!', 'success');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to advance round';
      showToast(msg, 'error');
    }
  };

  const currentDeadline = round.status === 'nominating' ? round.nominatingDeadline
    : round.status === 'voting' ? round.votingDeadline
    : round.status === 'playing' ? round.playingDeadline
    : round.status === 'reviewing' ? round.reviewingDeadline
    : null;

  return (
    <div className={`round-banner round-banner-${round.status}`}>
      <div className="round-banner-header">
        <div className="round-banner-title">
          <span className={`round-status-pill round-status-pill-${round.status}`}>
            {round.status.charAt(0).toUpperCase() + round.status.slice(1)}
          </span>
          <span className="round-banner-round">Round {round.roundNumber}</span>
        </div>
        {currentDeadline && (
          <span className="round-banner-deadline">Due {formatDeadline(currentDeadline)}</span>
        )}
      </div>

      <p className="round-banner-desc">{STATUS_DESCRIPTIONS[round.status]}</p>

      {round.gameName && round.status !== 'nominating' && round.status !== 'voting' && (
        <div className="round-banner-game">
          {round.gameCoverUrl && <img src={round.gameCoverUrl} alt="" className="round-game-cover" />}
          <span className="round-game-name">{round.gameName}</span>
        </div>
      )}

      {round.status === 'completed' && round.averageScore != null && (
        <div className="round-banner-score">
          Club Score: <strong>{round.averageScore.toFixed(1)}</strong>/100
        </div>
      )}

      <div className="round-banner-actions">
        {round.status === 'nominating' && (
          <button className="btn btn-primary" onClick={onNominate}>
            Nominate a Game
          </button>
        )}

        {round.status === 'reviewing' && !round.userHasReviewed && (
          <button className="btn btn-primary" onClick={onReview}>
            Submit Review
          </button>
        )}

        {round.status === 'reviewing' && round.userHasReviewed && (
          <button className="btn btn-secondary" onClick={onReview}>
            Update Review
          </button>
        )}

        {canAdvance && ADVANCE_LABELS[round.status] && (
          <button
            className="btn btn-secondary"
            onClick={handleAdvance}
            disabled={advanceRound.isPending}
          >
            {advanceRound.isPending ? 'Advancing...' : ADVANCE_LABELS[round.status]}
          </button>
        )}
      </div>
    </div>
  );
}
