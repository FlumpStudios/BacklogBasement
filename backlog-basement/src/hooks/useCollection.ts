import { useQuery, useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { collectionApi } from '../api';
import { useToast } from '../components';
import { CreatePlaySessionDto, GameStatus } from '../types';

const PAGE_SIZE = 50;

export interface CollectionQueryFilters {
  search?: string;
  status?: string;
  source?: string;
  playStatus?: string;
  sortBy?: string;
  sortDir?: string;
}

export function useInfiniteCollection(filters: CollectionQueryFilters) {
  return useInfiniteQuery({
    queryKey: ['collection', 'paged', filters],
    queryFn: ({ pageParam }) =>
      collectionApi.getPaged({ skip: pageParam as number, take: PAGE_SIZE, ...filters }),
    initialPageParam: 0,
    getNextPageParam: (lastPage, allPages) =>
      lastPage.hasMore ? allPages.length * PAGE_SIZE : undefined,
  });
}

export function useCollectionStats() {
  return useQuery({
    queryKey: ['collection', 'stats'],
    queryFn: collectionApi.getStats,
  });
}

export const COLLECTION_QUERY_KEY = ['collection'];

export function useCollection() {
  return useQuery({
    queryKey: COLLECTION_QUERY_KEY,
    queryFn: collectionApi.getAll,
  });
}

export function useAddToCollection() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (gameId: string) => collectionApi.addGame(gameId),
    onSuccess: ({ xpAwarded }) => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
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
  const { showToast } = useToast();

  return useMutation({
    mutationFn: (session: CreatePlaySessionDto) =>
      collectionApi.addPlaySession(gameId, session),
    onSuccess: ({ xpAwarded }) => {
      queryClient.invalidateQueries({
        queryKey: [...COLLECTION_QUERY_KEY, gameId, 'play-sessions'],
      });
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['games', gameId] });
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
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

export function useUpdateGameStatus() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useMutation({
    mutationFn: ({ gameId, status }: { gameId: string; status: GameStatus }) =>
      collectionApi.updateStatus(gameId, status),
    onSuccess: ({ xpAwarded }) => {
      queryClient.invalidateQueries({ queryKey: COLLECTION_QUERY_KEY });
      if (xpAwarded > 0) showToast(`+${xpAwarded} XP`, 'success');
    },
  });
}
