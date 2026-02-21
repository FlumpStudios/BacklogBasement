import { useState } from 'react';
import { Modal, useToast } from '../../components';
import { useSubmitReview } from '../../hooks';
import './GameClub.css';

interface ReviewModalProps {
  isOpen: boolean;
  onClose: () => void;
  clubId: string;
  roundId: string;
  gameName?: string | null;
  existingScore?: number | null;
  existingComment?: string | null;
}

export function ReviewModal({
  isOpen,
  onClose,
  clubId,
  roundId,
  gameName,
  existingScore,
  existingComment,
}: ReviewModalProps) {
  const [score, setScore] = useState(existingScore ?? 70);
  const [comment, setComment] = useState(existingComment ?? '');

  const submitReview = useSubmitReview(clubId);
  const { showToast } = useToast();

  const handleSubmit = async () => {
    try {
      await submitReview.mutateAsync({ roundId, request: { score, comment: comment.trim() || undefined } });
      showToast('Review submitted!', 'success');
      onClose();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to submit review';
      showToast(msg, 'error');
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={gameName ? `Review: ${gameName}` : 'Submit Review'}
    >
      <div className="club-modal-form">
        <div className="club-form-field">
          <label htmlFor="review-score">Score: <strong>{score}</strong>/100</label>
          <input
            id="review-score"
            type="range"
            min={0}
            max={100}
            value={score}
            onChange={(e) => setScore(Number(e.target.value))}
            className="review-score-slider"
          />
          <div className="review-score-labels">
            <span>0</span>
            <span>50</span>
            <span>100</span>
          </div>
        </div>

        <div className="club-form-field">
          <label htmlFor="review-comment">Thoughts (optional)</label>
          <textarea
            id="review-comment"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Share your thoughts on the game..."
            maxLength={1000}
            rows={4}
          />
        </div>

        <button
          className="btn btn-primary"
          onClick={handleSubmit}
          disabled={submitReview.isPending}
        >
          {submitReview.isPending ? 'Submitting...' : existingScore != null ? 'Update Review' : 'Submit Review'}
        </button>
      </div>
    </Modal>
  );
}
