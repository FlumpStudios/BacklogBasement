import { useState, useEffect } from 'react';
import './SearchInput.css';

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isLoading?: boolean;
  autoFocus?: boolean;
}

export function SearchInput({
  value,
  onChange,
  placeholder = 'Search games...',
  isLoading = false,
  autoFocus = false,
}: SearchInputProps) {
  const [localValue, setLocalValue] = useState(value);

  useEffect(() => {
    setLocalValue(value);
  }, [value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setLocalValue(newValue);
    onChange(newValue);
  };

  const handleClear = () => {
    setLocalValue('');
    onChange('');
  };

  return (
    <div className="search-input-container">
      <input
        type="text"
        value={localValue}
        onChange={handleChange}
        placeholder={placeholder}
        className="search-input"
        autoFocus={autoFocus}
      />
      {isLoading && <span className="search-input-spinner" />}
      {localValue && !isLoading && (
        <button
          type="button"
          onClick={handleClear}
          className="search-input-clear"
          aria-label="Clear search"
        >
          ✕
        </button>
      )}
    </div>
  );
}
