import { api } from './client';
import { CollectionItemDto, CreatePlaySessionDto } from '../types';

export const collectionApi = {
  /**
   * Get all games in the user's collection
   */
  getAll: () => api.get<CollectionItemDto[]>('/collection'),

  /**
   * Add a game to the user's collection
   */
  addGame: (gameId: string) =>
    api.post<CollectionItemDto>(`/collection/${gameId}`, {}),

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
    
    const response = await api.post<any>(`/collection/${gameId}/play-sessions`, backendSession);
    
    // Transform backend response to match frontend types
    return {
      id: response.id,
      collectionItemId: response.userGameId,
      durationMinutes: response.durationMinutes,
      datePlayed: new Date(response.playedAt).toISOString(),
      createdAt: new Date(response.playedAt).toISOString(),
    };
  },

  /**
   * Delete a play session
   */
  deletePlaySession: (gameId: string, sessionId: string) =>
    api.delete<void>(`/collection/${gameId}/play-sessions/${sessionId}`),
};
