import { api } from './client';

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
};
