#!/usr/bin/env node
// End-to-end compatibility check between two release server images (e.g. the production image and a
// new build). Both servers run one after another on the same Docker volumes:
//
//   1. The old server starts on a data set rebuilt from the production snapshot and receives uploads.
//   2. The new server takes over the same volumes and must answer exactly like the old one.
//   3. Backups of the new server are restored by the new and by the old server.
//   4. Data written by the new server must still be readable by the old server (rollback).
//
// Usage: node tools/compat/compat-check.mjs --old <image> --new <image>
// Requirements: docker, python3 (to create ZIP files), Node.js 18 or newer.

import { execFileSync } from 'node:child_process';
import { createHash, randomBytes } from 'node:crypto';
import { mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { isDeepStrictEqual } from 'node:util';

const repoRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const snapshotDir = join(repoRoot, 'sources', 'ReleaseServer', 'ReleaseServer.WebApi.Test', 'Integration', 'LiveSnapshot');

const args = Object.fromEntries(process.argv.slice(2).reduce((pairs, value, index, all) =>
  index % 2 === 0 ? [...pairs, [value.replace(/^--/, ''), all[index + 1]]] : pairs, []));
if (!args.old || !args.new) {
  console.error('Usage: node tools/compat/compat-check.mjs --old <image> --new <image>');
  process.exit(2);
}

const prefix = `rscompat-${randomBytes(4).toString('hex')}`;
const volumes = { artifacts: `${prefix}_artifacts`, backups: `${prefix}_backups` };
// The old server cannot handle ':' in passwords, so the check uses other special characters only.
const credentials = { username: 'compat-user', password: `p&w%d#${randomBytes(4).toString('hex')}` };
const authHeader = 'Basic ' + Buffer.from(`${credentials.username}:${credentials.password}`).toString('base64');
const port = Number(args.port ?? 15080);
const baseUrl = `http://127.0.0.1:${port}`;
const work = mkdtempSync(join(tmpdir(), `${prefix}-`));
const failures = [];

function docker (...dockerArgs) {
  return execFileSync('docker', dockerArgs, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] }).trim();
}

function log (message) {
  console.log(`[compat] ${message}`);
}

function check (name, expected, actual) {
  if (isDeepStrictEqual(expected, actual)) {
    log(`ok    ${name}`);
  } else {
    failures.push(name);
    log(`FAIL  ${name}`);
    console.log('  expected: ' + JSON.stringify(expected, null, 1).slice(0, 4000));
    console.log('  actual:   ' + JSON.stringify(actual, null, 1).slice(0, 4000));
  }
}

// ---------- Test data ----------

function createSeed () {
  // Rebuild the production data set (hand made, camelCase meta files) from the snapshot.
  const seed = join(work, 'seed');
  for (const file of readdirSync(join(snapshotDir, 'info'))) {
    const product = file.replace(/\.json$/, '');
    const info = JSON.parse(readFileSync(join(snapshotDir, 'info', file), 'utf8'));
    mkdirSync(join(seed, product), { recursive: true });
    for (const release of info.releases ?? []) {
      for (const platform of release.platforms) {
        const [os, architecture] = platform.split('/');
        const dir = join(seed, product, os, architecture, release.version);
        mkdirSync(dir, { recursive: true });
        writeFileSync(join(dir, 'deployment.json'), JSON.stringify({ releaseNotesFileName: 'releaseNotes.json', artifactFileName: 'artifact.bin' }, null, 2));
        writeFileSync(join(dir, 'releaseNotes.json'), JSON.stringify({
          releaseDate: release.releaseDate,
          isPreviewRelease: release.isPreviewRelease,
          isSecurityPatch: release.isSecurityPatch,
          changes: release.changes,
        }, null, 2));
        writeFileSync(join(dir, 'artifact.bin'), `artifact ${product}/${platform}/${release.version}`);
      }
    }
  }
  addKnownSpecialCases(seed);
  return seed;
}

function addKnownSpecialCases (seed) {
  // Structures in the production data that the public API does not reveal. Old server versions
  // extracted the complete upload archive into the artifact directory, e.g. the macOS metadata of a
  // Finder ZIP (production: testproduct/windows/arm64/1.0.0/__MACOSX/._releaseNotes.json).
  const macOsMetadata = join(seed, 'testproduct', 'windows', 'arm64', '1.0.0', '__MACOSX');
  mkdirSync(macOsMetadata, { recursive: true });
  writeFileSync(join(macOsMetadata, '._releaseNotes.json'), Buffer.from('00051607000200004d6163204f532058', 'hex'));

  // Old versions also accepted artifacts in sub directories of the upload archive
  const subfolder = join(seed, 'legacy-subfolder', 'windows', 'x64', '0.9');
  mkdirSync(join(subfolder, 'bin'), { recursive: true });
  writeFileSync(join(subfolder, 'deployment.json'), JSON.stringify({ releaseNotesFileName: 'releaseNotes.json', artifactFileName: 'bin/setup.exe' }));
  writeFileSync(join(subfolder, 'releaseNotes.json'), JSON.stringify({ releaseDate: '2021-05-07', changes: { de: [{ added: ['Alt'] }] } }));
  writeFileSync(join(subfolder, 'bin', 'setup.exe'), 'artifact legacy-subfolder 0.9');
  writeFileSync(join(subfolder, 'readme.txt'), 'extracted with the artifact');
}

function zipDirectory (sourceDir, entries, target) {
  execFileSync('python3', ['-m', 'zipfile', '-c', target, ...entries], { cwd: sourceDir });
  return readFileSync(target);
}

function createPackage (identifier, version) {
  const root = join(work, `${identifier}-v${version}`);
  mkdirSync(join(root, 'files'), { recursive: true });
  writeFileSync(join(root, 'package.json'), JSON.stringify({
    identifier,
    version,
    releaseNotes: { releaseDate: '2026-10-24', isPreviewRelease: false, changes: { de: [{ added: ['Neu'] }], en: [{ added: ['New'] }] } },
    deployments: { 'windows/x64': 'files/setup-x64.exe', 'linux/arm64': 'files/app-arm64.tar.gz', 'any/any': 'files/docs.zip' },
  }, null, 2));
  for (const file of ['setup-x64.exe', 'app-arm64.tar.gz', 'docs.zip']) {
    writeFileSync(join(root, 'files', file), `${identifier} ${version} ${file}`);
  }
  return zipDirectory(work, [`${identifier}-v${version}`], join(work, `${identifier}-${version}.zip`));
}

function createSingleArtifact () {
  return zipDirectory(join(repoRoot, 'examples', 'artifact-foobar-v2.0.0-preview-2_windows-x64'),
    ['deployment.json', 'releaseNotes.json', 'artifact.txt'], join(work, 'single.zip'));
}

// ---------- Server handling ----------

async function startServer (image, name) {
  docker('run', '-d', '--name', name, '-p', `127.0.0.1:${port}:5000`,
    '-e', `CREDENTIALS_USERNAME=${credentials.username}`, '-e', `CREDENTIALS_PASSWORD=${credentials.password}`,
    '-v', `${volumes.artifacts}:/app/artifacts`, '-v', `${volumes.backups}:/app/backups`, image);

  for (let attempt = 0; attempt < 60; attempt++) {
    try {
      const response = await fetch(`${baseUrl}/artifacts/`);
      if (response.ok) {
        log(`started ${name} (${image})`);
        return;
      }
    } catch {
      // Not ready yet
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  console.log(docker('logs', name));
  throw new Error(`Server ${name} did not start.`);
}

function stopServer (name) {
  docker('rm', '-f', name);
}

async function upload (path, field, content) {
  const form = new FormData();
  form.append(field, new Blob([content], { type: 'application/zip' }), 'upload.zip');
  const response = await fetch(baseUrl + path, { method: 'PUT', body: form, headers: { Authorization: authHeader } });
  return response.status;
}

// ---------- Snapshot of all public responses ----------

function normalize (value, key) {
  if (Array.isArray(value)) {
    const items = value.map(item => normalize(item));
    return (key === 'platforms' || key === 'products') && items.every(item => typeof item === 'string') ? items.sort() : items;
  }
  if (value && typeof value === 'object') {
    // "artifacts" was added in version 1.1 (additive) and is checked separately (checkArtifacts)
    return Object.fromEntries(Object.entries(value)
      .filter(([k]) => k !== 'artifacts')
      .map(([k, v]) => [k, k === 'traceId' ? '<traceId>' : normalize(v, k)]));
  }
  return value;
}

async function request (path, options = {}) {
  const response = await fetch(baseUrl + path, options);
  const body = Buffer.from(await response.arrayBuffer());
  const contentType = response.headers.get('content-type');
  const result = { status: response.status, contentType };
  if (contentType && contentType.includes('json')) {
    // Keep the property order: it is compared as part of the JSON text.
    result.json = JSON.stringify(normalize(JSON.parse(body.toString('utf8'))));
  } else {
    result.contentDisposition = response.headers.get('content-disposition');
    result.sha256 = createHash('sha256').update(body).digest('hex');
  }
  return result;
}

async function snapshot () {
  const result = {};
  const list = await request('/artifacts/');
  // Deliberate change in version 1.0: products without any release (directory skeletons, /info 404)
  // are no longer listed. They are removed from the list of both servers before comparing.
  const listed = JSON.parse(list.json).products;
  const products = [];
  for (const product of listed) {
    if ((await fetch(`${baseUrl}/artifacts/${encodeURIComponent(product)}/info`)).status !== 404) {
      products.push(product);
    }
  }
  result['GET /artifacts/'] = { ...list, json: JSON.stringify({ products: [...products].sort() }) };

  for (const product of products) {
    const base = `/artifacts/${encodeURIComponent(product)}`;
    for (const query of ['?limit=1000', '', '?sortOrder=ascending&limit=1000', '?limit=1&offset=1', '?operatingSystems=windows,linux&architectures=x64,any']) {
      result[`GET ${base}/info${query}`] = await request(`${base}/info${query}`);
    }
    const info = await fetch(`${baseUrl}${base}/info?limit=1000`).then(r => r.ok ? r.json() : { releases: [] });
    for (const release of info.releases) {
      for (const platform of release.platforms) {
        result[`GET ${base}/${platform}/${release.version}`] = await request(`${base}/${platform}/${release.version}`);
        result[`GET ${base}/${platform}/latest`] = await request(`${base}/${platform}/latest`);
      }
    }
  }

  for (const path of ['/artifacts/unknown/info', '/artifacts/unknown/windows/x64/1.0', '/artifacts/unknown/windows/x64/latest', '/artifacts/a/b/c/d/e/f']) {
    result[`GET ${path}`] = await request(path);
  }

  const statistics = await fetch(`${baseUrl}/artifacts/statistics`, { headers: { Authorization: authHeader } }).then(r => r.json());
  // Products without releases are counted by versions before 1.0 only (see above)
  const productsWithoutReleases = listed.length - products.length;
  result['GET /artifacts/statistics (counters)'] = {
    numberOfProducts: statistics.numberOfProducts - productsWithoutReleases,
    numberOfArtifacts: statistics.numberOfArtifacts
  };
  result['DELETE /artifacts/unknown (unauthenticated)'] = (await request('/artifacts/unknown', { method: 'DELETE' })).status;
  return result;
}

function compareSnapshots (name, expected, actual) {
  const keys = [...new Set([...Object.keys(expected), ...Object.keys(actual)])].sort();
  let differences = 0;
  for (const key of keys) {
    if (!isDeepStrictEqual(expected[key], actual[key])) {
      differences++;
      check(`${name}: ${key}`, expected[key], actual[key]);
    }
  }
  if (differences === 0) {
    log(`ok    ${name}: ${keys.length} responses identical`);
  }
}

// Version 1.1+: every release lists its files with size and SHA-256. The checksums of existing data are
// added in the background after the start; wait for them and compare them with the downloaded files.
async function checkArtifacts (name) {
  const products = (await fetch(`${baseUrl}/artifacts/`).then(r => r.json())).products;
  let artifacts = [];
  for (let attempt = 0; attempt < 120; attempt++) {
    artifacts = [];
    for (const product of products) {
      const info = await fetch(`${baseUrl}/artifacts/${encodeURIComponent(product)}/info?limit=1000`).then(r => r.ok ? r.json() : { releases: [] });
      artifacts.push(...info.releases.flatMap(release => release.artifacts ?? []));
    }
    if (artifacts.every(artifact => artifact.sha256)) {
      break;
    }
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
  let mismatches = 0;
  for (const artifact of artifacts) {
    const body = Buffer.from(await (await fetch(baseUrl + artifact.downloadUrl)).arrayBuffer());
    if (artifact.sha256 !== createHash('sha256').update(body).digest('hex') || artifact.size !== body.length) {
      mismatches++;
    }
  }
  check(`${name}: ${artifacts.length} artifacts with matching size and SHA-256`, 0, mismatches + artifacts.filter(a => !a.sha256).length);
}

async function backup () {
  const response = await fetch(`${baseUrl}/artifacts/backup`, { headers: { Authorization: authHeader } });
  check('backup status', 200, response.status);
  return Buffer.from(await response.arrayBuffer());
}

// ---------- Scenario ----------

async function main () {
  docker('volume', 'create', volumes.artifacts);
  docker('volume', 'create', volumes.backups);
  const seed = createSeed();
  docker('run', '--rm', '-v', `${volumes.artifacts}:/data`, '-v', `${seed}:/seed:ro`, '--entrypoint', 'sh', args.old, '-c', 'cp -a /seed/. /data/');

  // 1. Old server: production data plus uploads written by the old server itself.
  await startServer(args.old, `${prefix}-old`);
  check('old: upload package', 200, await upload('/artifacts', 'package', createPackage('compat-package', '1.2.3')));
  check('old: upload artifact', 200, await upload('/artifacts/compat-single/windows/x64/2.0.0-preview-2', 'artifact', createSingleArtifact()));
  const oldSnapshot = await snapshot();
  const oldBackup = await backup();
  stopServer(`${prefix}-old`);

  // 2. New server on the same volumes.
  await startServer(args.new, `${prefix}-new`);
  compareSnapshots('new reads old data', oldSnapshot, await snapshot());
  await checkArtifacts('new adds checksums to old data');

  // 3. Restore of an old backup and of an own backup.
  check('new: restore old backup', 200, await upload('/artifacts/restore', 'backupFile', oldBackup));
  compareSnapshots('new after restoring old backup', oldSnapshot, await snapshot());
  const newBackup = await backup();
  check('new: restore own backup', 200, await upload('/artifacts/restore', 'backupFile', newBackup));
  compareSnapshots('new after restoring own backup', oldSnapshot, await snapshot());

  // 4. Rollback: data written by the new server is read by the old server.
  check('new: upload package', 200, await upload('/artifacts', 'package', createPackage('compat-rollback', '3.0.0')));
  check('new: upload artifact', 200, await upload('/artifacts/compat-single/linux/x64/2.1.0', 'artifact', createSingleArtifact()));
  const newSnapshot = await snapshot();
  const rollbackBackup = await backup();
  stopServer(`${prefix}-new`);

  await startServer(args.old, `${prefix}-rollback`);
  compareSnapshots('old reads data of new server', newSnapshot, await snapshot());
  check('old: restore backup of new server', 200, await upload('/artifacts/restore', 'backupFile', rollbackBackup));
  compareSnapshots('old after restoring backup of new server', newSnapshot, await snapshot());
  stopServer(`${prefix}-rollback`);
}

try {
  await main();
} catch (error) {
  failures.push(String(error));
  console.error(error);
} finally {
  for (const name of ['old', 'new', 'rollback']) {
    try { docker('rm', '-f', `${prefix}-${name}`); } catch { /* not running */ }
  }
  try { docker('volume', 'rm', volumes.artifacts, volumes.backups); } catch { /* already removed */ }
  rmSync(work, { recursive: true, force: true });
}

if (failures.length > 0) {
  console.log(`[compat] ${failures.length} check(s) failed`);
  process.exit(1);
}
console.log('[compat] all checks passed');
