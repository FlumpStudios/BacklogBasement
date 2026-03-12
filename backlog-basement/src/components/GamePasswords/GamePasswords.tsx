import { useState } from 'react';
import { useGamePasswords, useAddGamePassword, useDeleteGamePassword, usePublicGamePasswords } from '../../hooks/useGamePasswords';
import { useToast } from '../Toast';
import './GamePasswords.css';

type Tab = 'mine' | 'community';

interface Props {
  gameId: string;
  isInCollection: boolean;
}

export function GamePasswords({ gameId, isInCollection }: Props) {
  const { showToast } = useToast();
  const { data: myPasswords } = useGamePasswords(gameId);
  const { data: communityPasswords } = usePublicGamePasswords(gameId);
  const addPassword = useAddGamePassword(gameId);
  const deletePassword = useDeleteGamePassword(gameId);

  const [expanded, setExpanded] = useState(false);
  const [activeTab, setActiveTab] = useState<Tab>(isInCollection ? 'mine' : 'community');
  const [showForm, setShowForm] = useState(false);
  const [passwordText, setPasswordText] = useState('');
  const [label, setLabel] = useState('');
  const [notes, setNotes] = useState('');
  const [isPublic, setIsPublic] = useState(true);
  const [copiedId, setCopiedId] = useState<string | null>(null);

  const handleCopy = (text: string, id: string) => {
    navigator.clipboard.writeText(text);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 1500);
  };

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!passwordText.trim()) return;
    try {
      await addPassword.mutateAsync({
        password: passwordText,
        label: label || undefined,
        notes: notes || undefined,
        isPublic,
      });
      setPasswordText('');
      setLabel('');
      setNotes('');
      setIsPublic(false);
      setShowForm(false);
      showToast('Password saved', 'success');
    } catch (err: any) {
      showToast(err?.response?.data?.message ?? 'Failed to save password', 'error');
    }
  };

  const handleDelete = async (passwordId: string) => {
    try {
      await deletePassword.mutateAsync(passwordId);
      showToast('Password deleted', 'success');
    } catch {
      showToast('Failed to delete password', 'error');
    }
  };

  const PAGE_SIZE = 10;
  const [myShowAll, setMyShowAll] = useState(false);
  const [communityShowAll, setCommunityShowAll] = useState(false);

  const myCount = myPasswords?.length ?? 0;
  const communityCount = communityPasswords?.length ?? 0;
  const visibleMyPasswords = myShowAll ? myPasswords : myPasswords?.slice(0, PAGE_SIZE);
  const visibleCommunityPasswords = communityShowAll ? communityPasswords : communityPasswords?.slice(0, PAGE_SIZE);

  return (
    <div className="game-passwords">
      <button
        className="game-passwords-toggle"
        onClick={() => setExpanded(!expanded)}
        aria-expanded={expanded}
      >
        <span className="game-passwords-toggle-label">🔑 Passwords & Cheats</span>
        <span className="game-passwords-toggle-chevron">{expanded ? '▲' : '▼'}</span>
      </button>

      {expanded && (
        <div className="game-passwords-body">
          <div className="game-passwords-tabs">
            {isInCollection && (
              <button
                className={`game-passwords-tab ${activeTab === 'mine' ? 'active' : ''}`}
                onClick={() => { setActiveTab('mine'); setShowForm(false); }}
              >
                My Passwords{myCount > 0 ? ` (${myCount})` : ''}
              </button>
            )}
            <button
              className={`game-passwords-tab ${activeTab === 'community' ? 'active' : ''}`}
              onClick={() => setActiveTab('community')}
            >
              Community{communityCount > 0 ? ` (${communityCount})` : ''}
            </button>
          </div>

          <div className="game-passwords-tab-content">
            {activeTab === 'mine' && isInCollection && (
              <>
                {myPasswords && myPasswords.length > 0 ? (
                  <ul className="game-passwords-list">
                    {visibleMyPasswords!.map((p) => (
                      <li key={p.id} className="game-password-item">
                        <div className="game-password-main">
                          <code className="game-password-text">{p.password}</code>
                          {p.isPublic && <span className="game-password-badge">Public</span>}
                          <button className="btn btn-ghost btn-sm" onClick={() => handleCopy(p.password, p.id)}>
                            {copiedId === p.id ? '✓ Copied' : 'Copy'}
                          </button>
                          <button
                            className="btn btn-ghost btn-sm game-password-delete"
                            onClick={() => handleDelete(p.id)}
                            disabled={deletePassword.isPending}
                            aria-label="Delete password"
                          >
                            ✕
                          </button>
                        </div>
                        {p.label && <div className="game-password-label">{p.label}</div>}
                        {p.notes && <div className="game-password-notes">{p.notes}</div>}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="game-passwords-empty">No codes saved yet.</p>
                )}
                {myCount > PAGE_SIZE && (
                  <button className="btn btn-ghost btn-sm" onClick={() => setMyShowAll(!myShowAll)}>
                    {myShowAll ? 'Show less' : `Show ${myCount - PAGE_SIZE} more`}
                  </button>
                )}

                {showForm ? (
                  <form className="game-password-form" onSubmit={handleAdd}>
                    <input
                      className="game-password-input"
                      type="text"
                      placeholder="Password or cheat code *"
                      value={passwordText}
                      onChange={(e) => setPasswordText(e.target.value)}
                      required
                      autoFocus
                    />
                    <input
                      className="game-password-input"
                      type="text"
                      placeholder="Label (e.g. After stage 4)"
                      value={label}
                      onChange={(e) => setLabel(e.target.value)}
                    />
                    <input
                      className="game-password-input"
                      type="text"
                      placeholder="Notes (optional)"
                      value={notes}
                      onChange={(e) => setNotes(e.target.value)}
                    />
                    <label className="game-password-public-toggle">
                      <input
                        type="checkbox"
                        checked={isPublic}
                        onChange={(e) => setIsPublic(e.target.checked)}
                      />
                      <span>Share publicly so other players can use it</span>
                    </label>
                    <div className="game-password-form-actions">
                      <button type="submit" className="btn btn-primary btn-sm" disabled={addPassword.isPending}>
                        Save
                      </button>
                      <button type="button" className="btn btn-ghost btn-sm" onClick={() => setShowForm(false)}>
                        Cancel
                      </button>
                    </div>
                  </form>
                ) : (
                  <button className="btn btn-secondary btn-sm" onClick={() => setShowForm(true)}>
                    + Add Password
                  </button>
                )}
              </>
            )}

            {activeTab === 'community' && (
              <>
                {communityPasswords && communityPasswords.length > 0 ? (
                  <ul className="game-passwords-list">
                    {visibleCommunityPasswords!.map((p) => (
                      <li key={p.id} className="game-password-item">
                        <div className="game-password-main">
                          <code className="game-password-text">{p.password}</code>
                          <button className="btn btn-ghost btn-sm" onClick={() => handleCopy(p.password, p.id)}>
                            {copiedId === p.id ? '✓ Copied' : 'Copy'}
                          </button>
                        </div>
                        {p.label && <div className="game-password-label">{p.label}</div>}
                        {p.notes && <div className="game-password-notes">{p.notes}</div>}
                        {p.submittedBy && <div className="game-password-submitter">Shared by {p.submittedBy}</div>}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="game-passwords-empty">No passwords shared yet.</p>
                )}
                {communityCount > PAGE_SIZE && (
                  <button className="btn btn-ghost btn-sm" onClick={() => setCommunityShowAll(!communityShowAll)}>
                    {communityShowAll ? 'Show less' : `Show ${communityCount - PAGE_SIZE} more`}
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
