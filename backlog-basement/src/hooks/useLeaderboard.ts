import { useQuery } from '@tanstack/react-query';
import { leaderboardApi } from '../api';

export const GLOBAL_LEADERBOARD_QUERY_KEY = ['leaderboard', 'global'];
export const FRIEND_LEADERBOARD_QUERY_KEY = ['leaderboard', 'friends'];

export function useGlobalLeaderboard() {
  return useQuery({
    queryKey: GLOBAL_LEADERBOARD_QUERY_KEY,
    queryFn: leaderboardApi.getGlobal,
  });
}

export function useFriendLeaderboard() {
  return useQuery({
    queryKey: FRIEND_LEADERBOARD_QUERY_KEY,
    queryFn: leaderboardApi.getFriends,
  });
}
