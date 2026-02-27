import { createContext, useContext, useEffect, useRef, ReactNode } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { authApi } from '../api';
import { useToast } from '../components';
import { UserDto } from '../types';

interface AuthContextType {
  user: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: () => void;
  loginWithSteam: () => void;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AUTH_QUERY_KEY = ['auth', 'user'];

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const shownLoginXpToast = useRef(false);

  const { data: authResult, isLoading } = useQuery({
    queryKey: AUTH_QUERY_KEY,
    queryFn: authApi.getCurrentUser,
    retry: false,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const user = authResult?.data ?? null;

  useEffect(() => {
    if (authResult?.xpAwarded && authResult.xpAwarded > 0 && !shownLoginXpToast.current) {
      shownLoginXpToast.current = true;
      showToast(`+${authResult.xpAwarded} XP — Daily login!`, 'success');
    }
  }, [authResult?.xpAwarded]);

  const login = () => {
    window.location.href = authApi.getLoginUrl();
  };

  const loginWithSteam = () => {
    window.location.href = authApi.getSteamLoginUrl();
  };

  const logout = async () => {
    await authApi.logout();
    queryClient.setQueryData(AUTH_QUERY_KEY, null);
    queryClient.clear();
  };

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated: !!user,
    login,
    loginWithSteam,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
