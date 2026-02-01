import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useCookieConsent } from '../../contexts/CookieConsentContext';
import { Modal } from '../Modal';
import './CookieBanner.css';

export function CookieBanner() {
  const { hasConsented, consent, acceptAll, updateConsent } = useCookieConsent();
  const [showPreferences, setShowPreferences] = useState(false);
  const [preferences, setPreferences] = useState(consent?.preferences ?? true);

  if (hasConsented && !showPreferences) {
    return null;
  }

  const handleSavePreferences = () => {
    updateConsent({ preferences });
    setShowPreferences(false);
  };

  return (
    <>
      {!hasConsented && (
        <div className="cookie-banner" role="dialog" aria-label="Cookie consent">
          <div className="cookie-banner-content">
            <p className="cookie-banner-text">
              We use cookies to keep you signed in and remember your preferences.
              See our <Link to="/privacy">Privacy Policy</Link> and{' '}
              <Link to="/cookies">Cookie Policy</Link> for details.
            </p>
            <div className="cookie-banner-actions">
              <button onClick={() => setShowPreferences(true)} className="btn btn-secondary btn-sm">
                Manage Preferences
              </button>
              <button onClick={acceptAll} className="btn btn-primary btn-sm">
                Accept All
              </button>
            </div>
          </div>
        </div>
      )}

      <Modal
        isOpen={showPreferences}
        onClose={() => setShowPreferences(false)}
        title="Cookie Preferences"
      >
        <div className="cookie-preferences">
          <div className="cookie-category">
            <div className="cookie-category-header">
              <div>
                <strong>Essential Cookies</strong>
                <p className="cookie-category-desc">
                  Required for authentication and core functionality. These cannot be disabled.
                </p>
              </div>
              <span className="cookie-always-on">Always on</span>
            </div>
          </div>

          <div className="cookie-category">
            <div className="cookie-category-header">
              <div>
                <strong>Preference Cookies</strong>
                <p className="cookie-category-desc">
                  Remember your settings like theme preference (light/dark mode).
                </p>
              </div>
              <label className="cookie-toggle">
                <input
                  type="checkbox"
                  checked={preferences}
                  onChange={(e) => setPreferences(e.target.checked)}
                />
                <span className="cookie-toggle-slider" />
              </label>
            </div>
          </div>

          <div className="cookie-preferences-actions">
            <button onClick={() => setShowPreferences(false)} className="btn btn-secondary">
              Cancel
            </button>
            <button onClick={handleSavePreferences} className="btn btn-primary">
              Save Preferences
            </button>
          </div>
        </div>
      </Modal>
    </>
  );
}
