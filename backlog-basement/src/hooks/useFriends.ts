import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { friendsApi } from '../api';
import { useDebounce } from './useDebounce';

export const FRIENDS_QUERY_KEY = ['friends'];
export const FRIEND_REQUESTS_QUERY_KEY = ['friend-requests'];
const NOTIFICATIONS_QUERY_KEY = ['notifications'];

export function usePlayerSearch(query: string) {
  const debouncedQuery = useDebounce(query, 400);

  return useQuery({
    queryKey: ['player-search', debouncedQuery],
    queryFn: () => friendsApi.searchPlayers(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
  });
}

export function useFriendshipStatus(userId: string | undefined) {
  return useQuery({
    queryKey: ['friendship-status', userId],
    queryFn: () => friendsApi.getFriendshipStatus(userId!),
    enabled: !!userId,
  });
}

export function useSendFriendRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => friendsApi.sendRequest(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FRIENDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: FRIEND_REQUESTS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['friendship-status'] });
    },
  });
}

export function useAcceptFriendRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => friendsApi.acceptRequest(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FRIENDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: FRIEND_REQUESTS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['friendship-status'] });
      queryClient.invalidateQueries({ queryKey: ['profile'] });
    },
  });
}

export function useDeclineFriendRequest() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => friendsApi.declineRequest(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FRIENDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: FRIEND_REQUESTS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['friendship-status'] });
    },
  });
}

export function useRemoveFriend() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => friendsApi.removeFriend(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: FRIENDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: FRIEND_REQUESTS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: ['friendship-status'] });
      queryClient.invalidateQueries({ queryKey: ['profile'] });
    },
  });
}

export function useFriends() {
  return useQuery({
    queryKey: FRIENDS_QUERY_KEY,
    queryFn: () => friendsApi.getFriends(),
  });
}

export function usePendingRequests() {
  return useQuery({
    queryKey: FRIEND_REQUESTS_QUERY_KEY,
    queryFn: () => friendsApi.getPendingRequests(),
  });
}

export const STEAM_SUGGESTIONS_QUERY_KEY = ['steam-friend-suggestions'];

export function useSteamFriendSuggestions() {
  return useQuery({
    queryKey: STEAM_SUGGESTIONS_QUERY_KEY,
    queryFn: () => friendsApi.getSteamSuggestions(),
    enabled: false,
    retry: false,
  });
}
