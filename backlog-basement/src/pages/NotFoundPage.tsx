import { Link } from 'react-router-dom';
import './NotFoundPage.css';

export function NotFoundPage() {
  return (
    <div className="not-found-page">
      <span className="not-found-icon">🕹️</span>
      <h1>404 - Page Not Found</h1>
      <p>Looks like this page got lost in the backlog.</p>
      <Link to="/" className="btn btn-primary">
        Back to Home
      </Link>
    </div>
  );
}
