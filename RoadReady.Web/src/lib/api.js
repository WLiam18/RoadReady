import axios from 'axios';
import { API_BASE_URL } from './env';
import { getAccessToken, setAccessToken, clearAccessToken } from './tokenStore';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Backend returns image paths like "/uploads/vehicle_images/abc.jpg" without
// scheme/host. Turn them into something the browser can <img src=...>.
export function resolveAssetUrl(path) {
  if (!path) return '';
  if (path.startsWith('http')) return path;
  return `${API_BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`;
}

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let refreshing = null;

async function refreshAccessToken(onExpired) {
  const refreshToken = localStorage.getItem('rr_refresh_token');
  if (!refreshToken) {
    if (onExpired) onExpired();
    return null;
  }
  if (refreshing) return refreshing;
  refreshing = (async () => {
    try {
      const res = await axios.post(
        `${API_BASE_URL}/api/v1/auth/refresh`,
        { refreshToken }
      );
      if (res.data?.success && res.data?.data) {
        setAccessToken(res.data.data.accessToken);
        localStorage.setItem('rr_refresh_token', res.data.data.refreshToken);
        return res.data.data.accessToken;
      }
      clearAccessToken();
      localStorage.removeItem('rr_refresh_token');
      if (onExpired) onExpired();
      return null;
    } catch {
      clearAccessToken();
      localStorage.removeItem('rr_refresh_token');
      if (onExpired) onExpired();
      return null;
    } finally {
      refreshing = null;
    }
  })();
  return refreshing;
}

const ACCESS_TOKEN_KEY = 'rr_access_token';
function notifyExpiredAccess() {
  // soft signal — components watching auth state will re-render with null user
  window.dispatchEvent(new CustomEvent('rr:access-expired'));
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const status = error.response?.status;
    const isAuthEndpoint =
      original?.url?.includes('/auth/login') ||
      original?.url?.includes('/auth/register') ||
      original?.url?.includes('/auth/refresh') ||
      original?.url?.includes('/auth/forgot-password') ||
      original?.url?.includes('/auth/reset-password');

    if (status === 401 && !original._retry && !isAuthEndpoint) {
      original._retry = true;
      const token = await refreshAccessToken(notifyExpiredAccess);
      if (token && original.headers) {
        original.headers.Authorization = `Bearer ${token}`;
        return api(original);
      }
    }
    return Promise.reject(error);
  }
);

export function handleApiError(error) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (data?.message) return data.message;
    if (data?.error?.description) return data.error.description;
    if (data?.title) return data.title;
    // ASP.NET Core [ApiController] 400 ValidationProblemDetails shape:
    // { type, title, status, errors: { Field: ["msg1","msg2"] }, traceId }
    if (data?.errors && typeof data.errors === 'object') {
      const parts = [];
      for (const [field, msgs] of Object.entries(data.errors)) {
        if (Array.isArray(msgs) && msgs.length) {
          parts.push(`${field}: ${msgs.join(', ')}`);
        }
      }
      if (parts.length) return parts.join(' • ');
    }
    if (error.response?.status === 0 || error.code === 'ERR_NETWORK')
      return 'Network error. Could not reach the server. Is the gateway on port 5000 running?';
    if (error.response?.status === 401) return 'Your session expired. Please log in again.';
    if (error.response?.status === 403) return 'You do not have permission for that action.';
    if (error.response?.status === 404) return 'Requested resource was not found.';
    if (error.response?.status >= 500) return 'Server error. Please try again in a moment.';
    return error.message || 'Request failed.';
  }
  return error?.message || 'Something went wrong. Please try again.';
}
