import { apiClient } from './client';
import { UserDto } from '../types';

const AUTH_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || '';

export const authApi = {
  /**
   * Get the current authenticated user
   */
  getCurrentUser: async (): Promise<UserDto> => {
    const response = await apiClient.get<UserDto>(`${AUTH_BASE_URL}/auth/me`);
    return response.data;
  },

  /**
   * Get the Google OAuth login URL
   * User should be redirected to this URL to initiate login
   */
  getLoginUrl: () => `${AUTH_BASE_URL}/auth/login/google`,

  /**
   * Logout the current user
   */
  logout: async (): Promise<void> => {
    await apiClient.post(`${AUTH_BASE_URL}/auth/logout`);
  },
};
