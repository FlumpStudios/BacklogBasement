import { api } from './client';
import { ActivityEventDto } from '../types';

export const activityApi = {
  getFeed: (limit = 50) => api.get<ActivityEventDto[]>(`/activity/feed?limit=${limit}`),
};
