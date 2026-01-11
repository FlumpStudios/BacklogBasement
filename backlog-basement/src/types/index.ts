// Game DTOs - matches backend GameDto/GameSummaryDto
export interface GameDto {
  id: string;
  igdbId: number;
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
}

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

