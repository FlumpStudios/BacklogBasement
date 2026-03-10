import { api } from './client';

export interface GamePasswordDto {
  id: string;
  gameId: string;
  password: string;
  label: string | null;
  notes: string | null;
  isPublic: boolean;
  submittedBy: string | null;
  createdAt: string;
}

export interface CreateGamePasswordRequest {
  password: string;
  label?: string;
  notes?: string;
  isPublic?: boolean;
}

export const passwordsApi = {
  getPasswords: (gameId: string) =>
    api.get<GamePasswordDto[]>(`/games/${gameId}/passwords`),

  getPublicPasswords: (gameId: string) =>
    api.get<GamePasswordDto[]>(`/games/${gameId}/passwords/public`),

  addPassword: (gameId: string, data: CreateGamePasswordRequest) =>
    api.post<GamePasswordDto>(`/games/${gameId}/passwords`, data),

  deletePassword: (gameId: string, passwordId: string) =>
    api.delete(`/games/${gameId}/passwords/${passwordId}`),
};
