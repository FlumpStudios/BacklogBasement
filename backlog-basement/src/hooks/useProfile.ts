import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { profileApi } from '../api';
import { useDebounce } from './useDebounce';
import { AUTH_QUERY_KEY } from '../auth/AuthContext';

export const PROFILE_QUERY_KEY = ['profile'];

export function useProfile(username: string) {
  return useQuery({
    queryKey: [...PROFILE_QUERY_KEY, username],
    queryFn: () => profileApi.getByUsername(username),
    enabled: !!username,
  });
}

export function useCheckUsername(username: string) {
  const debouncedUsername = useDebounce(username, 400);

  return useQuery({
    queryKey: ['check-username', debouncedUsername],
    queryFn: () => profileApi.checkUsername(debouncedUsername),
    enabled: debouncedUsername.length >= 3,
    retry: false,
  });
}

export function useSetUsername() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (username: string) => profileApi.setUsername(username),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
    },
  });
}
