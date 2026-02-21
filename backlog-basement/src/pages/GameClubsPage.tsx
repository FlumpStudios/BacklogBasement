import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePublicClubs, useMyClubs, useJoinClub, useMyClubInvites, useRespondToInvite } from '../hooks';
import { ClubCard, CreateClubModal } from '../features/gameclub';
import { useToast } from '../components';
import './GameClubsPage.css';

export function GameClubsPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [showCreate, setShowCreate] = useState(false);

  const { data: publicClubs, isLoading: loadingPublic } = usePublicClubs();
  const { data: myClubs, isLoading: loadingMy } = useMyClubs();
  const { data: pendingInvites } = useMyClubInvites();

  const joinClub = useJoinClub();
  const respondToInvite = useRespondToInvite();

  const myClubIds = new Set(myClubs?.map((c) => c.id) ?? []);

  const handleJoin = async (clubId: string) => {
    try {
      await joinClub.mutateAsync(clubId);
      showToast('Joined club!', 'success');
      navigate(`/clubs/${clubId}`);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to join club';
      showToast(msg, 'error');
    }
  };

  const handleRespondToInvite = async (clubId: string, inviteId: string, accept: boolean) => {
    try {
      await respondToInvite.mutateAsync({ clubId, inviteId, request: { accept } });
      showToast(accept ? 'Joined club!' : 'Invite declined.', 'success');
      if (accept) navigate(`/clubs/${clubId}`);
    } catch {
      showToast('Failed to respond to invite', 'error');
    }
  };

  return (
    <div className="clubs-page">
      <div className="clubs-page-header">
        <h1>Game Clubs</h1>
        <button className="btn btn-primary" onClick={() => setShowCreate(true)}>
          + Create Club
        </button>
      </div>

      {pendingInvites && pendingInvites.length > 0 && (
        <section className="clubs-section">
          <h2>Pending Invites</h2>
          <div className="invites-list">
            {pendingInvites.map((invite) => (
              <div key={invite.id} className="invite-banner">
                <span className="invite-banner-text">
                  <strong>{invite.invitedByDisplayName}</strong> invited you to join{' '}
                  <strong>{invite.clubName}</strong>
                </span>
                <div className="invite-banner-actions">
                  <button
                    className="btn btn-primary btn-sm"
                    onClick={() => handleRespondToInvite(invite.clubId, invite.id, true)}
                    disabled={respondToInvite.isPending}
                  >
                    Accept
                  </button>
                  <button
                    className="btn btn-secondary btn-sm"
                    onClick={() => handleRespondToInvite(invite.clubId, invite.id, false)}
                    disabled={respondToInvite.isPending}
                  >
                    Decline
                  </button>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      <section className="clubs-section">
        <h2>My Clubs</h2>
        {loadingMy ? (
          <p className="clubs-loading">Loading...</p>
        ) : myClubs && myClubs.length > 0 ? (
          <div className="clubs-grid">
            {myClubs.map((club) => (
              <ClubCard key={club.id} club={club} />
            ))}
          </div>
        ) : (
          <p className="clubs-empty">You haven't joined any clubs yet. Create one or join a public club below.</p>
        )}
      </section>

      <section className="clubs-section">
        <h2>Public Clubs</h2>
        {loadingPublic ? (
          <p className="clubs-loading">Loading...</p>
        ) : publicClubs && publicClubs.length > 0 ? (
          <div className="clubs-grid">
            {publicClubs
              .filter((c) => !myClubIds.has(c.id))
              .map((club) => (
                <ClubCard
                  key={club.id}
                  club={club}
                  showJoinButton
                  onJoin={handleJoin}
                  isJoining={joinClub.isPending}
                />
              ))}
            {publicClubs.filter((c) => !myClubIds.has(c.id)).length === 0 && (
              <p className="clubs-empty">You're already in all public clubs!</p>
            )}
          </div>
        ) : (
          <p className="clubs-empty">No public clubs yet. Be the first to create one!</p>
        )}
      </section>

      <CreateClubModal
        isOpen={showCreate}
        onClose={() => setShowCreate(false)}
        onCreated={(id) => navigate(`/clubs/${id}`)}
      />
    </div>
  );
}
