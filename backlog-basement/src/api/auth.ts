import axios from 'axios';
import { UserDto } from '../types';

// Auth endpoints live at /auth/* (not /api/auth/*), so derive the base URL
// by stripping the /api suffix from the API base URL.
const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';
const AUTH_BASE_URL = API_BASE.endsWith('/api')
  ? API_BASE.slice(0, -4)
  : API_BASE.replace(/\/api$/, '');

const authClient = axios.create({
  baseURL: AUTH_BASE_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authApi = {
  /**
   * Get the current authenticated user
   */
  getCurrentUser: async (): Promise<{ data: UserDto; xpAwarded: number }> => {
    const response = await authClient.get<UserDto>('/auth/me');
    const raw = response.headers['x-xp-awarded'];
    const xpAwarded = raw ? parseInt(raw, 10) : 0;
    return { data: response.data, xpAwarded: isNaN(xpAwarded) ? 0 : xpAwarded };
  },

  /**
   * Get the Google OAuth login URL
   * User should be redirected to this URL to initiate login
   */
  getLoginUrl: () => `${AUTH_BASE_URL}/auth/login/google`,

  /**
   * Get the Steam OpenID login URL
   */
  getSteamLoginUrl: () => `${AUTH_BASE_URL}/auth/login/steam`,

  /**
   * Get the Twitch OAuth login URL
   */
  getTwitchLoginUrl: () => `${AUTH_BASE_URL}/auth/login/twitch`,

  /**
   * Get the Twitch OAuth link URL (for linking to an existing account)
   */
  getTwitchLinkUrl: () => `${AUTH_BASE_URL}/auth/link-twitch`,

  /**
   * Unlink Twitch account from the current user
   */
  unlinkTwitch: async (): Promise<void> => {
    await authClient.post('/auth/unlink-twitch');
  },

  /**
   * Logout the current user
   */
  logout: async (): Promise<void> => {
    await authClient.post('/auth/logout');
  },
};
