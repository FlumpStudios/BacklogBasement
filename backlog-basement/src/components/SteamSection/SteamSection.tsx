import { useState } from 'react';
import { useSteamStatus, useSteamLink, useSteamUnlink, useSteamImport, useSyncAllSteamPlaytime, useCollection } from '../../hooks';
import { Modal } from '../Modal/Modal';
import { useToast } from '../Toast';
import { SteamImportResult } from '../../types';
import './SteamSection.css';

export function SteamSection() {
  const { data: steamStatus, isLoading: statusLoading } = useSteamStatus();
  const { link } = useSteamLink();
  const unlinkMutation = useSteamUnlink();
  const importMutation = useSteamImport();
  const syncAllPlaytime = useSyncAllSteamPlaytime();
  const { showToast } = useToast();

  const { data: collection } = useCollection();
  const hasSteamGames = collection?.some(g => g.source === 'steam') ?? false;

  const [showImportModal, setShowImportModal] = useState(false);
  const [includePlaytime, setIncludePlaytime] = useState(true);
  const [importResult, setImportResult] = useState<SteamImportResult | null>(null);

  const handleLink = () => {
    link();
  };

  const handleUnlink = async () => {
    if (window.confirm('Are you sure you want to unlink your Steam account?')) {
      await unlinkMutation.mutateAsync();
    }
  };

  const handleImport = async () => {
    setImportResult(null);
    const result = await importMutation.mutateAsync({ includePlaytime });
    setImportResult(result);
  };

  const handleSyncAllPlaytime = async () => {
    try {
      const result = await syncAllPlaytime.mutateAsync();
      showToast(`Updated playtime for ${result.updatedCount} Steam games`, 'success');
    } catch {
      showToast('Failed to sync playtime', 'error');
    }
  };

  const handleCloseModal = () => {
    setShowImportModal(false);
    setImportResult(null);
  };

  if (statusLoading) {
    return (
      <div className="steam-section">
        <h3>Steam Integration</h3>
        <p>Loading...</p>
      </div>
    );
  }

  return (
    <div className="steam-section">
      <h3>Steam Integration</h3>

      {steamStatus?.isLinked ? (
        <div className="steam-linked">
          <div className="steam-status">
            <span className="steam-status-icon">&#x2714;</span>
            <span>Steam account linked</span>            
          </div>

          <div className="steam-actions">
            <button
              className="btn btn-primary"
              onClick={() => setShowImportModal(true)}
            >
              {hasSteamGames ? 'Update Library' : 'Import Library'}
            </button>
            {hasSteamGames && (
              <button
                className="btn btn-secondary"
                onClick={handleSyncAllPlaytime}
                disabled={syncAllPlaytime.isPending}
              >
                {syncAllPlaytime.isPending ? 'Syncing...' : 'Update Playtime for all games'}
              </button>
            )}
            <button
              className="btn btn-secondary"
              onClick={handleUnlink}
              disabled={unlinkMutation.isPending}
            >
              {unlinkMutation.isPending ? 'Unlinking...' : 'Unlink Steam'}
            </button>
          </div>
        </div>
      ) : (
        <div className="steam-not-linked">
          <p>Connect your Steam account to import your game library.</p>
          <button className="btn btn-steam" onClick={handleLink}>
            <svg className="steam-icon" viewBox="0 0 24 24" width="20" height="20">
              <path fill="currentColor" d="M12 2a10 10 0 0 0-9.96 9.04l5.35 2.21a2.83 2.83 0 0 1 1.6-.49l2.39-3.47v-.05a3.77 3.77 0 1 1 3.77 3.77h-.09l-3.41 2.43a2.84 2.84 0 0 1-5.65.36l-3.83-1.58A10 10 0 1 0 12 2zm-4.99 15.57l-1.22-.5a2.13 2.13 0 0 0 3.87.57 2.13 2.13 0 0 0-1.14-2.78l1.26.52a1.56 1.56 0 1 1-2.77 2.19zm8.63-5.56a2.51 2.51 0 1 0-2.51-2.51 2.51 2.51 0 0 0 2.51 2.51z"/>
            </svg>
            Link Steam Account
          </button>
        </div>
      )}

      <Modal
        isOpen={showImportModal}
        onClose={handleCloseModal}
        title="Import Steam Library"
      >
        {!importResult ? (
          <div className="import-options">
            <p>
              This will import all games from your Steam library into your collection.
              Games already in your collection will be skipped.
            </p>

            <label className="checkbox-label">
              <input
                type="checkbox"
                checked={includePlaytime}
                onChange={(e) => setIncludePlaytime(e.target.checked)}
              />
              <span>Import playtime data</span>
            </label>

            <p className="import-note">
              {includePlaytime
                ? 'Total playtime from Steam will be added as a play session for each game.'
                : 'Only game ownership will be imported, no playtime data.'}
            </p>

            <div className="modal-actions">
              <button
                className="btn btn-primary"
                onClick={handleImport}
                disabled={importMutation.isPending}
              >
                {importMutation.isPending ? 'Importing...' : 'Start Import'}
              </button>
              <button className="btn btn-secondary" onClick={handleCloseModal}>
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <div className="import-results">
            <div className="import-summary">
              <h4>Import Complete</h4>
              <div className="import-stats">
                <div className="stat">
                  <span className="stat-value">{importResult.totalGames}</span>
                  <span className="stat-label">Total Games</span>
                </div>
                <div className="stat stat-success">
                  <span className="stat-value">{importResult.importedCount}</span>
                  <span className="stat-label">Imported</span>
                </div>
                <div className="stat stat-skipped">
                  <span className="stat-value">{importResult.skippedCount}</span>
                  <span className="stat-label">Skipped</span>
                </div>
                {importResult.failedCount > 0 && (
                  <div className="stat stat-failed">
                    <span className="stat-value">{importResult.failedCount}</span>
                    <span className="stat-label">Failed</span>
                  </div>
                )}
              </div>
            </div>

            {importResult.importedGames.length > 0 && (
              <div className="import-list">
                <h5>Imported Games ({importResult.importedGames.length})</h5>
                <ul>
                  {importResult.importedGames.slice(0, 10).map((game) => (
                    <li key={game.steamAppId}>
                      {game.name}
                      {game.matchedToIgdb && <span className="badge badge-igdb">IGDB</span>}
                      {game.playtimeMinutes && game.playtimeMinutes > 0 && (
                        <span className="playtime">
                          {Math.round(game.playtimeMinutes / 60)}h
                        </span>
                      )}
                    </li>
                  ))}
                  {importResult.importedGames.length > 10 && (
                    <li className="more">
                      ...and {importResult.importedGames.length - 10} more
                    </li>
                  )}
                </ul>
              </div>
            )}

            {importResult.skippedGames.length > 0 && (
              <div className="import-list import-list-skipped">
                <h5>Skipped Games ({importResult.skippedGames.length})</h5>
                <ul>
                  {importResult.skippedGames.slice(0, 5).map((game) => (
                    <li key={game.steamAppId}>
                      {game.name}
                      <span className="reason">{game.reason}</span>
                    </li>
                  ))}
                  {importResult.skippedGames.length > 5 && (
                    <li className="more">
                      ...and {importResult.skippedGames.length - 5} more
                    </li>
                  )}
                </ul>
              </div>
            )}

            <div className="modal-actions">
              <button className="btn btn-primary" onClick={handleCloseModal}>
                Done
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
