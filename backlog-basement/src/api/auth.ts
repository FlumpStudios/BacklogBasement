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
  getCurrentUser: async (): Promise<UserDto> => {
    const response = await authClient.get<UserDto>('/auth/me');
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
    await authClient.post('/auth/logout');
  },
};
