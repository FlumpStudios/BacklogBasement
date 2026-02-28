import { api } from './client';

export interface RetroArchEntry {
  name: string;
  platform: string;
}

export interface RetroArchMatchResult {
  inputName: string;
  platform: string;
  game: {
    id: string;
    igdbId?: number | null;
    name: string;
    coverUrl?: string | null;
    releaseDate?: string | null;
    criticScore?: number | null;
  } | null;
}

export const retroarchApi = {
  matchGames: (entries: RetroArchEntry[]) =>
    api.post<RetroArchMatchResult[]>('/games/match-retroarch', { entries }),
};
