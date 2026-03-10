import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { passwordsApi, CreateGamePasswordRequest } from '../api/passwords';

const passwordsKey = (gameId: string) => ['game-passwords', gameId];
const publicPasswordsKey = (gameId: string) => ['game-passwords-public', gameId];

export function useGamePasswords(gameId: string) {
  return useQuery({
    queryKey: passwordsKey(gameId),
    queryFn: () => passwordsApi.getPasswords(gameId),
  });
}

export function usePublicGamePasswords(gameId: string) {
  return useQuery({
    queryKey: publicPasswordsKey(gameId),
    queryFn: () => passwordsApi.getPublicPasswords(gameId),
  });
}

export function useAddGamePassword(gameId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateGamePasswordRequest) =>
      passwordsApi.addPassword(gameId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: passwordsKey(gameId) });
      if (variables.isPublic) {
        queryClient.invalidateQueries({ queryKey: publicPasswordsKey(gameId) });
      }
    },
  });
}

export function useDeleteGamePassword(gameId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (passwordId: string) =>
      passwordsApi.deletePassword(gameId, passwordId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: passwordsKey(gameId) });
      queryClient.invalidateQueries({ queryKey: publicPasswordsKey(gameId) });
    },
  });
}
