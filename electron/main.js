const { app, BrowserWindow, shell, Menu, MenuItem } = require('electron');
const path = require('path');
const fs = require('fs');

const APP_URL = 'https://backlogbasement.com';
const STATE_FILE = path.join(app.getPath('userData'), 'window-state.json');

function loadWindowState() {
  try {
    return JSON.parse(fs.readFileSync(STATE_FILE, 'utf8'));
  } catch {
    return { width: 1280, height: 800 };
  }
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
    icon: path.join(__dirname, 'assets', process.platform === 'win32' ? 'icon.ico' : 'icon.png'),
    title: 'Backlog Basement',
    webPreferences: {
      contextIsolation: true,
    },
  });

  if (state.maximized) win.maximize();

  win.loadURL(APP_URL);

  // Persist window size/position on resize and move
  win.on('resize', () => saveWindowState(win));
  win.on('move', () => saveWindowState(win));
  win.on('close', () => {
    fs.writeFileSync(STATE_FILE, JSON.stringify({ ...win.getBounds(), maximized: win.isMaximized() }));
  });

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

app.userAgentFallback = app.userAgentFallback.replace('Electron', '') + ' BacklogBasementApp';

app.whenReady().then(() => {
  buildMenu();
  createWindow();

  // Re-create window when dock icon is clicked on Mac (standard Mac behaviour)
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});
