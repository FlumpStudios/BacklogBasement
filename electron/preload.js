const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  platform: process.platform,
  showGameMenu: (gameId, currentStatus) =>
    ipcRenderer.invoke('show-game-menu', gameId, currentStatus),
  setBadgeCount: (count) =>
    ipcRenderer.send('set-badge-count', count),
  showNotification: (title, body) =>
    ipcRenderer.send('show-notification', { title, body }),
  setTitleBarOverlay: (options) =>
    ipcRenderer.send('set-title-bar-overlay', options),
});
