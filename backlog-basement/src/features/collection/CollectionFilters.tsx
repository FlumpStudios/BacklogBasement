import './CollectionFilters.css';

export type SortOption =
  | 'name-asc'
  | 'name-desc'
  | 'release-desc'
  | 'release-asc'
  | 'added-desc'
  | 'added-asc'
  | 'playtime-desc'
  | 'playtime-asc';

export type PlayStatusFilter = 'all' | 'played' | 'unplayed';
export type SourceFilter = 'all' | 'steam' | 'manual';

interface CollectionFiltersProps {
  searchQuery: string;
  onSearchChange: (query: string) => void;
  sortBy: SortOption;
  onSortChange: (sort: SortOption) => void;
  playStatus: PlayStatusFilter;
  onPlayStatusChange: (status: PlayStatusFilter) => void;
  sourceFilter: SourceFilter;
  onSourceFilterChange: (source: SourceFilter) => void;
  resultCount: number;
  totalCount: number;
}

const sortOptions: { value: SortOption; label: string }[] = [
  { value: 'name-asc', label: 'Name (A-Z)' },
  { value: 'name-desc', label: 'Name (Z-A)' },
  { value: 'release-desc', label: 'Release Date (Newest)' },
  { value: 'release-asc', label: 'Release Date (Oldest)' },
  { value: 'added-desc', label: 'Date Added (Newest)' },
  { value: 'added-asc', label: 'Date Added (Oldest)' },
  { value: 'playtime-desc', label: 'Most Played' },
  { value: 'playtime-asc', label: 'Least Played' },
];

export function CollectionFilters({
  searchQuery,
  onSearchChange,
  sortBy,
  onSortChange,
  playStatus,
  onPlayStatusChange,
  sourceFilter,
  onSourceFilterChange,
  resultCount,
  totalCount,
}: CollectionFiltersProps) {
  const isFiltered = searchQuery || playStatus !== 'all' || sourceFilter !== 'all';

  return (
    <div className="collection-filters">
      <div className="filters-row">
        <div className="search-wrapper">
          <input
            type="text"
            placeholder="Search games..."
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            className="search-input"
          />
          {searchQuery && (
            <button
              className="search-clear"
              onClick={() => onSearchChange('')}
              aria-label="Clear search"
            >
              x
            </button>
          )}
        </div>

        <div className="filter-controls">
          <div className="filter-group">
            <label htmlFor="play-status">Status:</label>
            <select
              id="play-status"
              value={playStatus}
              onChange={(e) => onPlayStatusChange(e.target.value as PlayStatusFilter)}
              className="filter-select"
            >
              <option value="all">All Games</option>
              <option value="played">Played</option>
              <option value="unplayed">Unplayed</option>
            </select>
          </div>

          <div className="filter-group">
            <label htmlFor="source-filter">Source:</label>
            <select
              id="source-filter"
              value={sourceFilter}
              onChange={(e) => onSourceFilterChange(e.target.value as SourceFilter)}
              className="filter-select"
            >
              <option value="all">All Sources</option>
              <option value="steam">Steam</option>
              <option value="manual">Added Manually</option>
            </select>
          </div>

          <div className="filter-group">
            <label htmlFor="sort-by">Sort:</label>
            <select
              id="sort-by"
              value={sortBy}
              onChange={(e) => onSortChange(e.target.value as SortOption)}
              className="filter-select"
            >
              {sortOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {isFiltered && (
        <div className="filter-results">
          Showing {resultCount} of {totalCount} games
        </div>
      )}
    </div>
  );
}
