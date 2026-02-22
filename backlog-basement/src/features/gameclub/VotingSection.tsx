import { Link } from 'react-router-dom';
import { useToast } from '../../components';
import { useVote } from '../../hooks';
import { GameClubNominationDto } from '../../types';
import './GameClub.css';

interface VotingSectionProps {
  clubId: string;
  roundId: string;
  nominations: GameClubNominationDto[];
  userVotedNominationId?: string | null;
  status: string;
}

export function VotingSection({
  clubId,
  roundId,
  nominations,
  userVotedNominationId,
  status,
}: VotingSectionProps) {
  const vote = useVote(clubId);
  const { showToast } = useToast();

  const isVoting = status === 'voting';
  const canVote = isVoting;

  const handleVote = async (nominationId: string) => {
    if (!canVote) return;

    try {
      await vote.mutateAsync({ roundId, nominationId });
      showToast('Vote cast!', 'success');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to cast vote';
      showToast(msg, 'error');
    }
  };

  const totalVotes = nominations.reduce((sum, n) => sum + n.voteCount, 0);

  return (
    <div className="voting-section">
      {nominations.length === 0 ? (
        <p className="club-empty-text">No nominations yet.</p>
      ) : (
        <ul className="nominations-list">
          {nominations.map((nomination) => {
            const isVoted = userVotedNominationId === nomination.id;
            const votePercent = totalVotes > 0 ? Math.round((nomination.voteCount / totalVotes) * 100) : 0;

            return (
              <li key={nomination.id} className={`nomination-item ${isVoted ? 'nomination-voted' : ''}`}>
                <div className="nomination-game-info">
                  {nomination.gameCoverUrl && (
                    <img
                      src={nomination.gameCoverUrl}
                      alt=""
                      className="nomination-cover"
                    />
                  )}
                  <div className="nomination-details">
                    <span className="nomination-game-name">{nomination.gameName}</span>
                    <span className="nomination-by">nominated by <Link to={`/profile/${nomination.nominatedByUsername}`}>{nomination.nominatedByDisplayName}</Link></span>
                  </div>
                </div>

                <div className="nomination-vote-area">
                  {(status !== 'nominating') && (
                    <div className="vote-bar-wrap">
                      <div className="vote-bar" style={{ width: `${votePercent}%` }} />
                      <span className="vote-count">{nomination.voteCount} {nomination.voteCount === 1 ? 'vote' : 'votes'}</span>
                    </div>
                  )}

                  {canVote && (
                    <button
                      className={`btn btn-sm ${isVoted ? 'btn-primary' : 'btn-secondary'}`}
                      onClick={() => handleVote(nomination.id)}
                      disabled={vote.isPending}
                    >
                      {isVoted ? '✓ Voted' : 'Vote'}
                    </button>
                  )}
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
