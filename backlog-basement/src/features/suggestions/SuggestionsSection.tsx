import { useSuggestions } from '../../hooks';
import { SuggestionCard } from './SuggestionCard';
import './SuggestionsSection.css';

export function SuggestionsSection() {
  const { data: suggestions } = useSuggestions();

  if (!suggestions || suggestions.length === 0) return null;

  return (
    <section className="dashboard-section suggestions-section">
      <div className="section-header">
        <h2>Suggested for You</h2>
      </div>
      <p className="section-description">Games your friends think you should play</p>
      <div className="suggestions-scroll">
        {suggestions.map((suggestion) => (
          <SuggestionCard key={suggestion.id} suggestion={suggestion} />
        ))}
      </div>
    </section>
  );
}
