import { useTheme } from '../../contexts/ThemeContext';
import './ThemeToggle.css';

export function ThemeToggle() {
  const { theme, toggleTheme } = useTheme();
  
  console.log('[ThemeToggle] Current theme:', theme);
  
  const handleClick = () => {
    console.log('[ThemeToggle] Click detected, current theme:', theme);
    toggleTheme();
  };

  return (
    <button 
      onClick={handleClick} 
      className="theme-toggle"
      aria-label={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
      title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode (${theme})`}
    >
      {theme === 'dark' ? '🌙' : '☀️'}
    </button>
  );
}