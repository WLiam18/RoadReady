import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { api, handleApiError } from '../lib/api';
import { setAccessToken, getAccessToken, clearAccessToken } from '../lib/tokenStore';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const applyTokens = useCallback(({ accessToken, refreshToken, user }) => {
    if (accessToken) setAccessToken(accessToken);
    if (refreshToken) localStorage.setItem('rr_refresh_token', refreshToken);
    if (user) localStorage.setItem('rr_user', JSON.stringify(user));
    setUser(user);
  }, []);

  const clear = useCallback(() => {
    clearAccessToken();
    localStorage.removeItem('rr_refresh_token');
    localStorage.removeItem('rr_user');
    setUser(null);
  }, []);

  // Restore session from storage on first mount
  useEffect(() => {
    const storedUser = localStorage.getItem('rr_user');
    const storedRefresh = localStorage.getItem('rr_refresh_token');
    if (storedUser && storedRefresh) {
      try {
        const parsed = JSON.parse(storedUser);
        setUser(parsed);
      } catch {/* ignore */}
    }
    setLoading(false);
  }, []);

  // Auto-logout when the API client detects an expired access + refresh pair
  useEffect(() => {
    const onExpired = () => {
      clear();
    };
    window.addEventListener('rr:access-expired', onExpired);
    return () => window.removeEventListener('rr:access-expired', onExpired);
  }, [clear]);

  // (Removed: previously subscribed to token changes with a listener that called
  // setAccessToken, which itself fires listeners — infinite loop. The in-memory
  // token reads via getAccessToken() are sufficient; cross-tab sync is not
  // required for this app since login is single-tab.)

  const login = useCallback(async (email, password) => {
    try {
      const res = await api.post('/api/v1/auth/login', { email, password });
      if (!res?.data?.success) {
        return { ok: false, message: res?.data?.message || 'Login failed.' };
      }
      applyTokens(res.data.data);
      return { ok: true };
    } catch (err) {
      return { ok: false, message: handleApiError(err) || 'Login failed.' };
    }
  }, [applyTokens]);

  const register = useCallback(async (payload) => {
    try {
      const res = await api.post('/api/v1/auth/register', payload);
      if (!res?.data?.success) {
        return { ok: false, message: res?.data?.message || 'Registration failed.' };
      }
      applyTokens(res.data.data);
      return { ok: true };
    } catch (err) {
      return { ok: false, message: handleApiError(err) || 'Registration failed.' };
    }
  }, [applyTokens]);

  const loginWithGoogle = useCallback(async (credential) => {
    try {
      const res = await api.post('/api/v1/auth/google', { credential });
      if (!res?.data?.success) {
        return { ok: false, message: res?.data?.message || 'Google login failed.' };
      }
      applyTokens(res.data.data);
      return { ok: true };
    } catch (err) {
      return { ok: false, message: handleApiError(err) || 'Google login failed.' };
    }
  }, [applyTokens]);

  const logout = useCallback(async () => {
    try {
      await api.post('/api/v1/auth/logout');
      clear();
      return { ok: true };
    } catch (err) {
      clear();
      return { ok: false, message: handleApiError(err) };
    }
  }, [clear]);

  const refreshProfile = useCallback(async () => {
    try {
      const res = await api.get('/api/v1/auth/profile');
      if (res.data?.success && res.data?.data) {
        localStorage.setItem('rr_user', JSON.stringify(res.data.data));
        setUser(res.data.data);
      }
    } catch {/* ignore */}
  }, []);

  // Backend may return role as one of:
  //   - string  "Admin" / "RentalAgent" / "Customer"
  //   - integer 1 / 2 / 0
  // We normalize to "admin" / "rentalagent" / "customer".
  const normalizeRole = (raw) => {
    const r = String(raw ?? '').toLowerCase().trim();
    const map = { 0: 'customer', 1: 'admin', 2: 'rentalagent', '1': 'admin', '2': 'rentalagent', '0': 'customer' };
    return map[r] || r;
  };

  const value = {
    user,
    loading,
    isAuthenticated: !!user,
    isAdmin: normalizeRole(user?.role) === 'admin',
    isAgent: normalizeRole(user?.role) === 'rentalagent',
    login,
    register,
    loginWithGoogle,
    logout,
    clear,
    refreshProfile,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider');
  return ctx;
}
