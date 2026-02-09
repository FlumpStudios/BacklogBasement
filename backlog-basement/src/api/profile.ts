import { api } from './client';
import { ProfileDto, UsernameAvailabilityResponse } from '../types';

export const profileApi = {
  getByUsername: (username: string) =>
    api.get<ProfileDto>(`/profile/${username}`),

  checkUsername: (username: string) =>
    api.get<UsernameAvailabilityResponse>(`/profile/check-username/${username}`),

  setUsername: (username: string) =>
    api.post<{ username: string }>('/profile/set-username', { username }),
};
