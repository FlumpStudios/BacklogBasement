import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { suggestionsApi } from '../api';
import { useToast } from '../components';
import { SendGameSuggestionRequest } from '../types';

export const SUGGESTIONS_QUERY_KEY = ['suggestions'];

export function useSuggestions(enabled = true) {
  return useQuery({
    queryKey: SUGGESTIONS_QUERY_KEY,
    queryFn: () => suggestionsApi.getReceived(),
    enabled,
  });
}

export function useSendSuggestion() {
  const { showToast } = useToast();
  return useMutation({
    mutationFn: (request: SendGameSuggestionRequest) => suggestionsApi.send(request),
    onSuccess: ({ xpAwarded }) => {
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
    },
  });
}

export function useDismissSuggestion() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => suggestionsApi.dismiss(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SUGGESTIONS_QUERY_KEY });
    },
  });
}
