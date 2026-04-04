import { useEffect, useRef } from 'react';
import { useUnreadCount } from './useNotifications';
import { useUnreadMessageCount } from './useMessages';

/**
 * Syncs unread notification + message counts with the Electron shell.
 * - Updates the taskbar/dock badge whenever either count changes.
 * - Fires a native OS notification when either count *increases* after init.
 *
 * No-ops when running in a regular browser (window.electron is absent).
 */
export function useElectronBridge() {
  const electron = (window as any).electron;

  const { data: notifData } = useUnreadCount();
  const { data: msgData } = useUnreadMessageCount(true);

  const prevNotifCount = useRef<number | null>(null);
  const prevMsgCount = useRef<number | null>(null);

  const notifCount = notifData?.count ?? 0;
  const msgCount = msgData?.count ?? 0;

  useEffect(() => {
    if (!electron) return;

    const prev = prevNotifCount.current;

    if (prev !== null && notifCount > prev) {
      const delta = notifCount - prev;
      electron.showNotification?.(
        'Backlog Basement',
        delta === 1 ? 'You have a new notification' : `You have ${delta} new notifications`,
      );
    }

    prevNotifCount.current = notifCount;
  }, [notifCount, electron]);

  useEffect(() => {
    if (!electron) return;

    const prev = prevMsgCount.current;

    if (prev !== null && msgCount > prev) {
      const delta = msgCount - prev;
      electron.showNotification?.(
        'Backlog Basement',
        delta === 1 ? 'You have a new message' : `You have ${delta} new messages`,
      );
    }

    prevMsgCount.current = msgCount;
  }, [msgCount, electron]);

  // Keep badge count in sync with combined total
  useEffect(() => {
    if (!electron) return;
    electron.setBadgeCount?.(notifCount + msgCount);
  }, [notifCount, msgCount, electron]);
}
