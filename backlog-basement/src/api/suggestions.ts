import { api } from './client';
import { GameSuggestionDto, SendGameSuggestionRequest } from '../types';

export const suggestionsApi = {
  send: (request: SendGameSuggestionRequest) =>
    api.postWithXp<GameSuggestionDto>('/suggestions', request),

  getReceived: () =>
    api.get<GameSuggestionDto[]>('/suggestions'),

  dismiss: (id: string) =>
    api.post<{ message: string }>(`/suggestions/${id}/dismiss`),
};
