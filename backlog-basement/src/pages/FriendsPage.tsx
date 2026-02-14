import { PlayerSearch, FriendsList } from '../features/friends';
import './FriendsPage.css';

export function FriendsPage() {
  return (
    <div className="friends-page">
      <h1>Friends</h1>

      <section className="friends-page-search">
        <h2>Find Players</h2>
        <PlayerSearch />
      </section>

      <section className="friends-page-list">
        <FriendsList />
      </section>
    </div>
  );
}
