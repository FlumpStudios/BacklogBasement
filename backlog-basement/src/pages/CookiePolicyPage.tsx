import { Link } from 'react-router-dom';

export function CookiePolicyPage() {
  return (
    <div className="policy-page">
      <h1>Cookie Policy</h1>
      <p className="policy-updated">Last updated: January 2026</p>

      <section>
        <h2>What Are Cookies?</h2>
        <p>
          Cookies are small text files stored on your device by your browser. We also
          use browser localStorage, which serves a similar purpose but stays until
          manually cleared.
        </p>
      </section>

      <section>
        <h2>Essential Cookies</h2>
        <p>
          These are required for the site to function and cannot be disabled.
        </p>
        <table className="policy-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Purpose</th>
              <th>Type</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td><code>backlog-basement-auth</code></td>
              <td>Keeps you signed in (session authentication)</td>
              <td>HttpOnly Cookie</td>
              <td>Session</td>
            </tr>
            <tr>
              <td>OAuth correlation cookies</td>
              <td>Protect against cross-site request forgery during sign-in</td>
              <td>HttpOnly Cookie</td>
              <td>Temporary</td>
            </tr>
            <tr>
              <td><code>cookie-consent</code></td>
              <td>Stores your cookie consent preferences</td>
              <td>localStorage</td>
              <td>Persistent</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section>
        <h2>Preference Cookies</h2>
        <p>
          These remember your settings. You can enable or disable them via the cookie
          banner.
        </p>
        <table className="policy-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Purpose</th>
              <th>Type</th>
              <th>Duration</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td><code>theme</code></td>
              <td>Remembers your light/dark mode preference</td>
              <td>localStorage</td>
              <td>Persistent</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section>
        <h2>Analytics &amp; Tracking</h2>
        <p>We do not use any analytics or tracking cookies.</p>
      </section>

      <section>
        <h2>Managing Cookies</h2>
        <p>
          You can change your preferences at any time by clicking &ldquo;Manage
          Preferences&rdquo; in the cookie banner, or by clearing your browser's
          cookies and localStorage.
        </p>
        <p>
          For more information about how we handle your data, see our{' '}
          <Link to="/privacy">Privacy Policy</Link>.
        </p>
      </section>
    </div>
  );
}
