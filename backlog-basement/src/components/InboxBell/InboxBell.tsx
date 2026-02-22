import { useNavigate } from 'react-router-dom';
import { useUnreadMessageCount } from '../../hooks';
import './InboxBell.css';

export function InboxBell() {
  const navigate = useNavigate();
  const { data: unreadData } = useUnreadMessageCount(true);
  const unreadCount = unreadData?.count ?? 0;

  return (
    <div className="inbox-bell">
      <button
        className="inbox-bell-btn"
        onClick={() => navigate('/inbox')}
        aria-label="Inbox"
        title="Inbox"
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <rect x="2" y="4" width="20" height="16" rx="2" />
          <path d="M2 7l10 7 10-7" />
        </svg>
        {unreadCount > 0 && (
          <span className="inbox-badge">{unreadCount > 99 ? '99+' : unreadCount}</span>
        )}
      </button>
    </div>
  );
}
