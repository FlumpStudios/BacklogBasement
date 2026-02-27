import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { pollApi } from '../api';
import { useToast } from '../components';

export const DAILY_POLL_QUERY_KEY = ['daily-poll'];
export const PREVIOUS_POLL_QUERY_KEY = ['daily-poll', 'previous'];

export function useDailyPoll() {
  return useQuery({
    queryKey: DAILY_POLL_QUERY_KEY,
    queryFn: pollApi.getToday,
  });
}

export function usePreviousPoll() {
  return useQuery({
    queryKey: PREVIOUS_POLL_QUERY_KEY,
    queryFn: pollApi.getPrevious,
  });
}

export function useVotePoll() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ pollId, gameId }: { pollId: string; gameId: string }) =>
      pollApi.vote(pollId, gameId),
    onSuccess: ({ data, xpAwarded }) => {
      queryClient.setQueryData(DAILY_POLL_QUERY_KEY, data);
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
    },
  });
}
