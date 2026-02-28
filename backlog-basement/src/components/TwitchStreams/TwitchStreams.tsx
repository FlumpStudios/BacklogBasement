import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { twitchApi } from '../../api/twitch';
import './TwitchStreams.css';

interface TwitchStreamsProps {
  igdbId: number;
}

function formatViewers(count: number): string {
  if (count >= 1000) return `${(count / 1000).toFixed(1)}K`;
  return String(count);
}

export function TwitchStreams({ igdbId }: TwitchStreamsProps) {
  const [activeChannel, setActiveChannel] = useState<string | null>(null);

  const { data: streams, isLoading } = useQuery({
    queryKey: ['twitch-streams', igdbId],
    queryFn: () => twitchApi.getStreams(igdbId),
    staleTime: 60_000, // refresh every minute
  });

  if (isLoading || !streams || streams.length === 0) return null;

  return (
    <div className="twitch-streams">
      <h2 className="twitch-streams-title">
        <span className="twitch-live-dot" />
        Live on Twitch
      </h2>

      {activeChannel && (
        <div className="twitch-embed-wrapper">
          <iframe
            src={`https://player.twitch.tv/?channel=${activeChannel}&parent=backlogbasement.com&autoplay=true`}
            allowFullScreen
            className="twitch-embed"
            title={`${activeChannel} on Twitch`}
          />
          <button
            className="twitch-embed-close"
            onClick={() => setActiveChannel(null)}
          >
            Close player
          </button>
        </div>
      )}

      <div className="twitch-stream-grid">
        {streams.map((stream) => (
          <button
            key={stream.login}
            className={`twitch-stream-card ${activeChannel === stream.login ? 'active' : ''}`}
            onClick={() => setActiveChannel(activeChannel === stream.login ? null : stream.login)}
          >
            <div className="twitch-thumbnail-wrapper">
              <img
                src={stream.thumbnailUrl}
                alt={`${stream.userName} stream thumbnail`}
                className="twitch-thumbnail"
              />
              <span className="twitch-viewer-count">
                {formatViewers(stream.viewerCount)} viewers
              </span>
            </div>
            <div className="twitch-stream-info">
              <span className="twitch-stream-user">{stream.userName}</span>
              <span className="twitch-stream-title">{stream.title}</span>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
