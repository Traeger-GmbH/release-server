#!/usr/bin/env node
// Compares the public API of two running release servers with GET requests only, e.g. the production
// server and a new version started on a copy of the production data:
//
//   node tools/compat/compare-servers.mjs --a https://releases.traeger.de --b http://localhost:5099
//
// Compared: product list, /info of every product (several query variants) and the 404 responses.
// With --downloads, every artifact is downloaded from both servers and compared by SHA-256 (this
// transfers all files twice). No authentication is used and nothing is changed on either server.
// The field "artifacts" (version 1.1) is only compared with --compare-artifacts.
// With --check-head, every compared URL is also requested with HEAD on both servers (version 1.1.1+):
// HEAD must return the status and headers of GET without body.

import { createHash } from 'node:crypto';
import { isDeepStrictEqual } from 'node:util';

const args = Object.fromEntries(process.argv.slice(2).reduce((pairs, value, index, all) => {
  if (!value.startsWith('--')) {
    return pairs;
  }
  const next = all[index + 1];
  return [...pairs, [value.substring(2), next && !next.startsWith('--') ? next : true]];
}, []));

if (!args.a || !args.b) {
  console.error('Usage: node tools/compat/compare-servers.mjs --a <url> --b <url> [--downloads]');
  process.exit(2);
}

function normalize (value, key) {
  if (Array.isArray(value)) {
    const items = value.map(item => normalize(item));
    return (key === 'platforms' || key === 'products') && items.every(item => typeof item === 'string') ? items.sort() : items;
  }
  if (value && typeof value === 'object') {
    // "artifacts" was added in version 1.1 (additive); it is not compared, so older servers can be compared
    return Object.fromEntries(Object.entries(value)
      .filter(([k]) => k !== 'artifacts' || args['compare-artifacts'])
      .map(([k, v]) => [k, k === 'traceId' ? '<traceId>' : normalize(v, k)]));
  }
  return value;
}

async function get (base, path, download = false) {
  const response = await fetch(base + path);
  const body = Buffer.from(await response.arrayBuffer());
  const result = { status: response.status, contentType: response.headers.get('content-type') };
  if (download) {
    result.contentDisposition = response.headers.get('content-disposition');
    result.sha256 = createHash('sha256').update(body).digest('hex');
    result.size = body.length;
  } else if (result.contentType && result.contentType.includes('json')) {
    result.json = JSON.stringify(normalize(JSON.parse(body.toString('utf8'))));
  } else {
    result.body = body.toString('utf8');
  }
  return result;
}

// Version 1.1.1+: HEAD returns the status and the headers of GET, without body
const HEAD_COMPARED_HEADERS = ['content-type', 'content-length', 'content-disposition', 'etag', 'repr-digest', 'accept-ranges', 'last-modified'];

async function headDifference (base, path) {
  const get = await fetch(base + path);
  const getBody = Buffer.from(await get.arrayBuffer());
  const head = await fetch(base + path, { method: 'HEAD' });
  const headBody = Buffer.from(await head.arrayBuffer());
  const differences = [];
  if (get.status !== head.status) differences.push(`status GET ${get.status}, HEAD ${head.status}`);
  for (const name of HEAD_COMPARED_HEADERS) {
    // The length of problem details differs by the trace id
    if (name === 'content-length' && (get.headers.get('content-type') ?? '').includes('problem+json')) continue;
    if (get.headers.get(name) !== head.headers.get(name)) differences.push(`${name}: GET ${get.headers.get(name)}, HEAD ${head.headers.get(name)}`);
  }
  if (headBody.length > 0) differences.push(`HEAD has a body of ${headBody.length} bytes`);
  if (get.headers.get('content-length') !== null && Number(get.headers.get('content-length')) !== getBody.length) differences.push('GET content-length does not match its body');
  return differences;
}

let compared = 0;
const differences = [];
const headProblems = [];

async function compare (path, download = false) {
  if (args['check-head']) {
    for (const [name, base] of [['a', args.a], ['b', args.b]]) {
      for (const difference of await headDifference(base, path)) headProblems.push(`${name} ${path}: ${difference}`);
    }
  }
  const [a, b] = await Promise.all([get(args.a, path, download), get(args.b, path, download)]);
  compared++;
  if (!isDeepStrictEqual(a, b)) {
    differences.push(path);
    console.log(`DIFF  ${path}\n  a: ${JSON.stringify(a).slice(0, 2000)}\n  b: ${JSON.stringify(b).slice(0, 2000)}`);
  }
  return a;
}

// Deliberate change in version 1.0: products without any release (directory skeletons, /info 404) are
// no longer listed. They are removed from both lists before comparing and reported separately.
async function productsWithReleases (base) {
  const listed = JSON.parse((await get(base, '/artifacts/')).json).products;
  const result = [];
  for (const product of listed) {
    if ((await fetch(`${base}/artifacts/${encodeURIComponent(product)}/info`)).status !== 404) {
      result.push(product);
    }
  }
  return { listed, withReleases: result.sort() };
}

const [listA, listB] = await Promise.all([productsWithReleases(args.a), productsWithReleases(args.b)]);
compared++;
if (!isDeepStrictEqual(listA.withReleases, listB.withReleases)) {
  differences.push('/artifacts/');
  console.log(`DIFF  /artifacts/\n  a: ${JSON.stringify(listA.withReleases)}\n  b: ${JSON.stringify(listB.withReleases)}`);
}
for (const [name, list] of [['a', listA], ['b', listB]]) {
  const skeletons = list.listed.filter(product => !list.withReleases.includes(product));
  if (skeletons.length > 0) {
    console.log(`INFO  ${name} lists products without releases (expected only for versions before 1.0): ${skeletons.join(', ')}`);
  }
}
const products = listA.withReleases;

for (const product of products) {
  const base = `/artifacts/${encodeURIComponent(product)}`;
  for (const query of ['?limit=1000', '', '?sortOrder=ascending&limit=1000', '?limit=1&offset=1', '?operatingSystems=windows,linux&architectures=x64,x86,any']) {
    await compare(`${base}/info${query}`);
  }

  if (args.downloads) {
    const info = await fetch(`${args.a}${base}/info?limit=1000`).then(response => response.ok ? response.json() : { releases: [] });
    for (const release of info.releases) {
      for (const platform of release.platforms) {
        await compare(`${base}/${platform}/${release.version}`, true);
      }
    }
  }
}

for (const path of ['/artifacts/unknown/info', '/artifacts/unknown/windows/x64/1.0', '/artifacts/unknown/windows/x64/latest']) {
  await compare(path);
}

console.log(`${compared} responses compared for ${products.length} products, ${differences.length} difference(s)`);
if (args['check-head']) {
  headProblems.forEach(problem => console.log(`HEAD  ${problem}`));
  console.log(`HEAD checked for ${compared} URLs on both servers, ${headProblems.length} problem(s)`);
}
process.exit(differences.length > 0 || headProblems.length > 0 ? 1 : 0);
