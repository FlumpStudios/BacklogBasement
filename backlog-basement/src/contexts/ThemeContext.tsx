import { createContext, useContext, useState, useEffect, ReactNode } from 'react';

export type Theme = 'light' | 'dark';
export type RetroMode = 'off' | 'c64' | 'bbc';

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
  retroMode: RetroMode;
  cycleRetro: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function useTheme() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}

interface ThemeProviderProps {
  children: ReactNode;
}

const C64_VARS: [string, string][] = [
  ['--color-background', '#1a1ab8'],
  ['--color-surface', '#2424cc'],
  ['--color-surface-elevated', '#2e2ed8'],
  ['--color-border', '#5858e8'],
  ['--color-text', '#a8a8ff'],
  ['--color-text-secondary', '#8080e0'],
  ['--color-text-muted', '#6060c0'],
  ['--color-primary', '#a8a8ff'],
  ['--color-primary-hover', '#c0c0ff'],
  ['--color-accent', '#70d4ff'],
];

const BBC_VARS: [string, string][] = [
  ['--color-background', '#000000'],
  ['--color-surface', '#111111'],
  ['--color-surface-elevated', '#1a1a00'],
  ['--color-border', '#ff0000'],
  ['--color-text', '#ffffff'],
  ['--color-text-secondary', '#ffff00'],
  ['--color-text-muted', '#888888'],
  ['--color-primary', '#ffff00'],
  ['--color-primary-hover', '#ffffff'],
  ['--color-accent', '#00ffff'],
];

const DARK_VARS: [string, string][] = [
  ['--color-background', '#0f0f14'],
  ['--color-surface', '#1a1a24'],
  ['--color-surface-elevated', '#252532'],
  ['--color-border', '#2a2a3a'],
  ['--color-text', '#f5f5f7'],
  ['--color-text-secondary', '#a1a1aa'],
  ['--color-text-muted', '#71717a'],
];

function loadSavedRetro(): RetroMode {
  const saved = localStorage.getItem('retro');
  // migrate old boolean value
  if (saved === 'true') return 'c64';
  if (saved === 'c64' || saved === 'bbc') return saved;
  return 'off';
}

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem('theme') as Theme;
    return savedTheme || 'dark';
  });

  const [retroMode, setRetroMode] = useState<RetroMode>(loadSavedRetro);

  // Apply base light/dark theme
  useEffect(() => {
    const body = document.body;
    if (theme === 'light') {
      body.style.backgroundColor = '#f8f9fa';
      body.style.color = '#212529';
      body.style.colorScheme = 'light';
      body.style.setProperty('--color-background', '#f8f9fa');
      body.style.setProperty('--color-surface', '#ffffff');
      body.style.setProperty('--color-surface-elevated', '#ffffff');
      body.style.setProperty('--color-border', '#e9ecef');
      body.style.setProperty('--color-text', '#212529');
      body.style.setProperty('--color-text-secondary', '#495057');
      body.style.setProperty('--color-text-muted', '#6c757d');
    } else {
      body.style.backgroundColor = '#0f0f14';
      body.style.color = '#f5f5f7';
      body.style.colorScheme = 'dark';
      for (const [k, v] of DARK_VARS) body.style.setProperty(k, v);
    }
    localStorage.setItem('theme', theme);
  }, [theme]);

  // Apply retro mode overrides
  useEffect(() => {
    const body = document.body;

    if (retroMode === 'off') {
      body.removeAttribute('data-retro');
      body.removeAttribute('data-retro-style');
      // Re-apply the base theme vars since retro overrides are cleared
      const vars = theme === 'light'
        ? [
            ['--color-background', '#f8f9fa'],
            ['--color-surface', '#ffffff'],
            ['--color-surface-elevated', '#ffffff'],
            ['--color-border', '#e9ecef'],
            ['--color-text', '#212529'],
            ['--color-text-secondary', '#495057'],
            ['--color-text-muted', '#6c757d'],
          ] as [string, string][]
        : DARK_VARS;
      body.style.backgroundColor = theme === 'light' ? '#f8f9fa' : '#0f0f14';
      body.style.color = theme === 'light' ? '#212529' : '#f5f5f7';
      for (const [k, v] of vars) body.style.setProperty(k, v);
    } else {
      const vars = retroMode === 'c64' ? C64_VARS : BBC_VARS;
      body.setAttribute('data-retro', 'true');
      body.setAttribute('data-retro-style', retroMode);
      body.style.backgroundColor = vars[0][1];
      body.style.color = vars[4][1];
      for (const [k, v] of vars) body.style.setProperty(k, v);
    }

    localStorage.setItem('retro', retroMode);
  }, [retroMode, theme]);

  const toggleTheme = () => {
    setTheme(prev => prev === 'dark' ? 'light' : 'dark');
  };

  const cycleRetro = () => {
    setRetroMode(prev => {
      if (prev === 'off') return 'c64';
      if (prev === 'c64') return 'bbc';
      return 'off';
    });
  };

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme, retroMode, cycleRetro }}>
      {children}
    </ThemeContext.Provider>
  );
}
