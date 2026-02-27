import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { quizApi } from '../api';
import { useToast } from '../components';

export const DAILY_QUIZ_QUERY_KEY = ['daily-quiz'];
export const PREVIOUS_QUIZ_QUERY_KEY = ['daily-quiz', 'previous'];

export function useDailyQuiz() {
  return useQuery({
    queryKey: DAILY_QUIZ_QUERY_KEY,
    queryFn: quizApi.getToday,
  });
}

export function usePreviousQuiz() {
  return useQuery({
    queryKey: PREVIOUS_QUIZ_QUERY_KEY,
    queryFn: quizApi.getPrevious,
  });
}

export function useAnswerQuiz() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ quizId, optionId }: { quizId: string; optionId: string }) =>
      quizApi.answer(quizId, optionId),
    onSuccess: ({ data, xpAwarded }) => {
      queryClient.setQueryData(DAILY_QUIZ_QUERY_KEY, data);
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
    },
  });
}
