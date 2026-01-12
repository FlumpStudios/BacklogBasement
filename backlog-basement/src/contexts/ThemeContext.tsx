import { createContext, useContext, useState, useEffect, ReactNode } from 'react';

export type Theme = 'light' | 'dark';

interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
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

export function ThemeProvider({ children }: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(() => {
    const savedTheme = localStorage.getItem('theme') as Theme;
    console.log('[ThemeContext] Initial theme:', savedTheme || 'dark (default)');
    return savedTheme || 'dark'; 
  });

  useEffect(() => {
    console.log('[ThemeContext] Theme changed to:', theme);
    const body = document.body;
    
    // Apply theme styles directly to body
    if (theme === 'light') {
      body.style.backgroundColor = '#f8f9fa';
      body.style.color = '#212529';
      
      // Set CSS variables on body
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
      
      // Reset CSS variables on body to dark theme
      body.style.setProperty('--color-background', '#0f0f14');
      body.style.setProperty('--color-surface', '#1a1a24');
      body.style.setProperty('--color-surface-elevated', '#252532');
      body.style.setProperty('--color-border', '#2a2a3a');
      body.style.setProperty('--color-text', '#f5f5f7');
      body.style.setProperty('--color-text-secondary', '#a1a1aa');
      body.style.setProperty('--color-text-muted', '#71717a');
    }
    
    localStorage.setItem('theme', theme);
    console.log('[ThemeContext] Theme applied:', theme);
    
  }, [theme]);

  const toggleTheme = () => {
    setTheme(prev => {
      const newTheme = prev === 'dark' ? 'light' : 'dark';
      console.log('[ThemeContext] Toggling theme from', prev, 'to', newTheme);
      return newTheme;
    });
  };

  const value = {
    theme,
    toggleTheme,
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}