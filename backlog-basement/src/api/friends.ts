import { api } from './client';
import {
  FriendDto,
  FriendRequestDto,
  FriendshipStatusDto,
  PlayerSearchResultDto,
} from '../types';

export const friendsApi = {
  searchPlayers: (query: string) =>
    api.get<PlayerSearchResultDto[]>(`/friends/search?q=${encodeURIComponent(query)}`),

  getFriendshipStatus: (userId: string) =>
    api.get<FriendshipStatusDto>(`/friends/status/${userId}`),

  sendRequest: (userId: string) =>
    api.post<FriendRequestDto>(`/friends/request/${userId}`),

  acceptRequest: (id: string) =>
    api.post<{ message: string }>(`/friends/${id}/accept`),

  declineRequest: (id: string) =>
    api.post<{ message: string }>(`/friends/${id}/decline`),

  removeFriend: (id: string) =>
    api.delete<void>(`/friends/${id}`),

  getFriends: () =>
    api.get<FriendDto[]>('/friends'),

  getPendingRequests: () =>
    api.get<FriendRequestDto[]>('/friends/requests'),
};
