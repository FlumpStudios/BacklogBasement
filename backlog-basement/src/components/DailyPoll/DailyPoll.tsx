import { useState } from 'react';
import { useDailyPoll, usePreviousPoll, useVotePoll } from '../../hooks';
import { DailyPollDto } from '../../types';
import './DailyPoll.css';

function PollResults({ poll }: { poll: DailyPollDto }) {
  const totalVotes = poll.results?.reduce((sum, r) => sum + r.voteCount, 0) ?? 0;
  return (
    <div className="poll-options">
      {poll.games.map((game) => {
        const result = poll.results?.find((r) => r.gameId === game.gameId);
        const isVoted = poll.userVotedGameId === game.gameId;
        return (
          <div
            key={game.gameId}
            className={`poll-option poll-option--voted${isVoted ? ' poll-option--selected' : ''}`}
          >
            <div className="poll-option-inner">
              {game.coverUrl ? (
                <img src={game.coverUrl} alt={game.name} className="poll-option-cover" loading="lazy" />
              ) : (
                <div className="poll-option-cover poll-option-cover--placeholder">🎮</div>
              )}
              <span className="poll-option-name">{game.name}</span>
              <span className="poll-option-pct">{result?.percentage ?? 0}%</span>
            </div>
            {result && (
              <div className="poll-option-bar" style={{ width: `${result.percentage}%` }} aria-hidden="true" />
            )}
          </div>
        );
      })}
      <p className="poll-total-votes">{totalVotes} vote{totalVotes !== 1 ? 's' : ''}</p>
    </div>
  );
}

export function DailyPoll() {
  const { data: poll, isLoading } = useDailyPoll();
  const { data: previousPoll } = usePreviousPoll();
  const voteMutation = useVotePoll();
  const [isOpen, setIsOpen] = useState(true);
  const [isPreviousOpen, setIsPreviousOpen] = useState(false);

  if (isLoading) {
    return (
      <section className="dashboard-section poll-section">
        <div className="section-header">
          <h2>Daily Poll</h2>
        </div>
        <div className="daily-poll-loading">
          <div className="loading-spinner" />
        </div>
      </section>
    );
  }

  if (!poll || poll.games.length === 0) return null;

  const hasVoted = !!poll.userVotedGameId;

  const handleVote = (gameId: string) => {
    if (hasVoted || voteMutation.isPending) return;
    voteMutation.mutate({ pollId: poll.pollId, gameId });
  };

  const totalVotes = poll.results?.reduce((sum, r) => sum + r.voteCount, 0) ?? 0;

  return (
    <section className="dashboard-section poll-section">
      <button className="poll-accordion-header" onClick={() => setIsOpen(o => !o)} aria-expanded={isOpen}>
        <h2>Daily Poll</h2>
        <div className="poll-accordion-meta">
          {hasVoted && (
            <span className="poll-vote-count">{totalVotes} vote{totalVotes !== 1 ? 's' : ''}</span>
          )}
          <span className="poll-accordion-chevron">{isOpen ? '▲' : '▼'}</span>
        </div>
      </button>

      {isOpen && (
        <>
          <p className="poll-category">{poll.category}</p>
          <div className="poll-options">
            {poll.games.map((game) => {
              const result = poll.results?.find((r) => r.gameId === game.gameId);
              const isVoted = poll.userVotedGameId === game.gameId;
              return (
                <button
                  key={game.gameId}
                  className={`poll-option${hasVoted ? ' poll-option--voted' : ''}${isVoted ? ' poll-option--selected' : ''}`}
                  onClick={() => handleVote(game.gameId)}
                  disabled={hasVoted || voteMutation.isPending}
                  aria-pressed={isVoted}
                >
                  <div className="poll-option-inner">
                    {game.coverUrl ? (
                      <img src={game.coverUrl} alt={game.name} className="poll-option-cover" loading="lazy" />
                    ) : (
                      <div className="poll-option-cover poll-option-cover--placeholder">🎮</div>
                    )}
                    <span className="poll-option-name">{game.name}</span>
                    {hasVoted && result && (
                      <span className="poll-option-pct">{result.percentage}%</span>
                    )}
                  </div>
                  {hasVoted && result && (
                    <div className="poll-option-bar" style={{ width: `${result.percentage}%` }} aria-hidden="true" />
                  )}
                </button>
              );
            })}
          </div>
        </>
      )}

      {previousPoll && previousPoll.games.length > 0 && (
        <div className="poll-previous">
          <button className="poll-accordion-header poll-previous-header" onClick={() => setIsPreviousOpen(o => !o)} aria-expanded={isPreviousOpen}>
            <span className="poll-previous-title">Previous poll results</span>
            <span className="poll-accordion-chevron">{isPreviousOpen ? '▲' : '▼'}</span>
          </button>
          {isPreviousOpen && (
            <>
              <p className="poll-category poll-previous-category">{previousPoll.category}</p>
              <PollResults poll={previousPoll} />
            </>
          )}
        </div>
      )}
    </section>
  );
}
