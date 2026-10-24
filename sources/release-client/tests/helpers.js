import { vi } from 'vitest';

/**
 * Replaces fetch by a mock that answers with the given handler: (url, options) => Response.
 */
export function mockFetch (handler) {
  const fetchMock = vi.fn(async (url, options = {}) => handler(String(url), options));
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

export function json (body, status = 200) {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

export function problem (status, title, detail) {
  return new Response(JSON.stringify({ title, status, detail }), { status, headers: { 'Content-Type': 'application/problem+json' } });
}

export function authorizationOf (options) {
  return options.headers?.Authorization ?? null;
}
