import { api } from './client';
import { NotificationDto, UnreadCountDto } from '../types';

export const notificationsApi = {
  getAll: (limit?: number) =>
    api.get<NotificationDto[]>(`/notifications${limit ? `?limit=${limit}` : ''}`),

  getUnreadCount: () =>
    api.get<UnreadCountDto>('/notifications/unread-count'),

  markAsRead: (id: string) =>
    api.post<{ message: string }>(`/notifications/${id}/read`),

  markAllAsRead: () =>
    api.post<{ message: string }>('/notifications/read-all'),
};
