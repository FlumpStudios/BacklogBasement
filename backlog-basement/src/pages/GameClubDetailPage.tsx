import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useClub, useStartRound, useRoundReviews, useInviteMember, useFriends, useDeleteClub } from '../hooks';
import {
  ClubMembersList,
  NominateGameModal,
  VotingSection,
  ReviewModal,
  RoundStatusBanner,
} from '../features/gameclub';
import { Modal, useToast } from '../components';
import { useAuth } from '../auth';
import './GameClubDetailPage.css';

export function GameClubDetailPage() {
  const { id: clubId } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { showToast } = useToast();
  const { user } = useAuth();

  const { data: club, isLoading } = useClub(clubId);
  const { data: friends } = useFriends();

  const [showNominate, setShowNominate] = useState(false);
  const [showReview, setShowReview] = useState(false);
  const [showStartRound, setShowStartRound] = useState(false);
  const [showInvite, setShowInvite] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [selectedRoundId, setSelectedRoundId] = useState<string | null>(null);
  const [playingDeadline, setPlayingDeadline] = useState('');

  const startRound = useStartRound(clubId ?? '');
  const inviteMember = useInviteMember(clubId ?? '');
  const deleteClub = useDeleteClub();

  const activeRound = club?.currentRound ?? null;

  const { data: roundReviews } = useRoundReviews(
    clubId,
    selectedRoundId ?? (activeRound?.status === 'reviewing' || activeRound?.status === 'completed' ? activeRound?.id : undefined)
  );

  const isAdmin = club?.currentUserRole === 'owner' || club?.currentUserRole === 'admin';
  const isMember = !!club?.currentUserRole;

  const currentUserReview = roundReviews?.find((r) => r.userId === user?.id);

  const handleStartRound = async () => {
    if (!clubId) return;
    try {
      await startRound.mutateAsync({
        playingDeadline: playingDeadline || undefined,
      });
      showToast('Round started!', 'success');
      setShowStartRound(false);
      setPlayingDeadline('');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to start round';
      showToast(msg, 'error');
    }
  };

  const handleInvite = async (friendUserId: string) => {
    try {
      await inviteMember.mutateAsync(friendUserId);
      showToast('Invite sent!', 'success');
      setShowInvite(false);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to send invite';
      showToast(msg, 'error');
    }
  };

  const handleDeleteClub = async () => {
    if (!clubId) return;
    try {
      await deleteClub.mutateAsync(clubId);
      showToast('Club deleted.', 'success');
      navigate('/clubs');
    } catch {
      showToast('Failed to delete club', 'error');
    }
  };

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading club...</p>
      </div>
    );
  }

  if (!club) {
    return (
      <div className="club-detail-page">
        <div className="not-found">
          <h1>Club not found</h1>
          <button onClick={() => navigate('/clubs')} className="btn btn-secondary">
            Back to Clubs
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="club-detail-page">
      <button onClick={() => navigate('/clubs')} className="back-button">
        ← Clubs
      </button>

      <div className="club-detail-header">
        <div>
          <h1 className="club-detail-name">{club.name}</h1>
          {club.description && <p className="club-detail-desc">{club.description}</p>}
          <div className="club-detail-meta">
            <span>by {club.ownerDisplayName}</span>
            <span>·</span>
            <span>{club.memberCount} {club.memberCount === 1 ? 'member' : 'members'}</span>
            <span>·</span>
            <span>{club.isPublic ? 'Public' : 'Private'}</span>
          </div>
        </div>

        {isMember && (
          <div className="club-detail-actions">
            {isAdmin && (
              <button
                className="btn btn-secondary"
                onClick={() => setShowInvite(true)}
              >
                Invite Friend
              </button>
            )}
            {isAdmin && !activeRound && (
              <button
                className="btn btn-primary"
                onClick={() => setShowStartRound(true)}
              >
                Start Round
              </button>
            )}
            {club.currentUserRole === 'owner' && (
              <button
                className="btn btn-danger btn-sm"
                onClick={() => setShowDeleteConfirm(true)}
              >
                Delete Club
              </button>
            )}
          </div>
        )}
      </div>

      {/* Active Round */}
      {activeRound && isMember && (
        <section className="club-section">
          <h2>Current Round</h2>
          <RoundStatusBanner
            clubId={clubId!}
            round={activeRound}
            currentUserRole={club.currentUserRole}
            onNominate={() => setShowNominate(true)}
            onReview={() => setShowReview(true)}
          />

          {(activeRound.status === 'nominating' || activeRound.status === 'voting') && (
            <div className="club-nominations-section">
              <h3>Nominations</h3>
              <VotingSection
                clubId={clubId!}
                roundId={activeRound.id}
                nominations={activeRound.nominations}
                userVotedNominationId={activeRound.userVotedNominationId}
                status={activeRound.status}
              />
            </div>
          )}

          {(activeRound.status === 'reviewing' || activeRound.status === 'completed') && roundReviews && roundReviews.length > 0 && (
            <div className="club-reviews-section">
              <h3>Reviews</h3>
              <div className="club-reviews-list">
                {roundReviews.map((review) => (
                  <div key={review.id} className="club-review-card">
                    <div className="club-review-header">
                      <span className="club-review-author">{review.displayName}</span>
                      <span className="club-review-score">{review.score}/100</span>
                    </div>
                    {review.comment && (
                      <p className="club-review-comment">{review.comment}</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </section>
      )}

      {/* Past Rounds */}
      {club.rounds.filter((r) => r.status === 'completed').length > 0 && (
        <section className="club-section">
          <h2>Past Rounds</h2>
          <div className="past-rounds-list">
            {club.rounds
              .filter((r) => r.status === 'completed')
              .map((round) => (
                <div
                  key={round.id}
                  className={`past-round-row ${selectedRoundId === round.id ? 'past-round-selected' : ''}`}
                  onClick={() => setSelectedRoundId(selectedRoundId === round.id ? null : round.id)}
                >
                  <div className="past-round-info">
                    <span className="past-round-number">Round {round.roundNumber}</span>
                    {round.gameName && (
                      <>
                        {round.gameId ? (
                          <Link
                            to={`/games/${round.gameId}`}
                            className="past-round-game-link"
                            onClick={(e) => e.stopPropagation()}
                          >
                            {round.gameName}
                          </Link>
                        ) : (
                          <span className="past-round-game">{round.gameName}</span>
                        )}
                      </>
                    )}
                  </div>
                  {round.averageScore != null && (
                    <span className="past-round-score">{round.averageScore.toFixed(1)}/100</span>
                  )}
                </div>
              ))}
          </div>

          {selectedRoundId && roundReviews && roundReviews.length > 0 && (
            <div className="club-reviews-section">
              <h3>Reviews</h3>
              <div className="club-reviews-list">
                {roundReviews.map((review) => (
                  <div key={review.id} className="club-review-card">
                    <div className="club-review-header">
                      <span className="club-review-author">{review.displayName}</span>
                      <span className="club-review-score">{review.score}/100</span>
                    </div>
                    {review.comment && (
                      <p className="club-review-comment">{review.comment}</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </section>
      )}

      {/* Members */}
      <section className="club-section">
        <h2>Members ({club.memberCount})</h2>
        <ClubMembersList
          clubId={clubId!}
          members={club.members}
          currentUserRole={club.currentUserRole}
          currentUserId={user?.id}
        />
      </section>

      {/* Modals */}
      {activeRound && (
        <>
          <NominateGameModal
            isOpen={showNominate}
            onClose={() => setShowNominate(false)}
            clubId={clubId!}
            roundId={activeRound.id}
          />
          <ReviewModal
            isOpen={showReview}
            onClose={() => setShowReview(false)}
            clubId={clubId!}
            roundId={activeRound.id}
            gameName={activeRound.gameName}
            existingScore={currentUserReview?.score}
            existingComment={currentUserReview?.comment}
          />
        </>
      )}

      {/* Start Round Modal */}
      <Modal
        isOpen={showStartRound}
        onClose={() => { setShowStartRound(false); setPlayingDeadline(''); }}
        title="Start New Round"
      >
        <div className="club-modal-form">
          <p className="club-form-hint" style={{ fontSize: '0.9rem', color: 'inherit', margin: 0 }}>
            This will start a new nominating round. Members will be notified and can start nominating games.
          </p>
          <div className="club-form-field">
            <label htmlFor="playing-deadline">Playing deadline (optional)</label>
            <input
              id="playing-deadline"
              type="date"
              value={playingDeadline}
              min={new Date().toISOString().split('T')[0]}
              onChange={(e) => setPlayingDeadline(e.target.value)}
            />
            <p className="club-form-hint">Let members know how long they have to finish the game. The round still closes manually.</p>
          </div>
          <button
            className="btn btn-primary"
            onClick={handleStartRound}
            disabled={startRound.isPending}
          >
            {startRound.isPending ? 'Starting...' : 'Start Round'}
          </button>
        </div>
      </Modal>

      {/* Invite Friend Modal */}
      <Modal
        isOpen={showInvite}
        onClose={() => setShowInvite(false)}
        title="Invite a Friend"
      >
        <div className="invite-friends-list">
          {friends && friends.length > 0 ? (
            <ul className="suggest-results">
              {friends.map((friend) => (
                <li
                  key={friend.userId}
                  className="suggest-result-item"
                  onClick={() => handleInvite(friend.userId)}
                >
                  <span className="suggest-result-name">{friend.displayName}</span>
                  <span className="suggest-result-username">@{friend.username}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="club-empty-text">No friends to invite yet.</p>
          )}
        </div>
      </Modal>

      {/* Delete Club Confirm Modal */}
      <Modal
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        title="Delete Club"
      >
        <div className="club-modal-form">
          <p style={{ margin: 0 }}>
            Are you sure you want to delete <strong>{club?.name}</strong>? This will permanently remove all rounds, nominations, and reviews. All members will be notified.
          </p>
          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button
              className="btn btn-danger"
              onClick={handleDeleteClub}
              disabled={deleteClub.isPending}
            >
              {deleteClub.isPending ? 'Deleting...' : 'Delete Club'}
            </button>
            <button
              className="btn btn-secondary"
              onClick={() => setShowDeleteConfirm(false)}
            >
              Cancel
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
