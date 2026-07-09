const TOKEN_KEY = 'rr_access_token';
const REFRESH_KEY = 'rr_refresh_token';

function readFromStorage() {
  try {
    return sessionStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

let accessToken = readFromStorage();
const listeners = new Set();

export function getAccessToken() {
  if (accessToken) return accessToken;
  accessToken = readFromStorage();
  return accessToken;
}

export function setAccessToken(token) {
  accessToken = token;
  try {
    if (token) sessionStorage.setItem(TOKEN_KEY, token);
    else sessionStorage.removeItem(TOKEN_KEY);
  } catch {/* ignore */}
  listeners.forEach((l) => l());
}

export function clearAccessToken() {
  accessToken = null;
  try { sessionStorage.removeItem(TOKEN_KEY); } catch {/* ignore */}
  listeners.forEach((l) => l());
}

export function subscribe(listener) {
  listeners.add(listener);
  return () => listeners.delete(listener);
}
