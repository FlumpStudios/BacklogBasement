import { api } from './client';
import { GameDto } from '../types';

export const featuredApi = {
  getFeatured: () =>
    api.get<GameDto[]>('/featured'),

  searchGames: (q: string) =>
    api.get<GameDto[]>('/admin/games/search', { params: { q } }),

  addFeatured: (gameId: string) =>
    api.post(`/admin/featured/${gameId}`, {}),

  removeFeatured: (gameId: string) =>
    api.delete(`/admin/featured/${gameId}`),
};
