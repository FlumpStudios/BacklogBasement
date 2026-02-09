import { Routes, Route } from 'react-router-dom';
import { Layout } from './components';
import { ProtectedRoute } from './auth';
import {
  LandingPage,
  LoginPage,
  DashboardPage,
  SearchPage,
  CollectionPage,
  GameDetailPage,
  NotFoundPage,
  PrivacyPolicyPage,
  CookiePolicyPage,
  ProfilePage,
} from './pages';

function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        {/* Public routes */}
        <Route path="/" element={<LandingPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/privacy" element={<PrivacyPolicyPage />} />
        <Route path="/cookies" element={<CookiePolicyPage />} />
        <Route path="/profile/:username" element={<ProfilePage />} />

        {/* Protected routes */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/collection"
          element={
            <ProtectedRoute>
              <CollectionPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/search"
          element={
            <ProtectedRoute>
              <SearchPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/games/:id"
          element={
            <ProtectedRoute>
              <GameDetailPage />
            </ProtectedRoute>
          }
        />

        {/* 404 */}
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}

export default App;
