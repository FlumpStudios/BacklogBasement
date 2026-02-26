import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { gameClubApi } from '../api';
import {
  CreateGameClubRequest,
  StartRoundRequest,
  SubmitReviewRequest,
  RespondToInviteRequest,
} from '../types';

export const PUBLIC_CLUBS_QUERY_KEY = ['public-clubs'];
export const MY_CLUBS_QUERY_KEY = ['my-clubs'];
export const CLUB_DETAIL_QUERY_KEY = (clubId: string) => ['club', clubId];
export const ROUND_REVIEWS_QUERY_KEY = (roundId: string) => ['club-reviews', roundId];
export const CLUB_SCORE_QUERY_KEY = (gameId: string) => ['club-score', gameId];
export const CLUB_REVIEWS_FOR_GAME_QUERY_KEY = (gameId: string) => ['club-reviews-for-game', gameId];
export const MY_CLUB_INVITES_QUERY_KEY = ['my-club-invites'];

export function usePublicClubs() {
  return useQuery({
    queryKey: PUBLIC_CLUBS_QUERY_KEY,
    queryFn: () => gameClubApi.getPublicClubs(),
  });
}

export function useMyClubs(enabled = true) {
  return useQuery({
    queryKey: MY_CLUBS_QUERY_KEY,
    queryFn: () => gameClubApi.getMyClubs(),
    enabled,
  });
}

export function useClub(clubId: string | undefined) {
  return useQuery({
    queryKey: CLUB_DETAIL_QUERY_KEY(clubId ?? ''),
    queryFn: () => gameClubApi.getClub(clubId!),
    enabled: !!clubId,
  });
}

export function useMyClubInvites() {
  return useQuery({
    queryKey: MY_CLUB_INVITES_QUERY_KEY,
    queryFn: () => gameClubApi.getMyPendingInvites(),
  });
}

export function useRoundReviews(clubId: string | undefined, roundId: string | undefined) {
  return useQuery({
    queryKey: ROUND_REVIEWS_QUERY_KEY(roundId ?? ''),
    queryFn: () => gameClubApi.getRoundReviews(clubId!, roundId!),
    enabled: !!clubId && !!roundId,
  });
}

export function useClubScoreForGame(gameId: string | undefined) {
  return useQuery({
    queryKey: CLUB_SCORE_QUERY_KEY(gameId ?? ''),
    queryFn: () => gameClubApi.getClubScoreForGame(gameId!),
    enabled: !!gameId,
    retry: false,
  });
}

export function useClubReviewsForGame(gameId: string | undefined) {
  return useQuery({
    queryKey: CLUB_REVIEWS_FOR_GAME_QUERY_KEY(gameId ?? ''),
    queryFn: () => gameClubApi.getClubReviewsForGame(gameId!),
    enabled: !!gameId,
    retry: false,
  });
}

export function useCreateClub() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateGameClubRequest) => gameClubApi.createClub(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MY_CLUBS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: PUBLIC_CLUBS_QUERY_KEY });
    },
  });
}

export function useJoinClub() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (clubId: string) => gameClubApi.joinClub(clubId),
    onSuccess: (_, clubId) => {
      queryClient.invalidateQueries({ queryKey: MY_CLUBS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: PUBLIC_CLUBS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useInviteMember(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (inviteeUserId: string) => gameClubApi.inviteMember(clubId, inviteeUserId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useRespondToInvite() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ clubId, inviteId, request }: { clubId: string; inviteId: string; request: RespondToInviteRequest }) =>
      gameClubApi.respondToInvite(clubId, inviteId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MY_CLUB_INVITES_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: MY_CLUBS_QUERY_KEY });
    },
  });
}

export function useRemoveMember(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (targetUserId: string) => gameClubApi.removeMember(clubId, targetUserId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useUpdateMemberRole(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ targetUserId, role }: { targetUserId: string; role: string }) =>
      gameClubApi.updateMemberRole(clubId, targetUserId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useStartRound(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: StartRoundRequest) => gameClubApi.startRound(clubId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
      queryClient.invalidateQueries({ queryKey: MY_CLUBS_QUERY_KEY });
    },
  });
}

export function useAdvanceRound(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (roundId: string) => gameClubApi.advanceRound(clubId, roundId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useNominateGame(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ roundId, gameId }: { roundId: string; gameId: string }) =>
      gameClubApi.nominateGame(clubId, roundId, gameId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useVote(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ roundId, nominationId }: { roundId: string; nominationId: string }) =>
      gameClubApi.vote(clubId, roundId, nominationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
    },
  });
}

export function useDeleteClub() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (clubId: string) => gameClubApi.deleteClub(clubId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MY_CLUBS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: PUBLIC_CLUBS_QUERY_KEY });
    },
  });
}

export function useSubmitReview(clubId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ roundId, request }: { roundId: string; request: SubmitReviewRequest }) =>
      gameClubApi.submitReview(clubId, roundId, request),
    onSuccess: (_, { roundId }) => {
      queryClient.invalidateQueries({ queryKey: CLUB_DETAIL_QUERY_KEY(clubId) });
      queryClient.invalidateQueries({ queryKey: ROUND_REVIEWS_QUERY_KEY(roundId) });
    },
  });
}
