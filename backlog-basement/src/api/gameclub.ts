import { api } from './client';
import {
  GameClubDto,
  GameClubDetailDto,
  GameClubRoundDto,
  GameClubNominationDto,
  GameClubReviewDto,
  GameClubInviteDto,
  GameClubScoreDto,
  GameClubReviewsForGameDto,
  CreateGameClubRequest,
  StartRoundRequest,
  SubmitReviewRequest,
  RespondToInviteRequest,
} from '../types';

export const gameClubApi = {
  getPublicClubs: () =>
    api.get<GameClubDto[]>('/clubs'),

  getMyClubs: () =>
    api.get<GameClubDto[]>('/clubs/my'),

  getClub: (clubId: string) =>
    api.get<GameClubDetailDto>(`/clubs/${clubId}`),

  createClub: (request: CreateGameClubRequest) =>
    api.post<GameClubDetailDto>('/clubs', request),

  joinClub: (clubId: string) =>
    api.post<{ message: string }>(`/clubs/${clubId}/join`),

  inviteMember: (clubId: string, inviteeUserId: string) =>
    api.post<GameClubInviteDto>(`/clubs/${clubId}/invite`, { inviteeUserId }),

  getMyPendingInvites: () =>
    api.get<GameClubInviteDto[]>('/clubs/invites'),

  respondToInvite: (clubId: string, inviteId: string, request: RespondToInviteRequest) =>
    api.post<{ message: string }>(`/clubs/${clubId}/invites/${inviteId}/respond`, request),

  removeMember: (clubId: string, targetUserId: string) =>
    api.delete<void>(`/clubs/${clubId}/members/${targetUserId}`),

  updateMemberRole: (clubId: string, targetUserId: string, role: string) =>
    api.put<{ message: string }>(`/clubs/${clubId}/members/${targetUserId}/role`, { role }),

  transferOwnership: (clubId: string, newOwnerId: string) =>
    api.post<{ message: string }>(`/clubs/${clubId}/transfer`, { newOwnerId }),

  startRound: (clubId: string, request: StartRoundRequest) =>
    api.post<GameClubRoundDto>(`/clubs/${clubId}/rounds`, request),

  advanceRound: (clubId: string, roundId: string) =>
    api.post<GameClubRoundDto>(`/clubs/${clubId}/rounds/${roundId}/advance`),

  nominateGame: (clubId: string, roundId: string, gameId: string) =>
    api.post<GameClubNominationDto>(`/clubs/${clubId}/rounds/${roundId}/nominate`, { gameId }),

  vote: (clubId: string, roundId: string, nominationId: string) =>
    api.post<{ id: string; nominationId: string; createdAt: string }>(
      `/clubs/${clubId}/rounds/${roundId}/vote`,
      { nominationId }
    ),

  submitReview: (clubId: string, roundId: string, request: SubmitReviewRequest) =>
    api.post<GameClubReviewDto>(`/clubs/${clubId}/rounds/${roundId}/review`, request),

  getRoundReviews: (clubId: string, roundId: string) =>
    api.get<GameClubReviewDto[]>(`/clubs/${clubId}/rounds/${roundId}/reviews`),

  getClubScoreForGame: (gameId: string) =>
    api.get<GameClubScoreDto>(`/clubs/score/${gameId}`),

  getClubReviewsForGame: (gameId: string) =>
    api.get<GameClubReviewsForGameDto[]>(`/clubs/reviews-for-game/${gameId}`),

  deleteClub: (clubId: string) =>
    api.delete<void>(`/clubs/${clubId}`),
};
