import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { featuredApi } from '../api/featured';

const FEATURED_KEY = ['featured-games'];

export function useFeaturedGames() {
  return useQuery({
    queryKey: FEATURED_KEY,
    queryFn: featuredApi.getFeatured,
  });
}

export function useAddFeatured() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (gameId: string) => featuredApi.addFeatured(gameId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: FEATURED_KEY }),
  });
}

export function useRemoveFeatured() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (gameId: string) => featuredApi.removeFeatured(gameId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: FEATURED_KEY }),
  });
}
