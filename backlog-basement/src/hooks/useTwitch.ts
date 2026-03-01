import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { twitchApi } from '../api/twitch';

export function useTwitchLive(twitchUserId?: string | null) {
  return useQuery({
    queryKey: ['twitch', 'live', twitchUserId],
    queryFn: () => twitchApi.getLiveStatus(twitchUserId!),
    enabled: !!twitchUserId,
    staleTime: 60_000,
    refetchInterval: 60_000,
  });
}

export function useTwitchImport() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: twitchApi.importStreams,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['collection'] });
    },
  });
}

export function useTwitchSync() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: twitchApi.syncNow,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['collection'] });
    },
  });
}
