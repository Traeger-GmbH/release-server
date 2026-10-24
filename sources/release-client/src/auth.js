import { computed, reactive } from 'vue';

// The credentials are kept in the session storage of the browser tab: they survive a reload, but are
// removed when the tab is closed and are never written to the persistent local storage.
const STORAGE_KEY = 'release-client.credentials';

// The previous client (Nuxt) persisted the whole store including the password in the local storage
// under this key. It is removed on start, so no password remains in the browser.
const LEGACY_STORAGE_KEY = 'vuex';

function readStoredCredentials () {
  try {
    const value = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
    if (value && typeof value.username === 'string' && typeof value.password === 'string') {
      return value;
    }
  } catch {
    // Storage not available or invalid content: not logged in
  }
  return null;
}

function removeLegacyCredentials () {
  try {
    localStorage.removeItem(LEGACY_STORAGE_KEY);
  } catch {
    // Storage not available
  }
}

const state = reactive({
  credentials: readStoredCredentials()
});

removeLegacyCredentials();

export const isLoggedIn = computed(() => state.credentials !== null);

export const username = computed(() => state.credentials?.username ?? null);

export function getCredentials () {
  return state.credentials;
}

export function setCredentials (credentials) {
  state.credentials = { username: credentials.username, password: credentials.password };
  try {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(state.credentials));
  } catch {
    // Without storage the login only lasts until the next reload
  }
}

export function clearCredentials () {
  state.credentials = null;
  try {
    sessionStorage.removeItem(STORAGE_KEY);
  } catch {
    // Storage not available
  }
}
