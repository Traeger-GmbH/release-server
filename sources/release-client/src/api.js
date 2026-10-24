// Client for the release server API. The UI is served from the same origin as the API: nginx (and the
// Vite dev server) forward /artifacts to the release server, so no CORS is involved.
import { clearCredentials, getCredentials, setCredentials } from './auth.js';

const BASE_URL = '/artifacts';

// Number of releases requested per page of /info (the server default is 50).
const PAGE_SIZE = 1000;

export class ApiError extends Error {
  constructor (status, title, detail) {
    super(detail ? `${title}: ${detail}` : title);
    this.name = 'ApiError';
    this.status = status;
    this.title = title;
    this.detail = detail;
  }
}

export function basicAuthorization ({ username, password }) {
  // btoa only supports Latin-1, so the UTF-8 bytes are encoded explicitly
  const bytes = new TextEncoder().encode(`${username}:${password}`);
  let binary = '';
  bytes.forEach((byte) => { binary += String.fromCharCode(byte); });
  return `Basic ${btoa(binary)}`;
}

async function createError (response) {
  let title = response.statusText || `Request failed (${response.status})`;
  let detail = null;
  try {
    const body = await response.json();
    title = body.title || title;
    detail = body.detail || body.error || null;
  } catch {
    // The response has no JSON body
  }
  return new ApiError(response.status, title, detail);
}

async function request (path, { method = 'GET', body, authenticated = false, credentials = null } = {}) {
  const headers = {};
  const usedCredentials = credentials ?? (authenticated ? getCredentials() : null);
  if (usedCredentials) {
    headers.Authorization = basicAuthorization(usedCredentials);
  }

  let response;
  try {
    response = await fetch(BASE_URL + path, { method, body, headers });
  } catch (error) {
    throw new ApiError(0, 'The release server is not reachable', error.message);
  }

  if (!response.ok) {
    // Stored credentials are no longer valid (e.g. the password was changed): log out
    if (response.status === 401 && authenticated && credentials === null) {
      clearCredentials();
    }
    throw await createError(response);
  }
  return response;
}

function encodePath (...segments) {
  return segments.map(segment => encodeURIComponent(segment)).join('/');
}

function splitPlatform (platform) {
  const [os, architecture] = platform.split('/');
  return [os, architecture];
}

/**
 * Extracts the file name from a Content-Disposition header
 * (supports filename*=UTF-8''..., quoted and unquoted filename=...).
 */
export function fileNameFromContentDisposition (header, fallback = 'download') {
  if (!header) {
    return fallback;
  }
  const extended = /filename\*\s*=\s*UTF-8''([^;]+)/i.exec(header);
  if (extended) {
    try {
      return decodeURIComponent(extended[1].trim());
    } catch {
      // Invalid encoding, try the plain file name
    }
  }
  const quoted = /filename\s*=\s*"([^"]*)"/i.exec(header);
  if (quoted) {
    return quoted[1];
  }
  const plain = /filename\s*=\s*([^;]+)/i.exec(header);
  return plain ? plain[1].trim() : fallback;
}

export async function getProducts () {
  const response = await request('/');
  const { products } = await response.json();
  return products;
}

/**
 * Loads all releases of a product (all pages), newest first.
 */
export async function getProductInfo (product) {
  let offset = 0;
  let info = null;
  do {
    const response = await request(`/${encodePath(product)}/info?limit=${PAGE_SIZE}&offset=${offset}`);
    const page = await response.json();
    info = info ? { ...page, releases: [...info.releases, ...page.releases] } : page;
    offset = page.nextOffset;
  } while (offset !== null && offset !== undefined);
  return info;
}

export async function getStatistics () {
  const response = await request('/statistics', { authenticated: true });
  return response.json();
}

/**
 * Checks the credentials with an authenticated request and stores them for the session.
 */
export async function login (username, password) {
  await request('/statistics', { credentials: { username, password } });
  setCredentials({ username, password });
}

export function logout () {
  clearCredentials();
}

export async function uploadPackage (file, force = false) {
  const form = new FormData();
  form.append('package', file, file.name);
  await request(`/?force=${force ? 'true' : 'false'}`, { method: 'PUT', body: form, authenticated: true });
}

/**
 * The public download URL of a release for one platform (no authentication needed).
 */
export function artifactUrl (product, platform, version) {
  return `${BASE_URL}/${encodePath(product, ...splitPlatform(platform), version)}`;
}

/**
 * Deletes a release on all its platforms. The server only deletes one platform per request, so a
 * failure can leave the release on some platforms; the error lists them.
 */
export async function deleteRelease (product, version, platforms) {
  const failed = [];
  for (const platform of platforms) {
    try {
      await request(`/${encodePath(product, ...splitPlatform(platform), version)}`, { method: 'DELETE', authenticated: true });
    } catch (error) {
      failed.push(`${platform} (${error.message})`);
    }
  }
  if (failed.length > 0) {
    throw new ApiError(0, 'The release was not deleted on all platforms', failed.join(', '));
  }
}

export async function createBackup () {
  const response = await request('/backup', { authenticated: true });
  const blob = await response.blob();
  const fileName = fileNameFromContentDisposition(response.headers.get('content-disposition'), 'backup.zip');
  return { blob, fileName };
}

export async function restoreBackup (file) {
  const form = new FormData();
  form.append('backupFile', file, file.name);
  await request('/restore', { method: 'PUT', body: form, authenticated: true });
}
