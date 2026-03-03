import { useQuery } from '@tanstack/react-query';
import { activityApi } from '../api';

export const ACTIVITY_FEED_QUERY_KEY = ['activity', 'feed'];

export function useActivityFeed() {
  return useQuery({
    queryKey: ACTIVITY_FEED_QUERY_KEY,
    queryFn: () => activityApi.getFeed(),
    staleTime: 60_000,
  });
}
