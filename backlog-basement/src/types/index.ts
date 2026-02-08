// Game DTOs - matches backend GameDto/GameSummaryDto
export interface GameDto {
  id: string;
  igdbId?: number | null;
  steamAppId?: number | null;
  name: string;
  coverUrl?: string | null;
  releaseDate?: string | null;
  summary?: string;
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

