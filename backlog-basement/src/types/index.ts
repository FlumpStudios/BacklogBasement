// Game DTOs - matches backend GameDto/GameSummaryDto
export interface GameDto {
  id: string;
  igdbId?: number | null;
  steamAppId?: number | null;
  name: string;
  coverUrl?: string | null;
  releaseDate?: string | null;
  summary?: string;
  criticScore?: number | null;
}

export interface GameDetailDto extends GameDto {
  totalPlaytimeMinutes?: number;
  isInCollection?: boolean;
}

// Collection DTOs - matches backend CollectionItemDto (flat structure)
export interface CollectionItemDto {
  id: string;
  gameId: string;
  gameName: string;
  releaseDate?: string | null;
  coverUrl?: string | null;
  dateAdded: string;
  notes?: string | null;
  totalPlayTimeMinutes: number;
  source: 'steam' | 'manual';
  status?: 'backlog' | 'playing' | 'completed' | null;
  dateCompleted?: string | null;
  criticScore?: number | null;
}

export type GameStatus = 'backlog' | 'playing' | 'completed' | null;

// Play Session DTOs - maps from backend PlaySessionDto
export interface PlaySessionDto {
  id: string;
  collectionItemId: string; // maps from UserGameId
  durationMinutes: number;
  datePlayed: string; // maps from PlayedAt
  createdAt: string; // maps from PlayedAt for now (could add separate timestamp)
}

export interface CreatePlaySessionDto {
  durationMinutes: number;
  datePlayed: string;
}

// XP / Level DTOs
export interface XpInfoDto {
  level: number;
  levelName: string;
  nextLevelName: string;
  xpTotal: number;
  xpForCurrentLevel: number;
  xpForNextLevel: number;
  xpIntoCurrentLevel: number;
  xpNeededForNextLevel: number;
  progressPercent: number;
  isMaxLevel: boolean;
}

// User DTOs
export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  steamId?: string | null;
  hasSteamLinked?: boolean;
  twitchId?: string | null;
  hasTwitchLinked?: boolean;
  username?: string | null;
  xpInfo?: XpInfoDto;
}

// Twitch DTOs
export interface TwitchLiveDto {
  isLive: boolean;
  streamTitle?: string | null;
  gameName?: string | null;
  igdbGameId?: number | null;
  viewerCount: number;
  twitchLogin?: string | null;
  thumbnailUrl?: string | null;
  updatedPlayingStatus: boolean;
}

export interface TwitchImportResultDto {
  total: number;
  imported: number;
  skipped: number;
  importedGames: TwitchImportedGameDto[];
}

export interface TwitchImportedGameDto {
  name: string;
  igdbId: number;
  streamedMinutes: number;
}

// Profile DTOs
export interface ProfileDto {
  userId: string;
  username: string;
  displayName: string;
  twitchId?: string | null;
  memberSince: string;
  stats: ProfileStatsDto;
  currentlyPlaying: CollectionItemDto[];
  backlog: CollectionItemDto[];
  collection: CollectionItemDto[];
  friends: FriendDto[];
  xpInfo: XpInfoDto;
}

export interface ProfileStatsDto {
  totalGames: number;
  totalPlayTimeMinutes: number;
  backlogCount: number;
  playingCount: number;
  completedCount: number;
  friendCount: number;
}

// Friend DTOs
export interface FriendDto {
  userId: string;
  username: string;
  displayName: string;
  friendsSince: string;
}

export interface FriendRequestDto {
  friendshipId: string;
  userId: string;
  username: string;
  displayName: string;
  sentAt: string;
  direction: 'incoming' | 'outgoing';
}

export interface FriendshipStatusDto {
  status: 'none' | 'pending_outgoing' | 'pending_incoming' | 'friends';
  friendshipId?: string;
}

export interface PlayerSearchResultDto {
  userId: string;
  username: string;
  displayName: string;
  totalGames: number;
}

export interface SteamFriendSuggestionsDto {
  isPrivate: boolean;
  suggestions: PlayerSearchResultDto[];
}

// Notification DTOs
export interface NotificationDto {
  id: string;
  type: string;
  message: string;
  relatedUserId?: string;
  relatedUsername?: string;
  relatedGameId?: string;
  relatedClubId?: string;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadCountDto {
  count: number;
}

export interface UsernameAvailabilityResponse {
  available: boolean;
  username: string;
}

// Steam DTOs
export interface SteamStatusDto {
  isLinked: boolean;
  steamId?: string | null;
}

export interface SteamImportRequest {
  includePlaytime: boolean;
}

export interface SteamImportResult {
  totalGames: number;
  importedCount: number;
  updatedCount: number;
  skippedCount: number;
  failedCount: number;
  importedGames: SteamImportedGameDto[];
  skippedGames: SteamSkippedGameDto[];
  failedGames: SteamFailedGameDto[];
}

export interface SteamImportedGameDto {
  gameId: string;
  name: string;
  steamAppId: number;
  igdbId?: number | null;
  matchedToIgdb: boolean;
  playtimeMinutes?: number | null;
}

export interface SteamSkippedGameDto {
  name: string;
  steamAppId: number;
  reason: string;
}

export interface SteamFailedGameDto {
  name: string;
  steamAppId: number;
  error: string;
}

export interface SteamPlaytimeSyncResult {
  success: boolean;
  playtimeMinutes: number;
  error?: string;
}

export interface SteamBulkPlaytimeSyncResult {
  totalGames: number;
  updatedCount: number;
  failedCount: number;
}

// Game Suggestion DTOs
export interface GameSuggestionDto {
  id: string;
  senderUserId: string;
  senderUsername: string;
  senderDisplayName: string;
  gameId: string;
  gameName: string;
  coverUrl?: string | null;
  message?: string | null;
  createdAt: string;
}

export interface SendGameSuggestionRequest {
  recipientUserId: string;
  gameId: string;
  message?: string;
}

// Game Club DTOs
export interface GameClubDto {
  id: string;
  name: string;
  description?: string | null;
  isPublic: boolean;
  discordLink?: string | null;
  whatsAppLink?: string | null;
  redditLink?: string | null;
  youTubeLink?: string | null;
  ownerDisplayName: string;
  ownerUsername: string;
  memberCount: number;
  currentRound?: GameClubRoundDto | null;
}

export interface GameClubDetailDto extends GameClubDto {
  members: GameClubMemberDto[];
  pendingInvites: GameClubInviteDto[];
  rounds: GameClubRoundDto[];
  currentUserRole?: string | null;
}

export interface GameClubRoundDto {
  id: string;
  roundNumber: number;
  status: 'nominating' | 'voting' | 'playing' | 'reviewing' | 'completed';
  gameId?: string | null;
  gameName?: string | null;
  gameCoverUrl?: string | null;
  nominations: GameClubNominationDto[];
  userHasVoted: boolean;
  userHasReviewed: boolean;
  userHasNominated: boolean;
  userVotedNominationId?: string | null;
  nominatingDeadline?: string | null;
  votingDeadline?: string | null;
  playingDeadline?: string | null;
  reviewingDeadline?: string | null;
  completedAt?: string | null;
  averageScore?: number | null;
}

export interface GameClubNominationDto {
  id: string;
  gameId: string;
  gameName: string;
  gameCoverUrl?: string | null;
  nominatedByUserId: string;
  nominatedByDisplayName: string;
  nominatedByUsername: string;
  voteCount: number;
  createdAt: string;
}

export interface GameClubMemberDto {
  userId: string;
  displayName: string;
  username: string;
  role: 'owner' | 'admin' | 'member';
  joinedAt: string;
}

export interface GameClubReviewDto {
  id: string;
  userId: string;
  displayName: string;
  username: string;
  score: number;
  comment?: string | null;
  submittedAt: string;
}

export interface GameClubInviteDto {
  id: string;
  clubId: string;
  clubName: string;
  invitedByUserId: string;
  invitedByDisplayName: string;
  inviteeUserId: string;
  inviteeDisplayName: string;
  status: 'pending' | 'accepted' | 'declined';
  createdAt: string;
}

export interface GameClubScoreDto {
  gameId: string;
  averageScore: number;
  reviewCount: number;
  roundCount: number;
}

export interface GameClubReviewsForGameDto {
  clubId: string;
  clubName: string;
  isPublic: boolean;
  averageScore: number;
  reviewCount: number;
  isCurrentUserMember: boolean;
  reviews: GameClubReviewDto[];
}

export interface CreateGameClubRequest {
  name: string;
  description?: string;
  isPublic: boolean;
  discordLink?: string;
  whatsAppLink?: string;
  redditLink?: string;
  youTubeLink?: string;
}

export interface StartRoundRequest {
  nominatingDeadline?: string;
  votingDeadline?: string;
  playingDeadline?: string;
  reviewingDeadline?: string;
}

export interface SubmitReviewRequest {
  score: number;
  comment?: string;
}

export interface RespondToInviteRequest {
  accept: boolean;
}

// Direct Message DTOs
export interface ConversationDto {
  friendUserId: string;
  friendUsername: string;
  friendDisplayName: string;
  lastMessageContent: string;
  lastMessageIsFromMe: boolean;
  lastMessageAt: string;
  unreadCount: number;
}

export interface DirectMessageDto {
  id: string;
  senderId: string;
  senderDisplayName: string;
  content: string;
  isRead: boolean;
  createdAt: string;
}

export interface SendMessageRequest {
  content: string;
}

export interface UnreadMessageCountDto {
  count: number;
}

// Daily Poll DTOs
export interface DailyPollGameDto {
  gameId: string;
  name: string;
  coverUrl?: string | null;
}

export interface PollResultDto {
  gameId: string;
  voteCount: number;
  percentage: number;
}

export interface DailyPollDto {
  pollId: string;
  date: string;
  category: string;
  games: DailyPollGameDto[];
  userVotedGameId?: string | null;
  results?: PollResultDto[] | null;
}

// Daily Quiz DTOs
export interface DailyQuizOptionDto {
  optionId: string;
  text: string;
  coverUrl?: string | null;
}

export interface DailyQuizResultDto {
  optionId: string;
  answerCount: number;
  percentage: number;
  isCorrect: boolean;
}

export interface DailyQuizDto {
  quizId: string;
  date: string;
  questionType: string;
  questionText: string;
  options: DailyQuizOptionDto[];
  userSelectedOptionId?: string | null;
  userWasCorrect?: boolean | null;
  results?: DailyQuizResultDto[] | null;
}

// Leaderboard DTOs
export interface LeaderboardEntryDto {
  rank: number;
  userId: string;
  username?: string | null;
  displayName: string;
  xpTotal: number;
  level: number;
  levelName: string;
  isCurrentUser: boolean;
}

export interface PagedCollectionResult {
  items: CollectionItemDto[];
  total: number;
  hasMore: boolean;
}

export interface CollectionStatsDto {
  totalGames: number;
  gamesBacklog: number;
  gamesPlaying: number;
  gamesCompleted: number;
}

// API Response types
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiError {
  message: string;
  code?: string;
  details?: Record<string, string[]>;
}

