import { PlaySessionDto } from '../../types';
import { formatPlaytime, formatDate } from '../../utils';
import './PlaySessionList.css';

interface PlaySessionListProps {
  sessions: PlaySessionDto[];
  onDelete?: (sessionId: string) => void;
  isDeleting?: boolean;
}

export function PlaySessionList({
  sessions,
  onDelete,
  isDeleting = false,
}: PlaySessionListProps) {
  if (sessions.length === 0) {
    return (
      <p className="play-session-empty">No play sessions logged yet.</p>
    );
  }

  return (
    <ul className="play-session-list">
      {sessions.map((session) => (
        <li key={session.id} className="play-session-item">
          <div className="play-session-info">
            <span className="play-session-duration">
              {formatPlaytime(session.durationMinutes)}
            </span>
            <span className="play-session-date">
              {formatDate(session.datePlayed)}
            </span>
          </div>
          {onDelete && (
            <button
              onClick={() => onDelete(session.id)}
              className="play-session-delete"
              disabled={isDeleting}
              aria-label="Delete session"
            >
              🗑️
            </button>
          )}
        </li>
      ))}
    </ul>
  );
}
