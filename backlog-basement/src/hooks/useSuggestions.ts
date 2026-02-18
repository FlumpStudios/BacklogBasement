import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { suggestionsApi } from '../api';
import { SendGameSuggestionRequest } from '../types';

export const SUGGESTIONS_QUERY_KEY = ['suggestions'];

export function useSuggestions() {
  return useQuery({
    queryKey: SUGGESTIONS_QUERY_KEY,
    queryFn: () => suggestionsApi.getReceived(),
  });
}

export function useSendSuggestion() {
  return useMutation({
    mutationFn: (request: SendGameSuggestionRequest) => suggestionsApi.send(request),
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
