import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Modal } from '../Modal/Modal';
import { useSteamImport } from '../../hooks';
import { SteamImportResult } from '../../types';
import './SteamImportPrompt.css';

interface Props {
  onClose: () => void;
}

export function SteamImportPrompt({ onClose }: Props) {
  const navigate = useNavigate();
  const importMutation = useSteamImport();
  const [includePlaytime, setIncludePlaytime] = useState(true);
  const [importResult, setImportResult] = useState<SteamImportResult | null>(null);

  const handleImport = async () => {
    setImportResult(null);
    const { data } = await importMutation.mutateAsync({ includePlaytime });
    setImportResult(data);
  };

  const handleDone = () => {
    navigate('/collection');
    onClose();
  };

  return (
    <Modal
      isOpen={true}
      onClose={onClose}
      title="Import Steam Library"
    >
      {!importResult ? (
        <div className="import-options">
          <p className="import-prompt-desc">
            Your Steam account is linked. Import your game library to populate your collection.
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
              {importMutation.isPending ? 'Importing...' : 'Import Library'}
            </button>
            <button className="btn btn-secondary" onClick={onClose}>
              Maybe Later
            </button>
          </div>
        </div>
      ) : (
        <div className="import-results">
          <div className="import-summary">
            <h4>Import Complete</h4>
            {importResult.totalGames === 0 && (
              <p className="import-warning">
                Could not find any Steam games to sync. Please ensure your Steam "Game details" are set to{' '}
                <strong>Public</strong> in your{' '}
                <a href="https://steamcommunity.com/my/edit/settings" target="_blank" rel="noopener noreferrer">
                  Steam privacy settings
                </a>.
              </p>
            )}
            <div className="import-stats">
              <div className="stat">
                <span className="stat-value">{importResult.totalGames}</span>
                <span className="stat-label">Total Games</span>
              </div>
              <div className="stat stat-success">
                <span className="stat-value">{importResult.importedCount}</span>
                <span className="stat-label">Imported</span>
              </div>
              {importResult.updatedCount > 0 && (
                <div className="stat stat-updated">
                  <span className="stat-value">{importResult.updatedCount}</span>
                  <span className="stat-label">Playtime Updated</span>
                </div>
              )}
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

          <div className="modal-actions">
            <button className="btn btn-primary" onClick={handleDone}>
              Go to Collection
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
