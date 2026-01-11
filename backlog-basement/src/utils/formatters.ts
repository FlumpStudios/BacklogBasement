/**
 * Format minutes into a human-readable playtime string
 */
export function formatPlaytime(minutes: number): string {
  if (minutes < 60) {
    return `${minutes}m`;
  }

  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;

  if (remainingMinutes === 0) {
    return `${hours}h`;
  }

  return `${hours}h ${remainingMinutes}m`;
}

/**
 * Format a date string into a localized date
 */
export function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

/**
 * Extract the year from a date string
 */
export function getYear(dateString?: string): number | null {
  if (!dateString) return null;
  const date = new Date(dateString);
  return date.getFullYear();
}

/**
 * Get today's date in YYYY-MM-DD format for date inputs
 */
export function getTodayDateString(): string {
  return new Date().toISOString().split('T')[0];
}
