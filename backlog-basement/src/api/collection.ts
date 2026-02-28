import { api } from './client';
import { CollectionItemDto, CreatePlaySessionDto, GameStatus, PagedCollectionResult, CollectionStatsDto } from '../types';

export interface WithXp<T> {
  data: T;
  xpAwarded: number;
}

export const collectionApi = {
  /**
   * Get all games in the user's collection
   */
  getAll: () => api.get<CollectionItemDto[]>('/collection'),

  /**
   * Add a game to the user's collection
   */
  addGame: (gameId: string) =>
    api.postWithXp<CollectionItemDto>(`/collection/${gameId}`, {}),

  /**
   * Remove a game from the user's collection
   */
  removeGame: (gameId: string) => api.delete<void>(`/collection/${gameId}`),

  /**
   * Get play sessions for a game in the collection
   */
  getPlaySessions: async (gameId: string) => {
    const response = await api.get<any[]>(`/collection/${gameId}/play-sessions`);
    // Transform backend response to match frontend types
    return response.map(session => ({
      id: session.id,
      collectionItemId: session.userGameId, // Map userGameId to collectionItemId
      durationMinutes: session.durationMinutes,
      datePlayed: new Date(session.playedAt).toISOString(), // Map playedAt to datePlayed with ISO format
      createdAt: new Date(session.playedAt).toISOString(), // Map playedAt to createdAt
    }));
  },

  /**
   * Log a new play session for a game
   */
  addPlaySession: async (gameId: string, session: CreatePlaySessionDto) => {
    // Transform frontend request to backend format
    const backendSession = {
      durationMinutes: session.durationMinutes,
      playedAt: new Date(session.datePlayed) // Map datePlayed to playedAt
    };

    const result = await api.postWithXp<any>(`/collection/${gameId}/play-sessions`, backendSession);

    // Transform backend response to match frontend types
    return {
      data: {
        id: result.data.id,
        collectionItemId: result.data.userGameId,
        durationMinutes: result.data.durationMinutes,
        datePlayed: new Date(result.data.playedAt).toISOString(),
        createdAt: new Date(result.data.playedAt).toISOString(),
      },
      xpAwarded: result.xpAwarded,
    };
  },

  /**
   * Bulk add games to collection, skipping duplicates
   */
  bulkAddGames: (gameIds: string[]) =>
    api.post<{ added: number; alreadyOwned: number }>('/collection/bulk-add', { gameIds }),

  /**
   * Delete a play session
   */
  deletePlaySession: (gameId: string, sessionId: string) =>
    api.delete<void>(`/collection/${gameId}/play-sessions/${sessionId}`),

  /**
   * Update the status of a game in the collection
   */
  updateStatus: (gameId: string, status: GameStatus) =>
    api.patchWithXp<CollectionItemDto>(`/collection/${gameId}/status`, { status }),

  /**
   * Get a paginated/filtered page of the collection
   */
  getPaged: (params: {
    skip: number;
    take: number;
    search?: string;
    status?: string;
    source?: string;
    playStatus?: string;
    sortBy?: string;
    sortDir?: string;
  }): Promise<PagedCollectionResult> => {
    const query = new URLSearchParams();
    query.set('skip', String(params.skip));
    query.set('take', String(params.take));
    if (params.search) query.set('search', params.search);
    if (params.status) query.set('status', params.status);
    if (params.source) query.set('source', params.source);
    if (params.playStatus) query.set('playStatus', params.playStatus);
    if (params.sortBy) query.set('sortBy', params.sortBy);
    if (params.sortDir) query.set('sortDir', params.sortDir);
    return api.get<PagedCollectionResult>(`/collection/paged?${query}`);
  },

  /**
   * Get collection counts for stat display
   */
  getStats: (): Promise<CollectionStatsDto> => api.get<CollectionStatsDto>('/collection/stats'),
};
