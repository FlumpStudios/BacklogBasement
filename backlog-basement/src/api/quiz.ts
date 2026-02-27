import { api } from './client';
import { DailyQuizDto } from '../types';

export const quizApi = {
  getToday: () => api.get<DailyQuizDto | null>('/quiz/today'),
  getPrevious: () => api.get<DailyQuizDto | null>('/quiz/previous'),
  answer: (quizId: string, optionId: string) =>
    api.postWithXp<DailyQuizDto>(`/quiz/${quizId}/answer`, { optionId }),
};
