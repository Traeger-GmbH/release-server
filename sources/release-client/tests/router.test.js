import { afterEach, describe, expect, it } from 'vitest';
import { createMemoryHistory } from 'vue-router';

import { clearCredentials, setCredentials } from '../src/auth.js';
import { createAppRouter } from '../src/router.js';

describe('router', () => {
  afterEach(() => clearCredentials());

  it('redirects to the login and back to the requested page', async () => {
    const router = createAppRouter(createMemoryHistory());

    await router.push('/products/alpha');

    expect(router.currentRoute.value.path).toBe('/login');
    expect(router.currentRoute.value.query.afterLogin).toBe('/products/alpha');
  });

  it('allows all pages after the login', async () => {
    setCredentials({ username: 'u', password: 'p' });
    const router = createAppRouter(createMemoryHistory());

    await router.push('/maintain');

    expect(router.currentRoute.value.path).toBe('/maintain');
  });

  it('leaves protected pages when the login ends', async () => {
    setCredentials({ username: 'u', password: 'p' });
    const router = createAppRouter(createMemoryHistory());
    await router.push('/upload');

    clearCredentials();
    await new Promise(resolve => setTimeout(resolve, 0));
    await router.isReady();

    expect(router.currentRoute.value.path).toBe('/login');
  });
});
