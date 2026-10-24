import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import {
  ApiError, artifactUrl, basicAuthorization, createBackup, deleteRelease, fileNameFromContentDisposition,
  getProductInfo, getProducts, getStatistics, login, uploadPackage
} from '../src/api.js';
import { clearCredentials, getCredentials, isLoggedIn, setCredentials } from '../src/auth.js';
import { authorizationOf, json, mockFetch, problem } from './helpers.js';

const release = (version, platforms = ['windows/x64']) => ({
  version, platforms, releaseDate: '2021-05-07T00:00:00', isPreviewRelease: false, isSecurityPatch: false, changes: {}
});

describe('api', () => {
  beforeEach(() => clearCredentials());
  afterEach(() => clearCredentials());

  it('encodes credentials with special characters as UTF-8 Basic authorization', () => {
    const header = basicAuthorization({ username: 'admin', password: 'p:ä&%#' });
    const decoded = new TextDecoder().decode(Uint8Array.from(atob(header.substring(6)), c => c.charCodeAt(0)));
    expect(header.startsWith('Basic ')).toBe(true);
    expect(decoded).toBe('admin:p:ä&%#');
  });

  it('loads the product list without authentication', async () => {
    const fetchMock = mockFetch(() => json({ products: ['b', 'a'] }));
    setCredentials({ username: 'u', password: 'p' });

    expect(await getProducts()).toEqual(['b', 'a']);
    expect(fetchMock.mock.calls[0][0]).toBe('/artifacts/');
    expect(authorizationOf(fetchMock.mock.calls[0][1])).toBeNull();
  });

  it('loads all pages of the releases of a product', async () => {
    const fetchMock = mockFetch((url) => url.includes('offset=0')
      ? json({ identifier: 'p', releases: [release('2.0')], totalReleaseCount: 2, nextOffset: 1000 })
      : json({ identifier: 'p', releases: [release('1.0')], totalReleaseCount: 2, nextOffset: null }));

    const info = await getProductInfo('p');

    expect(info.releases.map(r => r.version)).toEqual(['2.0', '1.0']);
    expect(fetchMock.mock.calls.map(call => call[0])).toEqual([
      '/artifacts/p/info?limit=1000&offset=0',
      '/artifacts/p/info?limit=1000&offset=1000'
    ]);
  });

  it('encodes product names in URLs', async () => {
    const fetchMock = mockFetch(() => json({ identifier: 'a b', releases: [], nextOffset: null }));

    await getProductInfo('a b');

    expect(fetchMock.mock.calls[0][0]).toBe('/artifacts/a%20b/info?limit=1000&offset=0');
    expect(artifactUrl('s5-lan-3964R', 'Windows/X86', '1.36')).toBe('/artifacts/s5-lan-3964R/Windows/X86/1.36');
  });

  it('turns problem details into readable errors', async () => {
    mockFetch(() => problem(404, 'Resource not found.', 'The specified product does not exist.'));

    const error = await getProductInfo('unknown').catch(e => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(404);
    expect(error.message).toBe('Resource not found.: The specified product does not exist.');
  });

  it('reports an unreachable server', async () => {
    mockFetch(() => { throw new TypeError('Failed to fetch'); });

    const error = await getProducts().catch(e => e);

    expect(error.status).toBe(0);
    expect(error.title).toBe('The release server is not reachable');
  });

  it('stores the credentials only after a successful login', async () => {
    const fetchMock = mockFetch((url, options) => authorizationOf(options) === basicAuthorization({ username: 'u', password: 'right' })
      ? json({}) : problem(401, 'Unauthorized'));

    await expect(login('u', 'wrong')).rejects.toMatchObject({ status: 401 });
    expect(isLoggedIn.value).toBe(false);

    await login('u', 'right');
    expect(isLoggedIn.value).toBe(true);
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });

  it('logs out when stored credentials are rejected', async () => {
    setCredentials({ username: 'u', password: 'old' });
    mockFetch(() => problem(401, 'Unauthorized'));

    await expect(getStatistics()).rejects.toMatchObject({ status: 401 });

    expect(isLoggedIn.value).toBe(false);
    expect(getCredentials()).toBeNull();
  });

  it('uploads a package with the force flag and authentication', async () => {
    setCredentials({ username: 'u', password: 'p' });
    const fetchMock = mockFetch(() => new Response(null, { status: 200 }));
    const file = new File(['zip'], 'package.zip', { type: 'application/zip' });

    await uploadPackage(file, true);

    const [url, options] = fetchMock.mock.calls[0];
    expect(url).toBe('/artifacts/?force=true');
    expect(options.method).toBe('PUT');
    expect(options.body.get('package').name).toBe('package.zip');
    expect(authorizationOf(options)).toBe(basicAuthorization({ username: 'u', password: 'p' }));
  });

  it('deletes a release on every platform and reports failed platforms', async () => {
    setCredentials({ username: 'u', password: 'p' });
    const fetchMock = mockFetch((url) => url.includes('/linux/') ? problem(404, 'Resource not found') : new Response(null, { status: 200 }));

    const error = await deleteRelease('p', '1.0', ['windows/x64', 'linux/x64']).catch(e => e);

    expect(fetchMock.mock.calls.map(call => [call[1].method, call[0]])).toEqual([
      ['DELETE', '/artifacts/p/windows/x64/1.0'],
      ['DELETE', '/artifacts/p/linux/x64/1.0']
    ]);
    expect(error.message).toContain('linux/x64');
    expect(error.message).not.toContain('windows/x64');
  });

  it('downloads a backup with its file name', async () => {
    setCredentials({ username: 'u', password: 'p' });
    mockFetch(() => new Response('zip', {
      status: 200,
      headers: { 'Content-Disposition': "attachment; filename=backup_2026-10-24_10-00-00.zip; filename*=UTF-8''backup_2026-10-24_10-00-00.zip" }
    }));

    const backup = await createBackup();

    expect(backup.fileName).toBe('backup_2026-10-24_10-00-00.zip');
    expect(await backup.blob.text()).toBe('zip');
  });

  it.each([
    ['attachment; filename=alpha.zip', 'alpha.zip'],
    ['attachment; filename="Setup S5 LAN.exe"', 'Setup S5 LAN.exe'],
    ["attachment; filename=a.zip; filename*=UTF-8''%C3%BCbersicht.zip", 'übersicht.zip'],
    [null, 'download']
  ])('reads the file name from "%s"', (header, expected) => {
    expect(fileNameFromContentDisposition(header)).toBe(expected);
  });
});
