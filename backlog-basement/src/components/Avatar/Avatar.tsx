import './Avatar.css';

interface AvatarProps {
  avatarUrl?: string | null;
  displayName: string;
  userId?: string;
  size?: 'sm' | 'md' | 'lg';
}

const COLORS = [
  '#e74c3c', '#e67e22', '#f1c40f', '#2ecc71',
  '#1abc9c', '#3498db', '#9b59b6', '#e91e63',
];

function colorFromString(str: string): string {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  return COLORS[Math.abs(hash) % COLORS.length];
}

export function Avatar({ avatarUrl, displayName, userId, size = 'md' }: AvatarProps) {
  const initial = displayName ? displayName[0].toUpperCase() : '?';
  const color = colorFromString(userId ?? displayName);

  if (avatarUrl) {
    return (
      <img
        src={avatarUrl}
        alt={displayName}
        className={`avatar avatar--${size}`}
        onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
      />
    );
  }

  return (
    <div
      className={`avatar avatar--${size} avatar--initials`}
      style={{ backgroundColor: color }}
      aria-label={displayName}
    >
      {initial}
    </div>
  );
}
