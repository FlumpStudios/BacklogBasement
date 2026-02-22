import { api } from './client';
import { ConversationDto, DirectMessageDto, SendMessageRequest, UnreadMessageCountDto } from '../types';

export const messagesApi = {
  getConversations: () =>
    api.get<ConversationDto[]>('/messages'),

  getMessages: (userId: string) =>
    api.get<DirectMessageDto[]>(`/messages/${userId}`),

  sendMessage: (userId: string, request: SendMessageRequest) =>
    api.post<DirectMessageDto>(`/messages/${userId}`, request),

  markAsRead: (userId: string) =>
    api.post<{ message: string }>(`/messages/${userId}/read`),

  getUnreadCount: () =>
    api.get<UnreadMessageCountDto>('/messages/unread-count'),
};
