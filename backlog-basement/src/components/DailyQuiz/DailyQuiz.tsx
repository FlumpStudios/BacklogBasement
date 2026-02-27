import { useState } from 'react';
import { useDailyQuiz, usePreviousQuiz, useAnswerQuiz } from '../../hooks';
import { DailyQuizDto, DailyQuizOptionDto } from '../../types';
import './DailyQuiz.css';

const GAME_TYPES = new Set([
  'release_order_first',
  'release_order_last',
  'highest_metacritic',
  'lowest_metacritic',
  'most_collected',
  'most_played',
]);

const TRUE_FALSE_TYPES = new Set(['true_false_release', 'true_false_metacritic']);

function QuizResults({ quiz }: { quiz: DailyQuizDto }) {
  const totalAnswers = quiz.results?.reduce((s, r) => s + r.answerCount, 0) ?? 0;
  const isGameType = GAME_TYPES.has(quiz.questionType);
  const isTrueFalse = TRUE_FALSE_TYPES.has(quiz.questionType);

  return (
    <div className={`quiz-options${isTrueFalse ? ' quiz-options--tf' : ''}`}>
      {quiz.options.map((opt) => {
        const result = quiz.results?.find((r) => r.optionId === opt.optionId);
        const isSelected = quiz.userSelectedOptionId === opt.optionId;
        const isCorrect = result?.isCorrect ?? false;
        return (
          <div
            key={opt.optionId}
            className={`quiz-option quiz-option--result${isSelected ? ' quiz-option--selected' : ''}${isCorrect ? ' quiz-option--correct' : ''}${isSelected && !isCorrect ? ' quiz-option--wrong' : ''}`}
          >
            <div className="quiz-option-inner">
              {isGameType && (
                opt.coverUrl ? (
                  <img src={opt.coverUrl} alt={opt.text} className="quiz-option-cover" loading="lazy" />
                ) : (
                  <div className="quiz-option-cover quiz-option-cover--placeholder">🎮</div>
                )
              )}
              <span className="quiz-option-name">{opt.text}</span>
              <span className="quiz-option-pct">{result?.percentage ?? 0}%</span>
            </div>
            {result && (
              <div className="quiz-option-bar" style={{ width: `${result.percentage}%` }} aria-hidden="true" />
            )}
          </div>
        );
      })}
      <p className="quiz-total-answers">{totalAnswers} answer{totalAnswers !== 1 ? 's' : ''}</p>
    </div>
  );
}

function QuizOption({
  opt,
  isGameType,
  isTrueFalse,
  onAnswer,
  isPending,
}: {
  opt: DailyQuizOptionDto;
  isGameType: boolean;
  isTrueFalse: boolean;
  onAnswer: (optionId: string) => void;
  isPending: boolean;
}) {
  return (
    <button
      className={`quiz-option${isTrueFalse ? ' quiz-option--tf' : ''}`}
      onClick={() => onAnswer(opt.optionId)}
      disabled={isPending}
    >
      <div className="quiz-option-inner">
        {isGameType && (
          opt.coverUrl ? (
            <img src={opt.coverUrl} alt={opt.text} className="quiz-option-cover" loading="lazy" />
          ) : (
            <div className="quiz-option-cover quiz-option-cover--placeholder">🎮</div>
          )
        )}
        <span className="quiz-option-name">{opt.text}</span>
      </div>
    </button>
  );
}

function QuizWidget({ quiz, label }: { quiz: DailyQuizDto; label?: string }) {
  const answerMutation = useAnswerQuiz();
  const hasAnswered = quiz.userSelectedOptionId != null;
  const isGameType = GAME_TYPES.has(quiz.questionType);
  const isTrueFalse = TRUE_FALSE_TYPES.has(quiz.questionType);

  const handleAnswer = (optionId: string) => {
    if (hasAnswered || answerMutation.isPending) return;
    answerMutation.mutate({ quizId: quiz.quizId, optionId });
  };

  const correctOption = quiz.options.find((o) => {
    if (!quiz.results) return false;
    return quiz.results.find((r) => r.optionId === o.optionId && r.isCorrect);
  });

  return (
    <div className="quiz-widget">
      {label && <p className="quiz-date-label">{label}</p>}
      <p className="quiz-question">{quiz.questionText}</p>

      {hasAnswered ? (
        <>
          {quiz.userWasCorrect !== null && quiz.userWasCorrect !== undefined && (
            <div className={`quiz-feedback-pill${quiz.userWasCorrect ? ' quiz-feedback-pill--correct' : ' quiz-feedback-pill--wrong'}`}>
              {quiz.userWasCorrect
                ? '✓ Correct!'
                : `✗ The answer was ${correctOption?.text ?? '?'}`}
            </div>
          )}
          <QuizResults quiz={quiz} />
        </>
      ) : (
        <div className={`quiz-options${isTrueFalse ? ' quiz-options--tf' : ''}`}>
          {quiz.options.map((opt) => (
            <QuizOption
              key={opt.optionId}
              opt={opt}
              isGameType={isGameType}
              isTrueFalse={isTrueFalse}
              onAnswer={handleAnswer}
              isPending={answerMutation.isPending}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export function DailyQuiz() {
  const { data: quiz, isLoading } = useDailyQuiz();
  const { data: previousQuiz } = usePreviousQuiz();
  const [isOpen, setIsOpen] = useState(true);
  const [isPreviousOpen, setIsPreviousOpen] = useState(false);

  if (isLoading) {
    return (
      <section className="dashboard-section quiz-section">
        <div className="section-header">
          <h2>Daily Quiz</h2>
        </div>
        <div className="daily-quiz-loading">
          <div className="loading-spinner" />
        </div>
      </section>
    );
  }

  if (!quiz) return null;

  const totalAnswers = quiz.results?.reduce((s, r) => s + r.answerCount, 0) ?? 0;
  const hasAnswered = quiz.userSelectedOptionId != null;

  return (
    <section className="dashboard-section quiz-section">
      <button
        className="poll-accordion-header"
        onClick={() => setIsOpen((o) => !o)}
        aria-expanded={isOpen}
      >
        <h2>Daily Quiz</h2>
        <div className="poll-accordion-meta">
          {hasAnswered && (
            <span className="poll-vote-count">{totalAnswers} answer{totalAnswers !== 1 ? 's' : ''}</span>
          )}
          <span className="poll-accordion-chevron">{isOpen ? '▲' : '▼'}</span>
        </div>
      </button>

      {isOpen && <QuizWidget quiz={quiz} />}

      {previousQuiz && (
        <div className="poll-previous">
          <button
            className="poll-accordion-header poll-previous-header"
            onClick={() => setIsPreviousOpen((o) => !o)}
            aria-expanded={isPreviousOpen}
          >
            <span className="poll-previous-title">Previous quiz results</span>
            <span className="poll-accordion-chevron">{isPreviousOpen ? '▲' : '▼'}</span>
          </button>
          {isPreviousOpen && <QuizWidget quiz={previousQuiz} />}
        </div>
      )}
    </section>
  );
}
