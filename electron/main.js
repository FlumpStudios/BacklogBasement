const { app, BrowserWindow, shell, Menu, MenuItem, ipcMain, Tray, Notification } = require('electron');
const path = require('path');
const fs = require('fs');

let tray = null;
let mainWin = null;
let isQuitting = false;

const isMac = process.platform === 'darwin';
const isWin = process.platform === 'win32';

const APP_URL = 'https://backlogbasement.com';
const STATE_FILE = path.join(app.getPath('userData'), 'window-state.json');

function loadWindowState() {
  try {
    return JSON.parse(fs.readFileSync(STATE_FILE, 'utf8'));
  } catch {
    return { width: 1280, height: 800 };
  }
}

function saveLastUrl(url) {
  if (!url.startsWith(APP_URL)) return;
  try {
    const state = loadWindowState();
    fs.writeFileSync(STATE_FILE, JSON.stringify({ ...state, lastUrl: url }));
  } catch { /* ignore */ }
}

function saveWindowState(win) {
  if (win.isMaximized() || win.isMinimized()) return;
  const bounds = win.getBounds();
  fs.writeFileSync(STATE_FILE, JSON.stringify(bounds));
}

function createWindow() {
  const state = loadWindowState();

  const win = new BrowserWindow({
    ...state,
    minWidth: 900,
    minHeight: 600,
    icon: path.join(__dirname, 'assets', isWin ? 'icon.ico' : 'icon.png'),
    title: 'Backlog Basement',
    // Hide the OS title bar on Mac and Windows so the web nav bar is the only top bar.
    // Linux keeps the default title bar since there's no equivalent titleBarOverlay support.
    ...(isMac && {
      titleBarStyle: 'hiddenInset',
      trafficLightPosition: { x: 16, y: 24 },
    }),
    ...(isWin && {
      titleBarStyle: 'hidden',
      titleBarOverlay: {
        color: '#1a1a24',
        symbolColor: '#ffffff',
        height: 64,
      },
    }),
    webPreferences: {
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js'),
    },
  });

  mainWin = win;

  if (state.maximized) win.maximize();

  win.loadURL(state.lastUrl || APP_URL);

  // Persist window size/position on resize and move
  win.on('resize', () => saveWindowState(win));
  win.on('move', () => saveWindowState(win));
  win.on('close', (event) => {
    fs.writeFileSync(STATE_FILE, JSON.stringify({ ...win.getBounds(), maximized: win.isMaximized() }));
    if (!isQuitting) {
      event.preventDefault();
      win.hide();
    }
  });

  // Track last visited URL so we can restore it on next launch
  win.webContents.on('did-navigate', (_e, url) => saveLastUrl(url));
  win.webContents.on('did-navigate-in-page', (_e, url) => saveLastUrl(url));

  // Show offline page on load failure, ignoring user-initiated navigation aborts
  win.webContents.on('did-fail-load', (event, errorCode) => {
    if (errorCode === -3) return; // ERR_ABORTED — user navigated away, not a real failure
    win.loadFile(path.join(__dirname, 'offline.html'));
  });

  // Open new windows (target="_blank") in the system browser
  win.webContents.setWindowOpenHandler(({ url }) => {
    shell.openExternal(url);
    return { action: 'deny' };
  });

  // Keep navigation within the app — open external links in the system browser
  // OAuth provider domains are allowed to navigate in-app so the auth flow
  // (redirect → provider → callback) stays in the same session/cookie jar
  const AUTH_DOMAINS = [
    'https://store.steampowered.com',
    'https://steamcommunity.com',
    'https://accounts.google.com',
    'https://id.twitch.tv',
  ];

  win.webContents.on('will-navigate', (event, url) => {
    const isAppUrl = url.startsWith(APP_URL) || url.startsWith('file://');
    const isAuthDomain = AUTH_DOMAINS.some(domain => url.startsWith(domain));
    if (!isAppUrl && !isAuthDomain) {
      event.preventDefault();
      shell.openExternal(url);
    }
  });

  // Right-click context menu with cut/copy/paste
  win.webContents.on('context-menu', (event, params) => {
    const menu = new Menu();

    if (params.selectionText) {
      menu.append(new MenuItem({ role: 'copy' }));
    }
    if (params.isEditable) {
      if (params.selectionText) {
        menu.append(new MenuItem({ role: 'cut' }));
      }
      menu.append(new MenuItem({ role: 'paste' }));
    }

    if (menu.items.length > 0) {
      menu.popup();
    }
  });

  return win;
}

function createTray(win) {
  const iconPath = path.join(__dirname, 'assets', process.platform === 'win32' ? 'icon.ico' : 'icon.png');
  tray = new Tray(iconPath);

  const buildTrayMenu = (unread = 0) => Menu.buildFromTemplate([
    {
      label: unread > 0 ? `Backlog Basement (${unread} unread)` : 'Backlog Basement',
      enabled: false,
    },
    { type: 'separator' },
    {
      label: 'Show',
      click: () => { win.show(); win.focus(); },
    },
    { type: 'separator' },
    {
      label: 'Quit',
      click: () => { isQuitting = true; app.quit(); },
    },
  ]);

  tray.setToolTip('Backlog Basement');
  tray.setContextMenu(buildTrayMenu());

  tray.on('click', () => {
    if (win.isVisible()) {
      win.focus();
    } else {
      win.show();
    }
  });

  // Expose a helper so IPC handlers can update the tray menu
  tray.updateBadge = (count) => {
    tray.setToolTip(count > 0 ? `Backlog Basement — ${count} unread` : 'Backlog Basement');
    tray.setContextMenu(buildTrayMenu(count));
  };
}

// Minimal menu — keeps useful shortcuts (reload, zoom, devtools) without clutter
function buildMenu() {
  const template = [
    {
      label: 'View',
      submenu: [
        { role: 'reload' },
        { role: 'forceReload' },
        { type: 'separator' },
        { role: 'resetZoom' },
        { role: 'zoomIn' },
        { role: 'zoomOut' },
        { type: 'separator' },
        { role: 'togglefullscreen' },
      ],
    },
    {
      label: 'Window',
      submenu: [
        { role: 'minimize' },
        { role: 'zoom' },
        { role: 'close' },
      ],
    },
  ];

  // On Mac, add the standard app menu with quit/about
  if (process.platform === 'darwin') {
    template.unshift({
      label: app.name,
      submenu: [
        { role: 'about' },
        { type: 'separator' },
        { role: 'hide' },
        { role: 'hideOthers' },
        { role: 'unhide' },
        { type: 'separator' },
        { role: 'quit' },
      ],
    });
  }

  Menu.setApplicationMenu(Menu.buildFromTemplate(template));
}

ipcMain.handle('show-game-menu', (event, gameId, currentStatus) => {
  return new Promise((resolve) => {
    const items = [
      { label: 'Add to Backlog', status: 'backlog' },
      { label: 'Start Playing',  status: 'playing' },
      { label: 'Mark Completed', status: 'completed' },
    ];

    let resolved = false;
    const menu = Menu.buildFromTemplate(
      items.map(({ label, status }) => ({
        label,
        enabled: status !== currentStatus,
        click: () => {
          resolved = true;
          resolve({ gameId, status });
        },
      }))
    );

    menu.popup({
      window: BrowserWindow.fromWebContents(event.sender),
      callback: () => { if (!resolved) resolve(null); },
    });
  });
});

ipcMain.on('set-badge-count', (_event, count) => {
  if (process.platform === 'darwin') {
    app.setBadgeCount(count);
  }
  if (tray) {
    tray.updateBadge(count);
  }
});

ipcMain.on('set-title-bar-overlay', (_event, options) => {
  if (isWin && mainWin) {
    mainWin.setTitleBarOverlay(options);
  }
});

ipcMain.on('show-notification', (_event, { title, body }) => {
  if (Notification.isSupported()) {
    const iconPath = path.join(__dirname, 'assets', 'icon.png');
    new Notification({ title, body, icon: iconPath }).show();
  }
});

app.userAgentFallback = app.userAgentFallback.replace('Electron', '') + ' BacklogBasementApp';

app.whenReady().then(() => {
  buildMenu();
  const win = createWindow();
  createTray(win);

  // Re-create window when dock icon is clicked on Mac (standard Mac behaviour)
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      const newWin = createWindow();
      createTray(newWin);
    } else {
      win.show();
      win.focus();
    }
  });
});

// With a system tray, closing the window hides it rather than quitting.
// Only quit when the user explicitly chooses Quit from the tray menu.
app.on('window-all-closed', () => {
  if (isQuitting && process.platform !== 'darwin') app.quit();
});
