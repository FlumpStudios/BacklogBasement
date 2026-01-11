import { api } from './client';
import { GameDto } from '../types';

export const gamesApi = {
  /**
   * Search for games via IGDB-backed search
   */
  search: (query: string) =>
    api.get<GameDto[]>('/games/search', {
      params: { query },
    }),

  /**
   * Get detailed information about a specific game
   */
  getById: (id: string) => api.get<GameDto>(`/games/${id}`),
};
