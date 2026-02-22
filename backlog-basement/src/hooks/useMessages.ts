import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { messagesApi } from '../api';
import { SendMessageRequest } from '../types';

export const CONVERSATIONS_QUERY_KEY = ['messages', 'conversations'];
export const UNREAD_MESSAGES_COUNT_QUERY_KEY = ['messages', 'unread-count'];
export const MESSAGES_QUERY_KEY = (userId: string) => ['messages', 'thread', userId];

export function useConversations() {
  return useQuery({
    queryKey: CONVERSATIONS_QUERY_KEY,
    queryFn: () => messagesApi.getConversations(),
  });
}

export function useMessages(userId: string | undefined) {
  const queryClient = useQueryClient();

  return useQuery({
    queryKey: userId ? MESSAGES_QUERY_KEY(userId) : ['messages', 'thread', 'none'],
    queryFn: async () => {
      const messages = await messagesApi.getMessages(userId!);
      // Invalidate unread count and conversations when thread is fetched (marks as read)
      queryClient.invalidateQueries({ queryKey: UNREAD_MESSAGES_COUNT_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: CONVERSATIONS_QUERY_KEY });
      return messages;
    },
    enabled: !!userId,
    refetchInterval: 5000, // poll every 5s while thread is open
  });
}

export function useSendMessage(recipientId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: SendMessageRequest) =>
      messagesApi.sendMessage(recipientId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MESSAGES_QUERY_KEY(recipientId) });
      queryClient.invalidateQueries({ queryKey: CONVERSATIONS_QUERY_KEY });
    },
  });
}

export function useUnreadMessageCount(enabled = true) {
  return useQuery({
    queryKey: UNREAD_MESSAGES_COUNT_QUERY_KEY,
    queryFn: () => messagesApi.getUnreadCount(),
    refetchInterval: 30000, // 30s polling
    enabled,
  });
}
