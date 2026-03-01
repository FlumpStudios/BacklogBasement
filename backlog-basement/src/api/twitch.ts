import { api } from './client';
import { TwitchLiveDto, TwitchImportResultDto } from '../types';

export interface TwitchStreamDto {
  login: string;
  userName: string;
  title: string;
  viewerCount: number;
  thumbnailUrl: string;
}

export const twitchApi = {
  getStreams: (igdbId: number): Promise<TwitchStreamDto[]> =>
    api.get<TwitchStreamDto[]>(`/twitch/streams/${igdbId}`),

  getLiveStatus: (twitchUserId: string): Promise<TwitchLiveDto> =>
    api.get<TwitchLiveDto>(`/twitch/live/${twitchUserId}`),

  importStreams: (): Promise<TwitchImportResultDto> =>
    api.post<TwitchImportResultDto>('/twitch/import', {}),

  syncNow: (): Promise<TwitchLiveDto> =>
    api.post<TwitchLiveDto>('/twitch/sync-now', {}),
};
