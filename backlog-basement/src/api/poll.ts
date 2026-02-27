import { api } from './client';
import { DailyPollDto } from '../types';

export const pollApi = {
  getToday: () => api.get<DailyPollDto>('/poll/today'),
  getPrevious: () => api.get<DailyPollDto | null>('/poll/previous'),
  vote: (pollId: string, gameId: string) =>
    api.postWithXp<DailyPollDto>(`/poll/${pollId}/vote`, { gameId }),
};
