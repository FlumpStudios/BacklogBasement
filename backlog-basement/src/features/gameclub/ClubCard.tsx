import { useNavigate, Link } from 'react-router-dom';
import { GameClubDto } from '../../types';
import './GameClub.css';

interface ClubCardProps {
  club: GameClubDto;
  showJoinButton?: boolean;
  onJoin?: (clubId: string) => void;
  isJoining?: boolean;
}

const STATUS_LABELS: Record<string, string> = {
  nominating: 'Nominating',
  voting: 'Voting',
  playing: 'Playing',
  reviewing: 'Reviewing',
  completed: 'Completed',
};

export function ClubCard({ club, showJoinButton, onJoin, isJoining }: ClubCardProps) {
  const navigate = useNavigate();

  return (
    <div className="club-card" onClick={() => navigate(`/clubs/${club.id}`)}>
      <div className="club-card-header">
        <h3 className="club-card-name">{club.name}</h3>
        <span className={`club-badge ${club.isPublic ? 'club-badge-public' : 'club-badge-private'}`}>
          {club.isPublic ? 'Public' : 'Private'}
        </span>
      </div>

      {club.description && (
        <p className="club-card-description">{club.description}</p>
      )}

      <div className="club-card-meta">
        <span className="club-card-owner">by <Link to={`/profile/${club.ownerUsername}`} onClick={(e) => e.stopPropagation()}>{club.ownerDisplayName}</Link></span>
        <span className="club-card-members">{club.memberCount} {club.memberCount === 1 ? 'member' : 'members'}</span>
      </div>

      {club.currentRound && (
        <div className="club-card-round">
          <span className={`club-round-status club-round-status-${club.currentRound.status}`}>
            {STATUS_LABELS[club.currentRound.status]}
          </span>
          {club.currentRound.gameName && (
            <span className="club-round-game">Playing: {club.currentRound.gameName}</span>
          )}
        </div>
      )}

      {showJoinButton && (
        <button
          className="btn btn-secondary club-join-btn"
          onClick={(e) => {
            e.stopPropagation();
            onJoin?.(club.id);
          }}
          disabled={isJoining}
        >
          {isJoining ? 'Joining...' : 'Join Club'}
        </button>
      )}
    </div>
  );
}
