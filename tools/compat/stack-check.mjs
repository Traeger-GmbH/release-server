#!/usr/bin/env node
// End-to-end check of the deployed stack: release server and release client containers in one Docker
// network (like the Portainer stack). Everything is accessed through the client (nginx), which serves
// the UI and forwards /artifacts to the server.
//
// Usage: node tools/compat/stack-check.mjs --server <image> --client <image> [--password <password>]
// Requirements: docker, python3 (to create ZIP files), Node.js 18 or newer.

import { execFileSync } from 'node:child_process';
import { createHash, randomBytes } from 'node:crypto';
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const args = Object.fromEntries(process.argv.slice(2).reduce((pairs, value, index, all) =>
  index % 2 === 0 ? [...pairs, [value.replace(/^--/, ''), all[index + 1]]] : pairs, []));
if (!args.server || !args.client) {
  console.error('Usage: node tools/compat/stack-check.mjs --server <image> --client <image>');
  process.exit(2);
}

const prefix = `rsstack-${randomBytes(4).toString('hex')}`;
const network = `${prefix}-net`;
const port = Number(args.port ?? 13000);
const baseUrl = `http://127.0.0.1:${port}`;
// Servers before version 1.0 do not accept ':' in passwords; --password allows checking them as well
const password = args.password ?? `p:w&d%#${randomBytes(4).toString('hex')}`;
const authHeader = 'Basic ' + Buffer.from(`admin:${password}`).toString('base64');
const work = mkdtempSync(join(tmpdir(), `${prefix}-`));
const failures = [];

function docker (...dockerArgs) {
  return execFileSync('docker', dockerArgs, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] }).trim();
}

function check (name, condition, info = '') {
  if (condition) {
    console.log(`[stack] ok    ${name}`);
  } else {
    failures.push(name);
    console.log(`[stack] FAIL  ${name} ${info}`);
  }
}

async function waitFor (url, accept) {
  for (let attempt = 0; attempt < 60; attempt++) {
    try {
      const response = await fetch(url);
      if (accept(response)) {
        return response;
      }
    } catch {
      // Not ready yet
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  throw new Error(`${url} did not become ready`);
}

function createLargePackage (sizeInMb) {
  // Random content does not compress, so the ZIP file has about the given size
  const root = join(work, 'large-v1.0.0');
  mkdirSync(join(root, 'files'), { recursive: true });
  writeFileSync(join(root, 'package.json'), JSON.stringify({
    identifier: 'large-product',
    version: '1.0.0',
    releaseNotes: { releaseDate: '2026-10-24', changes: { en: [{ added: ['Large file'] }] } },
    deployments: { 'any/any': 'files/large.bin' }
  }));
  writeFileSync(join(root, 'files', 'large.bin'), randomBytes(sizeInMb * 1024 * 1024));
  const zip = join(work, 'large.zip');
  execFileSync('python3', ['-m', 'zipfile', '-c', zip, 'large-v1.0.0'], { cwd: work });
  return readFileSync(zip);
}

async function put (path, field, content) {
  const form = new FormData();
  form.append(field, new Blob([content], { type: 'application/zip' }), 'upload.zip');
  const response = await fetch(baseUrl + path, { method: 'PUT', body: form, headers: { Authorization: authHeader } });
  return { status: response.status, body: await response.text() };
}

async function main () {
  docker('network', 'create', network);

  // The client starts first: it must not depend on the server being reachable at start up
  docker('run', '-d', '--name', `${prefix}-client`, '--network', network, '-p', `127.0.0.1:${port}:3000`,
    '-e', 'RELEASE_SERVER_URL=http://release-server:5000/', args.client);
  const index = await waitFor(`${baseUrl}/`, response => response.ok);
  check('client serves the UI', (await index.text()).includes('<div id="app">'));

  const unavailable = await fetch(`${baseUrl}/artifacts/`);
  check('client answers while the server is down', unavailable.status === 502, `(status ${unavailable.status})`);

  docker('run', '-d', '--name', `${prefix}-server`, '--network', network, '--network-alias', 'release-server',
    '-e', 'CREDENTIALS_USERNAME=admin', '-e', `CREDENTIALS_PASSWORD=${password}`, args.server);
  await waitFor(`${baseUrl}/artifacts/`, response => response.ok);
  check('client forwards the API to the server', true);

  const deepLink = await fetch(`${baseUrl}/products/large-product`);
  check('deep links return the UI', deepLink.ok && (await deepLink.text()).includes('<div id="app">'));

  const missingAsset = await fetch(`${baseUrl}/assets/does-not-exist.js`);
  check('missing assets are not answered with the UI', missingAsset.status === 404);

  const unauthorized = await fetch(`${baseUrl}/artifacts/statistics`);
  check('authentication is enforced through the proxy', unauthorized.status === 401);

  // 200 MB: above the former multipart limit of 128 MB of the server
  const large = createLargePackage(200);
  const upload = await put('/artifacts', 'package', large);
  check('upload of a 200 MB package through the proxy', upload.status === 200, `(status ${upload.status}: ${upload.body.slice(0, 300)})`);

  const download = await fetch(`${baseUrl}/artifacts/large-product/any/any/latest`);
  const downloaded = Buffer.from(await download.arrayBuffer());
  const original = readFileSync(join(work, 'large-v1.0.0', 'files', 'large.bin'));
  check('download through the proxy is identical', download.ok &&
    createHash('sha256').update(downloaded).digest('hex') === createHash('sha256').update(original).digest('hex'));
  check('download keeps the file name', download.headers.get('content-disposition') === 'attachment; filename=large.bin',
    `(${download.headers.get('content-disposition')})`);

  const partial = await fetch(`${baseUrl}/artifacts/large-product/any/any/1.0.0`, { headers: { Range: 'bytes=100-199' } });
  check('range requests work through the proxy', partial.status === 206 && (await partial.arrayBuffer()).byteLength === 100);

  // A backup of more than 30 MB (the former request limit of the restore endpoint)
  const backupResponse = await fetch(`${baseUrl}/artifacts/backup`, { headers: { Authorization: authHeader } });
  const backup = Buffer.from(await backupResponse.arrayBuffer());
  check('backup through the proxy', backupResponse.ok && backup.length > 30 * 1024 * 1024, `(${backup.length} bytes)`);

  const restore = await put('/artifacts/restore', 'backupFile', backup);
  check('restore of a backup larger than 30 MB', restore.status === 200, `(status ${restore.status}: ${restore.body.slice(0, 300)})`);

  const products = await (await fetch(`${baseUrl}/artifacts/`)).json();
  check('data is complete after the restore', JSON.stringify(products.products) === '["large-product"]', JSON.stringify(products));
}

try {
  await main();
} catch (error) {
  failures.push(String(error));
  console.error(error);
} finally {
  for (const name of ['client', 'server']) {
    try { docker('rm', '-f', `${prefix}-${name}`); } catch { /* not running */ }
  }
  try { docker('network', 'rm', network); } catch { /* already removed */ }
  rmSync(work, { recursive: true, force: true });
}

if (failures.length > 0) {
  console.log(`[stack] ${failures.length} check(s) failed`);
  process.exit(1);
}
console.log('[stack] all checks passed');
