import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { steamApi } from '../api';
import { useToast } from '../components';
import { SteamImportRequest } from '../types';
import { COLLECTION_QUERY_KEY } from './useCollection';

export const STEAM_STATUS_QUERY_KEY = ['steam', 'status'];

export function useSteamStatus() {
  return useQuery({
    queryKey: STEAM_STATUS_QUERY_KEY,
    queryFn: steamApi.getStatus,
  });
}

export function useSteamLink() {
  return {
    linkUrl: steamApi.getLinkUrl(),
    link: () => {
      window.location.href = steamApi.getLinkUrl();
    },
  };
}

export function useSteamUnlink() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: steamApi.unlink,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: STEAM_STATUS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['auth', 'me'] });
    },
  });
}

export function useSteamImport() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (request: SteamImportRequest) => steamApi.importLibrary(request),
    onSuccess: ({ xpAwarded }) => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP — Steam library imported!`, 'success');
    },
  });
}

export function useSyncAllSteamPlaytime() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => steamApi.syncAllPlaytime(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
    },
  });
}

export function useSyncSteamPlaytime(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => steamApi.syncPlaytime(gameId),
    onSuccess: () => {
      // Invalidate play sessions to refresh the list
      queryClient.invalidateQueries({ queryKey: [...COLLECTION_QUERY_KEY, gameId, 'play-sessions'] });
      // Invalidate collection to update total playtime
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
    },
  });
}
