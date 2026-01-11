import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { collectionApi } from '../api';
import { CreatePlaySessionDto } from '../types';

export const COLLECTION_QUERY_KEY = ['collection'];

export function useCollection() {
  return useQuery({
    queryKey: COLLECTION_QUERY_KEY,
    queryFn: collectionApi.getAll,
  });
}

export function useAddToCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (gameId: string) => collectionApi.addGame(gameId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
    },
  });
}

export function useRemoveFromCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (gameId: string) => collectionApi.removeGame(gameId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
    },
  });
}

export function usePlaySessions(gameId: string) {
  return useQuery({
    queryKey: [...COLLECTION_QUERY_KEY, gameId, 'play-sessions'],
    queryFn: () => collectionApi.getPlaySessions(gameId),
    enabled: !!gameId,
  });
}

export function useAddPlaySession(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (session: CreatePlaySessionDto) =>
      collectionApi.addPlaySession(gameId, session),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [...COLLECTION_QUERY_KEY, gameId, 'play-sessions'],
      });
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['games', gameId] });
    },
  });
}

export function useDeletePlaySession(gameId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (sessionId: string) =>
      collectionApi.deletePlaySession(gameId, sessionId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [...COLLECTION_QUERY_KEY, gameId, 'play-sessions'],
      });
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
    },
  });
}
