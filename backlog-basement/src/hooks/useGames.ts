import { useQuery } from '@tanstack/react-query';
import { gamesApi } from '../api';
import { useDebounce } from './useDebounce';

export function useGameSearch(query: string) {
  const debouncedQuery = useDebounce(query, 300);

  return useQuery({
    queryKey: ['games', 'search', debouncedQuery],
    queryFn: () => gamesApi.search(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
    staleTime: 60 * 1000, // 1 minute
  });
}

export function useGameDetails(id: string) {
  return useQuery({
    queryKey: ['games', id],
    queryFn: () => gamesApi.getById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
