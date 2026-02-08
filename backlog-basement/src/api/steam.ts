import { apiClient } from './client';
import { SteamStatusDto, SteamImportRequest, SteamImportResult, SteamPlaytimeSyncResult, SteamBulkPlaytimeSyncResult } from '../types';

const AUTH_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || '';

export const steamApi = {
  /**
   * Get the Steam link URL
   * User should be redirected to this URL to initiate Steam linking
   */
  getLinkUrl: () => `${AUTH_BASE_URL}/auth/link-steam`,

  /**
   * Unlink Steam account from user
   */
  unlink: async (): Promise<void> => {
    await apiClient.delete(`${AUTH_BASE_URL}/auth/unlink-steam`);
  },

  /**
   * Get Steam link status
   */
  getStatus: async (): Promise<SteamStatusDto> => {
    const response = await apiClient.get<SteamStatusDto>('/steam/status');
    return response.data;
  },

  /**
   * Import Steam library
   */
  importLibrary: async (request: SteamImportRequest): Promise<SteamImportResult> => {
    const response = await apiClient.post<SteamImportResult>('/steam/import', request);
    return response.data;
  },

  /**
   * Sync playtime from Steam for a specific game
   */
  syncPlaytime: async (gameId: string): Promise<SteamPlaytimeSyncResult> => {
    const response = await apiClient.post<SteamPlaytimeSyncResult>(`/steam/${gameId}/sync-playtime`);
    return response.data;
  },

  syncAllPlaytime: async (): Promise<SteamBulkPlaytimeSyncResult> => {
    const response = await apiClient.post<SteamBulkPlaytimeSyncResult>('/steam/sync-all-playtime');
    return response.data;
  },
};
