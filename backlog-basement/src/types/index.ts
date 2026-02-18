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

// User DTOs
export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  steamId?: string | null;
  hasSteamLinked?: boolean;
  username?: string | null;
}

// Profile DTOs
export interface ProfileDto {
  userId: string;
  username: string;
  displayName: string;
  memberSince: string;
  stats: ProfileStatsDto;
  currentlyPlaying: CollectionItemDto[];
  backlog: CollectionItemDto[];
  collection: CollectionItemDto[];
  friends: FriendDto[];
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

// Notification DTOs
export interface NotificationDto {
  id: string;
  type: string;
  message: string;
  relatedUserId?: string;
  relatedUsername?: string;
  relatedGameId?: string;
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

