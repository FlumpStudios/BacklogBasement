import { useState } from 'react';
import { retroarchApi, RetroArchMatchResult, RetroArchEntry } from '../../api/retroarch';
import { collectionApi } from '../../api/collection';
import { useQueryClient } from '@tanstack/react-query';
import { Modal } from '../Modal/Modal';
import { useToast } from '../Toast';
import './RetroArchSection.css';

// File System Access API types (Chrome/Edge only)
declare global {
  interface Window {
    showOpenFilePicker?: (options?: {
      multiple?: boolean;
      types?: Array<{ description: string; accept: Record<string, string[]> }>;
    }) => Promise<FileSystemFileHandle[]>;
    showDirectoryPicker?: () => Promise<FileSystemDirectoryHandle>;
  }
}

interface FileSystemFileHandle {
  kind: 'file';
  name: string;
  getFile(): Promise<File>;
}

interface FileSystemDirectoryHandle {
  kind: 'directory';
  name: string;
  entries(): AsyncIterable<[string, FileSystemFileHandle | FileSystemDirectoryHandle]>;
}

const PLATFORM_NAMES: Record<string, string> = {
  'Nintendo - Nintendo Entertainment System': 'NES',
  'Nintendo - Super Nintendo Entertainment System': 'SNES',
  'Nintendo - Game Boy': 'Game Boy',
  'Nintendo - Game Boy Color': 'GBC',
  'Nintendo - Game Boy Advance': 'GBA',
  'Nintendo - Nintendo 64': 'N64',
  'Nintendo - GameCube': 'GameCube',
  'Nintendo - Wii': 'Wii',
  'Sega - Master System - Mark III': 'Sega Master System',
  'Sega - Mega Drive - Genesis': 'Sega Genesis',
  'Sega - Saturn': 'Sega Saturn',
  'Sega - Dreamcast': 'Sega Dreamcast',
  'Sega - Game Gear': 'Game Gear',
  'Sony - PlayStation': 'PlayStation',
  'Sony - PlayStation 2': 'PlayStation 2',
  'Sony - PlayStation Portable': 'PSP',
  'Atari - 2600': 'Atari 2600',
  'NEC - PC Engine - TurboGrafx 16': 'TurboGrafx-16',
  'SNK - Neo Geo': 'Neo Geo',
};

function cleanPlatformName(raw: string): string {
  const name = raw.replace(/\.lpl$/i, '');
  return PLATFORM_NAMES[name] ?? name;
}

function cleanGameName(label: string): string {
  return label
    .replace(/\s*\([^)]*\)/g, '')   // Remove (USA), (Europe), (Rev 1), (Disc 1), (Beta), etc.
    .replace(/\s*\[[^\]]*\]/g, '')   // Remove [!], [b], [T+Eng], etc.
    .replace(/ - /g, ': ')           // RetroArch uses " - " for subtitles, IGDB uses ": "
    .replace(/^(.*?),\s*(The|A|An)(\s*:|$)/i, '$2 $1$3')  // "Title, The: Sub" → "The Title: Sub"
    .trim();
}

async function parsePlaylistFiles(fileHandles: FileSystemFileHandle[]): Promise<RetroArchEntry[]> {
  const entries: RetroArchEntry[] = [];

  for (const handle of fileHandles) {
    try {
      const file = await handle.getFile();
      const text = await file.text();
      const playlist = JSON.parse(text);
      const platform = cleanPlatformName(handle.name);

      for (const item of playlist.items ?? []) {
        const raw: string = item.label ?? '';
        const cleaned = cleanGameName(raw);
        if (cleaned.length >= 2) entries.push({ name: cleaned, platform });
      }
    } catch {
      // Skip unreadable or malformed playlist files
    }
  }

  // Deduplicate by normalised name (after cleaning, region variants collapse to one entry)
  const seen = new Set<string>();
  return entries.filter((e) => {
    const key = e.name.toLowerCase();
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

type Step = 'idle' | 'parsing' | 'matching' | 'review' | 'importing' | 'done';

export function RetroArchSection() {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [isOpen, setIsOpen] = useState(true);
  const [step, setStep] = useState<Step>('idle');
  const [matches, setMatches] = useState<RetroArchMatchResult[]>([]);
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [importedCount, setImportedCount] = useState(0);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const isSupported = typeof window !== 'undefined' && typeof window.showOpenFilePicker === 'function';

  const runImport = async (fileHandles: FileSystemFileHandle[]) => {
    setStep('parsing');
    let entries: RetroArchEntry[];
    try {
      entries = await parsePlaylistFiles(fileHandles);
    } catch {
      setErrorMsg('Failed to read the playlist files.');
      setStep('idle');
      return;
    }

    if (entries.length === 0) {
      setErrorMsg('No games found in the selected playlist files.');
      setStep('idle');
      return;
    }

    setStep('matching');
    let results: RetroArchMatchResult[];
    try {
      const allResults: RetroArchMatchResult[] = [];
      for (let i = 0; i < entries.length; i += 200) {
        const batch = entries.slice(i, i + 200);
        const batchResults = await retroarchApi.matchGames(batch);
        allResults.push(...batchResults);
      }
      results = allResults;
    } catch {
      setErrorMsg('Failed to match games against IGDB. Please try again.');
      setStep('idle');
      return;
    }

    const matchedIds = new Set(results.filter((r) => r.game).map((r) => r.inputName));
    setMatches(results);
    setSelected(matchedIds);
    setStep('review');
  };

  const handleBrowse = async () => {
    if (!window.showOpenFilePicker) return;
    setErrorMsg(null);
    let fileHandles: FileSystemFileHandle[];
    try {
      fileHandles = await window.showOpenFilePicker({
        multiple: true,
        types: [{ description: 'RetroArch Playlists', accept: { 'application/json': ['.lpl'] } }],
      });
    } catch {
      return; // User cancelled
    }
    await runImport(fileHandles);
  };


  const handleSelectAll = () => setSelected(new Set(matches.filter((m) => m.game).map((m) => m.inputName)));
  const handleDeselectAll = () => setSelected(new Set());

  const handleToggle = (inputName: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(inputName) ? next.delete(inputName) : next.add(inputName);
      return next;
    });
  };

  const handleImport = async () => {
    const toImport = matches.filter((m) => m.game && selected.has(m.inputName));
    if (toImport.length === 0) return;

    setStep('importing');
    const gameIds = toImport.map((m) => m.game!.id);
    const result = await collectionApi.bulkAddGames(gameIds);

    await queryClient.invalidateQueries({ queryKey: ['collection'] });
    setImportedCount(result.added);
    setStep('done');
    showToast(`Added ${result.added} retro games to your collection`, 'success');
  };

  const handleClose = () => {
    setStep('idle');
    setMatches([]);
    setSelected(new Set());
    setImportedCount(0);
    setErrorMsg(null);
  };

  const matched = matches.filter((m) => m.game);
  const unmatched = matches.filter((m) => !m.game);
  const selectedCount = selected.size;

  return (
    <div className="retroarch-section">
      <button className="retroarch-header" onClick={() => setIsOpen(!isOpen)}>
        <div className="retroarch-header-left">
          <h3>RetroArch Import</h3>
          <span className="retroarch-header-badge">Beta</span>
        </div>
        <span className={`retroarch-chevron ${isOpen ? 'open' : ''}`}>&#9662;</span>
      </button>

      <div className={`retroarch-body ${isOpen ? 'open' : ''}`}>
        <div className="retroarch-body-inner">
          <div className="retroarch-body-content">
            <p className="retroarch-desc">
              Import your retro game collection by selecting one or more RetroArch <code>.lpl</code> playlist files.
              For best results, import a platform or two at a time — matching can take over a minute per 1,000 games.
            </p>
            {!isSupported ? (
              <p className="retroarch-unsupported">
                This feature requires a Chromium-based browser (Chrome or Edge). Firefox and Safari are not supported.
              </p>
            ) : (
              <div className="retroarch-actions">
                <button
                  className="btn btn-primary"
                  onClick={handleBrowse}
                  disabled={step === 'parsing' || step === 'matching'}
                >
                  {step === 'parsing' ? 'Reading playlists...' : step === 'matching' ? 'Matching games...' : 'Choose Playlist Files'}
                </button>
              </div>
            )}
            {errorMsg && <p className="retroarch-error">{errorMsg}</p>}
          </div>
        </div>
      </div>

      {/* Review Modal */}
      <Modal
        isOpen={step === 'review' || step === 'importing' || step === 'done'}
        onClose={handleClose}
        title="RetroArch Import"
      >
        {step === 'done' ? (
          <div className="retroarch-done">
            <p className="retroarch-done-msg">
              Added <strong>{importedCount}</strong> game{importedCount !== 1 ? 's' : ''} to your collection.
            </p>
            <div className="modal-actions">
              <button className="btn btn-primary" onClick={handleClose}>Done</button>
            </div>
          </div>
        ) : (
          <>
            <div className="retroarch-warning">
              Matching is based on game names and may not be 100% accurate. Review the results before importing and
              deselect any incorrect matches.
            </div>

            <div className="retroarch-stats">
              <div className="stat stat-success">
                <span className="stat-value">{matched.length}</span>
                <span className="stat-label">Matched</span>
              </div>
              <div className="stat">
                <span className="stat-value">{unmatched.length}</span>
                <span className="stat-label">Unmatched</span>
              </div>
              <div className="stat">
                <span className="stat-value">{matches.length}</span>
                <span className="stat-label">Total</span>
              </div>
            </div>

            {matched.length > 0 && (
              <div className="retroarch-matched-section">
                <div className="retroarch-list-header">
                  <h5>Matched games ({matched.length})</h5>
                  <div className="retroarch-select-controls">
                    <button className="btn-link" onClick={handleSelectAll}>Select all</button>
                    <span>·</span>
                    <button className="btn-link" onClick={handleDeselectAll}>Deselect all</button>
                  </div>
                </div>
                <ul className="retroarch-game-list">
                  {matched.map((m) => (
                    <li key={m.inputName} className="retroarch-game-item">
                      <label className="retroarch-game-label">
                        <input
                          type="checkbox"
                          checked={selected.has(m.inputName)}
                          onChange={() => handleToggle(m.inputName)}
                        />
                        {m.game?.coverUrl && (
                          <img src={m.game.coverUrl} alt="" className="retroarch-cover" />
                        )}
                        <div className="retroarch-game-info">
                          <span className="retroarch-game-name">{m.game?.name}</span>
                          {m.inputName !== m.game?.name && (
                            <span className="retroarch-input-name">from: {m.inputName}</span>
                          )}
                        </div>
                        <span className="retroarch-platform">{m.platform}</span>
                      </label>
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {unmatched.length > 0 && (
              <div className="retroarch-unmatched-section">
                <h5>No match found ({unmatched.length}) — will be skipped</h5>
                <ul className="retroarch-unmatched-list">
                  {unmatched.slice(0, 8).map((m) => (
                    <li key={m.inputName}>{m.inputName} <span className="retroarch-platform">{m.platform}</span></li>
                  ))}
                  {unmatched.length > 8 && <li className="retroarch-more">...and {unmatched.length - 8} more</li>}
                </ul>
              </div>
            )}

            <div className="modal-actions">
              <button
                className="btn btn-primary"
                onClick={handleImport}
                disabled={selectedCount === 0 || step === 'importing'}
              >
                {step === 'importing' ? 'Adding...' : `Add ${selectedCount} game${selectedCount !== 1 ? 's' : ''} to collection`}
              </button>
              <button className="btn btn-secondary" onClick={handleClose} disabled={step === 'importing'}>
                Cancel
              </button>
            </div>
          </>
        )}
      </Modal>
    </div>
  );
}
