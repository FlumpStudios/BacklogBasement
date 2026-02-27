import { api } from './client';
import { LeaderboardEntryDto } from '../types';

export const leaderboardApi = {
  getGlobal: () => api.get<LeaderboardEntryDto[]>('/leaderboard'),
  getFriends: () => api.get<LeaderboardEntryDto[]>('/leaderboard/friends'),
};
