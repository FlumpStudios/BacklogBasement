import { useEffect, useRef, useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth';
import { useConversations, useMessages, useSendMessage, useFriends } from '../hooks';
import { messagesApi } from '../api';
import './InboxPage.css';

function timeAgo(dateString: string): string {
  const seconds = Math.floor((Date.now() - new Date(dateString).getTime()) / 1000);
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(dateString).toLocaleDateString();
}

function formatTime(dateString: string): string {
  return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

export function InboxPage() {
  const { friendUserId } = useParams<{ friendUserId?: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [draft, setDraft] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const [showFriendPicker, setShowFriendPicker] = useState(false);

  const { data: conversations } = useConversations();
  const { data: friends } = useFriends();
  const { data: messages } = useMessages(friendUserId);
  const sendMessage = useSendMessage(friendUserId ?? '');

  const activeFriend = conversations?.find(c => c.friendUserId === friendUserId);
  const activeFriendFromList = friends?.find(f => f.userId === friendUserId);
  const friendDisplayName = activeFriend?.friendDisplayName ?? activeFriendFromList?.displayName;
  const friendUsername = activeFriend?.friendUsername ?? activeFriendFromList?.username;


  // Mark conversation as read when switching threads
  useEffect(() => {
    if (friendUserId) {
      messagesApi.markAsRead(friendUserId).catch(() => {});
    }
  }, [friendUserId]);

  // Scroll to bottom when messages load or update
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSend = () => {
    const content = draft.trim();
    if (!content || sendMessage.isPending) return;
    setDraft('');
    sendMessage.mutate({ content });
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const hasThread = !!friendUserId;

  return (
    <div className={`inbox-page ${hasThread ? 'thread-active' : ''}`}>
      {/* Left sidebar */}
      <aside className="inbox-sidebar">
        <div className="inbox-sidebar-header">
          <h2>Inbox</h2>
          {friends && friends.length > 0 && (
            <button
              className="btn btn-primary btn-sm"
              onClick={() => setShowFriendPicker(p => !p)}
            >
              + New
            </button>
          )}
        </div>

        {showFriendPicker && (
          <div className="inbox-friend-picker">
            <p className="inbox-friend-picker-label">Start a conversation with:</p>
            {friends && friends.length > 0 ? (
              <ul className="inbox-friend-picker-list">
                {friends.map(f => (
                  <li
                    key={f.userId}
                    className="inbox-friend-picker-item"
                    onClick={() => {
                      setShowFriendPicker(false);
                      navigate(`/inbox/${f.userId}`);
                    }}
                  >
                    <span className="inbox-friend-picker-name">{f.displayName}</span>
                    <span className="inbox-friend-picker-username">@{f.username}</span>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="inbox-friend-picker-empty">No friends yet</p>
            )}
          </div>
        )}

        {!conversations || conversations.length === 0 ? (
          <div className="inbox-empty-sidebar">
            {friends && friends.length > 0
              ? 'Click "+ New" to start a conversation'
              : 'No messages yet'}
          </div>
        ) : (
          <ul className="conversation-list">
            {conversations.map(conv => (
              <li
                key={conv.friendUserId}
                className={`conversation-item ${conv.friendUserId === friendUserId ? 'active' : ''}`}
                onClick={() => navigate(`/inbox/${conv.friendUserId}`)}
              >
                <div className="conversation-header">
                  <span className="conversation-name">{conv.friendDisplayName}</span>
                  <span className="conversation-time">{timeAgo(conv.lastMessageAt)}</span>
                </div>
                <div className="conversation-preview-row">
                  <span className="conversation-preview">
                    {conv.lastMessageIsFromMe ? 'You: ' : ''}{conv.lastMessageContent}
                  </span>
                  {conv.unreadCount > 0 && (
                    <span className="conversation-unread-badge">{conv.unreadCount}</span>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </aside>

      {/* Right thread panel */}
      <section className="inbox-thread">
        {!friendUserId ? (
          <div className="inbox-empty-thread">
            <p>Select a conversation to start messaging</p>
          </div>
        ) : (
          <>
            <div className="inbox-thread-header">
              {friendUsername ? (
                <Link
                  to={`/profile/${friendUsername}`}
                  className="inbox-thread-friend-link"
                >
                  {friendDisplayName}
                </Link>
              ) : (
                <span className="inbox-thread-friend-link">Loading...</span>
              )}
            </div>

            <div className="inbox-messages">
              {messages && messages.length === 0 && (
                <div className="inbox-no-messages">No messages yet. Say hello!</div>
              )}
              {messages?.map(msg => {
                const isMe = msg.senderId === user?.id;
                return (
                  <div key={msg.id} className={`message-bubble-wrapper ${isMe ? 'mine' : 'theirs'}`}>
                    <div className={`message-bubble ${isMe ? 'bubble-mine' : 'bubble-theirs'}`}>
                      <span className="message-content">{msg.content}</span>
                      <span className="message-time">{formatTime(msg.createdAt)}</span>
                    </div>
                  </div>
                );
              })}
              <div ref={messagesEndRef} />
            </div>

            <div className="inbox-compose">
              <textarea
                ref={textareaRef}
                className="inbox-textarea"
                placeholder="Write a message... (Enter to send, Shift+Enter for newline)"
                value={draft}
                onChange={e => setDraft(e.target.value)}
                onKeyDown={handleKeyDown}
                maxLength={2000}
                rows={1}
              />
              <button
                className="btn btn-primary inbox-send-btn"
                onClick={handleSend}
                disabled={!draft.trim() || sendMessage.isPending}
              >
                Send
              </button>
            </div>
          </>
        )}
      </section>
    </div>
  );
}
