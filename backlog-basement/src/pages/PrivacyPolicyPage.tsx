import { Link } from 'react-router-dom';

export function PrivacyPolicyPage() {
  return (
    <div className="policy-page">
      <h1>Privacy Policy</h1>
      <p className="policy-updated">Last updated: January 2026</p>

      <section>
        <h2>What Data We Collect</h2>
        <p>When you sign in with Google, we receive and store:</p>
        <ul>
          <li><strong>Email address</strong> &mdash; used to identify your account</li>
          <li><strong>Display name</strong> &mdash; shown in the app interface</li>
          <li><strong>Profile picture URL</strong> &mdash; shown in the navigation bar</li>
        </ul>
        <p>If you link your Steam account, we also store:</p>
        <ul>
          <li><strong>Steam ID</strong> &mdash; used to import your game library and playtime data</li>
        </ul>
      </section>

      <section>
        <h2>How We Use Your Data</h2>
        <ul>
          <li>To authenticate you and maintain your session</li>
          <li>To display your game collection and playtime statistics</li>
          <li>To import games from your Steam library (when linked)</li>
        </ul>
        <p>We do not sell, share, or use your data for advertising purposes.</p>
      </section>

      <section>
        <h2>Cookies &amp; Local Storage</h2>
        <p>
          We use cookies and browser storage for essential functionality and user
          preferences. For a full breakdown, see our{' '}
          <Link to="/cookies">Cookie Policy</Link>.
        </p>
      </section>

      <section>
        <h2>Third-Party Services</h2>
        <ul>
          <li>
            <strong>Google OAuth</strong> &mdash; used for sign-in. Subject to{' '}
            <a href="https://policies.google.com/privacy" target="_blank" rel="noopener noreferrer">
              Google's Privacy Policy
            </a>.
          </li>
          <li>
            <strong>Steam</strong> &mdash; optional account linking to import your game library. Subject to{' '}
            <a href="https://store.steampowered.com/privacy_agreement/" target="_blank" rel="noopener noreferrer">
              Steam's Privacy Policy
            </a>.
          </li>
          <li>
            <strong>IGDB</strong> &mdash; used to retrieve game metadata (cover art, descriptions, release dates). No personal data is shared with IGDB.
          </li>
        </ul>
      </section>

      <section>
        <h2>Your Rights</h2>
        <p>You have the right to:</p>
        <ul>
          <li><strong>Access</strong> &mdash; request a copy of the data we hold about you</li>
          <li><strong>Deletion</strong> &mdash; request that we delete your account and all associated data</li>
          <li><strong>Portability</strong> &mdash; request your data in a portable format</li>
          <li><strong>Withdraw consent</strong> &mdash; manage cookie preferences at any time via the cookie banner</li>
        </ul>
      </section>

      <section>
        <h2>Contact</h2>
        <p>
          For privacy-related questions or to exercise your rights, please contact us
          at <a href="mailto:paul.marrable@flumpstudios.co.uk">paul.marrable@flumpstudios.co.uk</a>.
        </p>
      </section>
    </div>
  );
}
