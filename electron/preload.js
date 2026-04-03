const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  showGameMenu: (gameId, currentStatus) =>
    ipcRenderer.invoke('show-game-menu', gameId, currentStatus),
});
