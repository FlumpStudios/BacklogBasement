import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useClubReviewsForGame, useJoinClub, CLUB_REVIEWS_FOR_GAME_QUERY_KEY } from '../../hooks';
import { useAuth } from '../../auth';
import { useToast, FriendButton } from '../../components';
import './GameClub.css';

interface GameClubReviewsSectionProps {
  gameId: string;
}

export function GameClubReviewsSection({ gameId }: GameClubReviewsSectionProps) {
  const { data: clubReviews } = useClubReviewsForGame(gameId);
  const { isAuthenticated } = useAuth();
  const joinClub = useJoinClub();
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  const { user } = useAuth();

  const [expandedReviews, setExpandedReviews] = useState<Set<string>>(new Set());
  const [visibleCounts, setVisibleCounts] = useState<Record<string, number>>({});
  const [joiningClubId, setJoiningClubId] = useState<string | null>(null);

  if (!clubReviews || clubReviews.length === 0) return null;

  const toggleReview = (reviewId: string) => {
    setExpandedReviews((prev) => {
      const next = new Set(prev);
      if (next.has(reviewId)) next.delete(reviewId);
      else next.add(reviewId);
      return next;
    });
  };

  const getVisibleCount = (clubId: string) => visibleCounts[clubId] ?? 5;

  const handleJoin = async (clubId: string) => {
    setJoiningClubId(clubId);
    try {
      await joinClub.mutateAsync(clubId);
      queryClient.invalidateQueries({ queryKey: CLUB_REVIEWS_FOR_GAME_QUERY_KEY(gameId) });
      showToast('Joined club!', 'success');
    } catch {
      showToast('Failed to join club', 'error');
    } finally {
      setJoiningClubId(null);
    }
  };

  return (
    <div className="game-club-reviews-section">
      <h2>Game Club Reviews</h2>
      {clubReviews.map((club) => {
        const visibleCount = getVisibleCount(club.clubId);
        const visibleReviews = club.reviews.slice(0, visibleCount);
        const remaining = club.reviews.length - visibleCount;

        return (
          <div key={club.clubId} className="game-club-reviews-card">
            <div className="game-club-reviews-header">
              <Link to={`/clubs/${club.clubId}`} className="game-club-reviews-name">
                {club.clubName}
              </Link>
              <div className="game-club-reviews-avg">
                <span className="game-club-reviews-avg-value">{club.averageScore.toFixed(1)}</span>
                <span className="game-club-reviews-avg-label">
                  avg · {club.reviewCount} {club.reviewCount === 1 ? 'review' : 'reviews'}
                </span>
              </div>
            </div>

            <div className="game-club-reviews-list">
              {visibleReviews.map((review) => {
                const isExpanded = expandedReviews.has(review.id);
                const hasComment = !!review.comment;
                return (
                  <div key={review.id} className="game-club-review-item">
                    <div
                      className={`game-club-review-row ${!hasComment ? 'no-comment' : ''}`}
                      onClick={() => hasComment && toggleReview(review.id)}
                      role={hasComment ? 'button' : undefined}
                      tabIndex={hasComment ? 0 : undefined}
                      onKeyDown={hasComment ? (e) => e.key === 'Enter' && toggleReview(review.id) : undefined}
                    >
                      <Link
                        to={`/profile/${review.username}`}
                        className="game-club-review-user"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {review.displayName}
                      </Link>
                      {user?.id !== review.userId && (
                        <span onClick={(e) => e.stopPropagation()}>
                          <FriendButton userId={review.userId} />
                        </span>
                      )}
                      <span className="game-club-review-score">{review.score}</span>
                      {hasComment && (
                        <span className="game-club-review-chevron">{isExpanded ? '▲' : '▼'}</span>
                      )}
                    </div>
                    {isExpanded && hasComment && (
                      <p className="game-club-review-comment">{review.comment}</p>
                    )}
                  </div>
                );
              })}
            </div>

            {remaining > 0 && (
              <button
                className="btn btn-ghost btn-sm game-club-reviews-load-more"
                onClick={() => setVisibleCounts((prev) => ({ ...prev, [club.clubId]: club.reviews.length }))}
              >
                Load {remaining} more {remaining === 1 ? 'review' : 'reviews'}
              </button>
            )}

            {isAuthenticated && club.isPublic && !club.isCurrentUserMember && (
              <button
                className="btn btn-secondary game-club-reviews-join"
                onClick={() => handleJoin(club.clubId)}
                disabled={joiningClubId === club.clubId}
              >
                {joiningClubId === club.clubId ? 'Joining...' : 'Join Club'}
              </button>
            )}
          </div>
        );
      })}
    </div>
  );
}
