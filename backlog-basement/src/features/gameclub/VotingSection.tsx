import { Link } from 'react-router-dom';
import { GameClubNominationDto } from '../../types';
import './GameClub.css';

interface VotingSectionProps {
  nominations: GameClubNominationDto[];
}

export function VotingSection({ nominations }: VotingSectionProps) {
  return (
    <div className="voting-section">
      {nominations.length === 0 ? (
        <p className="club-empty-text">No nominations yet.</p>
      ) : (
        <ul className="nominations-list">
          {nominations.map((nomination) => (
            <li key={nomination.id} className="nomination-item">
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
                  <span className="nomination-by">
                    nominated by{' '}
                    <Link to={`/profile/${nomination.nominatedByUsername}`}>
                      {nomination.nominatedByDisplayName}
                    </Link>
                  </span>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
