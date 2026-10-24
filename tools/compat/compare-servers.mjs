#!/usr/bin/env node
// Compares the public API of two running release servers with GET requests only, e.g. the production
// server and a new version started on a copy of the production data:
//
//   node tools/compat/compare-servers.mjs --a https://releases.traeger.de --b http://localhost:5099
//
// Compared: product list, /info of every product (several query variants) and the 404 responses.
// With --downloads, every artifact is downloaded from both servers and compared by SHA-256 (this
// transfers all files twice). No authentication is used and nothing is changed on either server.

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
    return Object.fromEntries(Object.entries(value).map(([k, v]) => [k, k === 'traceId' ? '<traceId>' : normalize(v, k)]));
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

let compared = 0;
const differences = [];

async function compare (path, download = false) {
  const [a, b] = await Promise.all([get(args.a, path, download), get(args.b, path, download)]);
  compared++;
  if (!isDeepStrictEqual(a, b)) {
    differences.push(path);
    console.log(`DIFF  ${path}\n  a: ${JSON.stringify(a).slice(0, 2000)}\n  b: ${JSON.stringify(b).slice(0, 2000)}`);
  }
  return a;
}

const list = await compare('/artifacts/');
const products = JSON.parse(list.json).products;

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
process.exit(differences.length > 0 ? 1 : 0);
