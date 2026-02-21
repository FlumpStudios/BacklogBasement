import { useState, useRef, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useUnreadCount, useNotifications, useMarkAsRead, useMarkAllAsRead } from '../../hooks';
import './NotificationBell.css';

function timeAgo(dateString: string): string {
  const seconds = Math.floor((Date.now() - new Date(dateString).getTime()) / 1000);
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { data: unreadData } = useUnreadCount();
  const { data: notifications, refetch } = useNotifications();
  const markAsRead = useMarkAsRead();
  const markAllAsRead = useMarkAllAsRead();

  const unreadCount = unreadData?.count ?? 0;

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleToggle = () => {
    const opening = !isOpen;
    setIsOpen(opening);
    if (opening) {
      refetch();
    }
  };

  const handleMarkAllAsRead = () => {
    markAllAsRead.mutate();
  };

  const handleNotificationClick = (id: string, isRead: boolean) => {
    if (!isRead) {
      markAsRead.mutate(id);
    }
  };

  return (
    <div className="notification-bell" ref={dropdownRef}>
      <button className="notification-bell-btn" onClick={handleToggle} aria-label="Notifications">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M13.73 21a2 2 0 0 1-3.46 0" />
        </svg>
        {unreadCount > 0 && (
          <span className="notification-badge">{unreadCount > 99 ? '99+' : unreadCount}</span>
        )}
      </button>

      {isOpen && (
        <div className="notification-dropdown">
          <div className="notification-dropdown-header">
            <span className="notification-dropdown-title">Notifications</span>
            {unreadCount > 0 && (
              <button
                className="notification-mark-all-btn"
                onClick={handleMarkAllAsRead}
                disabled={markAllAsRead.isPending}
              >
                Mark all as read
              </button>
            )}
          </div>

          <div className="notification-list">
            {!notifications || notifications.length === 0 ? (
              <div className="notification-empty">No notifications yet</div>
            ) : (
              notifications.map((notification) => (
                <div
                  key={notification.id}
                  className={`notification-item ${notification.isRead ? 'read' : 'unread'}`}
                  onClick={() => handleNotificationClick(notification.id, notification.isRead)}
                >
                  {notification.type === 'game_suggestion' && notification.relatedGameId ? (
                    <Link
                      to={`/games/${notification.relatedGameId}`}
                      className="notification-link"
                      onClick={() => setIsOpen(false)}
                    >
                      <span className="notification-message">{notification.message}</span>
                      <span className="notification-time">{timeAgo(notification.createdAt)}</span>
                    </Link>
                  ) : notification.relatedUsername ? (
                    <Link
                      to={`/profile/${notification.relatedUsername}`}
                      className="notification-link"
                      onClick={() => setIsOpen(false)}
                    >
                      <span className="notification-message">{notification.message}</span>
                      <span className="notification-time">{timeAgo(notification.createdAt)}</span>
                    </Link>
                  ) : notification.relatedClubId ? (
                    <Link
                      to={`/clubs/${notification.relatedClubId}`}
                      className="notification-link"
                      onClick={() => setIsOpen(false)}
                    >
                      <span className="notification-message">{notification.message}</span>
                      <span className="notification-time">{timeAgo(notification.createdAt)}</span>
                    </Link>
                  ) : (
                    <div className="notification-content">
                      <span className="notification-message">{notification.message}</span>
                      <span className="notification-time">{timeAgo(notification.createdAt)}</span>
                    </div>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
