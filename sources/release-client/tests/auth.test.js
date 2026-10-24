import { describe, expect, it, vi } from 'vitest';

describe('auth', () => {
  it('removes the password persisted by the previous client', async () => {
    localStorage.setItem('vuex', JSON.stringify({ auth: { username: 'u', password: 'secret', isLoggedIn: true } }));
    vi.resetModules();

    const auth = await import('../src/auth.js');

    expect(localStorage.getItem('vuex')).toBeNull();
    expect(auth.isLoggedIn.value).toBe(false);
  });

  it('keeps the credentials in the session storage only', async () => {
    vi.resetModules();
    const auth = await import('../src/auth.js');

    auth.setCredentials({ username: 'u', password: 'secret' });

    expect(JSON.stringify({ ...localStorage })).not.toContain('secret');
    expect(sessionStorage.getItem('release-client.credentials')).toContain('secret');

    auth.clearCredentials();
    expect(sessionStorage.getItem('release-client.credentials')).toBeNull();
  });

  it('restores the login of the current session after a reload', async () => {
    sessionStorage.setItem('release-client.credentials', JSON.stringify({ username: 'u', password: 'p' }));
    vi.resetModules();

    const auth = await import('../src/auth.js');

    expect(auth.isLoggedIn.value).toBe(true);
    expect(auth.username.value).toBe('u');
    auth.clearCredentials();
  });
});
