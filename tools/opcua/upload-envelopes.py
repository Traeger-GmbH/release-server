#!/usr/bin/env python3
"""
Uploads the envelopes of a staging folder to a release server and verifies every upload.

- Finds the envelopes itself: every .zip file in the staging folder and its subfolders. Product,
  version and platforms are read from the package.json of each envelope; an overview.csv is not
  needed (and ignored if present). Size and SHA-256 of the original files are calculated from the
  envelope.
- Stops before the first upload if a .zip file is not a valid envelope or a product version occurs
  twice.
- Asks for user name and password at the start (the password without echo); nothing is stored.
- Product versions that already exist on the server are skipped (never overwritten).
- --dry-run only checks which envelopes would be uploaded (read-only requests).
- After every upload: /info must contain the release with the expected platforms, file names, sizes
  and SHA-256, and the download of every platform must have the same SHA-256 as the original file.
- Stops at the first failed upload or check. Everything is logged to a file in the staging folder.

Usage:
    python3 tools/opcua/upload-envelopes.py --staging ~/Desktop/opc-ua-release-server --dry-run
    python3 tools/opcua/upload-envelopes.py --staging ~/Desktop/opc-ua-release-server
    python3 tools/opcua/upload-envelopes.py --staging ... --server http://localhost:5099   # local test stack
"""

import argparse
import base64
import datetime
import getpass
import hashlib
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
import uuid
import zipfile

DEFAULT_SERVER = 'https://releases.traeger.de'


class Log:
    def __init__(self, path):
        self.stream = open(path, 'a', encoding='utf-8')
        self.path = path

    def write(self, message):
        line = f'{datetime.datetime.now().isoformat(timespec="seconds")} {message}'
        print(line)
        self.stream.write(line + '\n')
        self.stream.flush()


def request(url, method='GET', body=None, headers=None):
    req = urllib.request.Request(url, data=body, method=method, headers=headers or {})
    try:
        with urllib.request.urlopen(req, timeout=600) as response:
            return response.status, response.read()
    except urllib.error.HTTPError as error:
        return error.code, error.read()


def sha256_of_stream(stream):
    digest = hashlib.sha256()
    size = 0
    for block in iter(lambda: stream.read(1024 * 1024), b''):
        digest.update(block)
        size += len(block)
    return digest.hexdigest(), size


def download_sha256(url):
    with urllib.request.urlopen(url, timeout=600) as response:
        return sha256_of_stream(response)


def product_url(server, product, *segments):
    parts = [urllib.parse.quote(product, safe='')] + [urllib.parse.quote(s, safe='') for s in segments]
    return f'{server}/artifacts/' + '/'.join(parts)


def find_release(server, product, version):
    """Returns the release from /info or None if the product version does not exist."""
    status, body = request(product_url(server, product, 'info') + '?' + urllib.parse.urlencode({'version': version}))
    if status == 404:
        return None
    if status != 200:
        raise RuntimeError(f'/info returned {status}: {body[:300]!r}')
    releases = json.loads(body)['releases']
    return releases[0] if releases else None


def read_envelope(path):
    """Reads product, version and the original files (platform, file name, size, SHA-256) of an envelope."""
    with zipfile.ZipFile(path) as envelope:
        names = [n for n in envelope.namelist() if n == 'package.json' or n.endswith('/package.json')]
        if len(names) != 1:
            raise ValueError(f'contains {len(names)} package.json files instead of 1')
        package = json.loads(envelope.read(names[0]).decode('utf-8-sig'))
        product, version, deployments = package.get('identifier'), package.get('version'), package.get('deployments')
        if not product or not version or not deployments:
            raise ValueError('package.json has no identifier, version or deployments')
        # Paths in deployments are relative to the folder of package.json
        folder = names[0][:-len('package.json')]
        artifacts = []
        for platform, file_path in sorted(deployments.items()):
            with envelope.open(folder + file_path) as stream:
                sha256, size = sha256_of_stream(stream)
            artifacts.append({'platform': platform, 'file': file_path.rsplit('/', 1)[-1], 'size': size, 'sha256': sha256})
    return {'product': product, 'version': version, 'path': path, 'artifacts': artifacts}


def find_envelopes(staging):
    """Returns all envelopes of the staging folder and the problems (invalid envelopes, duplicates)."""
    envelopes, problems = [], []
    for root, folders, files in os.walk(staging):
        folders[:] = sorted(f for f in folders if not f.startswith('.'))
        for name in sorted(files):
            if name.lower().endswith('.zip') and not name.startswith('.'):
                path = os.path.join(root, name)
                try:
                    envelopes.append(read_envelope(path))
                except (OSError, KeyError, ValueError, zipfile.BadZipFile) as error:
                    problems.append(f'{os.path.relpath(path, staging)}: no valid envelope ({error})')
    seen = {}
    for envelope in envelopes:
        key = (envelope['product'], envelope['version'])
        if key in seen:
            problems.append(f'{envelope["product"]} {envelope["version"]} occurs twice: '
                            f'{os.path.relpath(seen[key], staging)}, {os.path.relpath(envelope["path"], staging)}')
        seen[key] = envelope['path']
    return envelopes, problems


def upload(server, envelope_path, authorization):
    boundary = uuid.uuid4().hex
    with open(envelope_path, 'rb') as stream:
        content = stream.read()
    body = (f'--{boundary}\r\n'
            f'Content-Disposition: form-data; name="package"; filename="{os.path.basename(envelope_path)}"\r\n'
            'Content-Type: application/zip\r\n\r\n').encode() + content + f'\r\n--{boundary}--\r\n'.encode()
    headers = {'Authorization': authorization, 'Content-Type': f'multipart/form-data; boundary={boundary}'}
    # force=false: an existing version is never overwritten
    return request(f'{server}/artifacts/?force=false', method='PUT', body=body, headers=headers)


def verify(server, envelope):
    release = find_release(server, envelope['product'], envelope['version'])
    if release is None:
        return f'/info does not contain version {envelope["version"]}'
    for expected in envelope['artifacts']:
        artifacts = [a for a in release.get('artifacts', []) if a['platform'] == expected['platform']]
        if len(artifacts) != 1:
            return f'/info has no artifact for {expected["platform"]}'
        artifact = artifacts[0]
        wanted = (expected['file'], expected['size'], expected['sha256'])
        actual = (artifact['fileName'], artifact['size'], artifact['sha256'])
        if actual != wanted:
            return f'/info artifact of {expected["platform"]} differs: expected {wanted}, got {actual}'
        downloaded = download_sha256(server + artifact['downloadUrl'])
        if downloaded != (expected['sha256'], expected['size']):
            return f'download of {expected["platform"]} differs: SHA-256 {downloaded[0]}, size {downloaded[1]}'
    return None


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--staging', required=True, help='folder with the envelopes (searched recursively)')
    parser.add_argument('--server', default=DEFAULT_SERVER, help=f'release server (default {DEFAULT_SERVER})')
    parser.add_argument('--dry-run', action='store_true', help='only show what would be uploaded')
    args = parser.parse_args()

    staging = os.path.expanduser(args.staging)
    server = args.server.rstrip('/')
    envelopes, problems = find_envelopes(staging)

    log = Log(os.path.join(staging, f'upload-log-{datetime.datetime.now():%Y-%m-%d_%H-%M-%S}.txt'))
    log.write(f'Server {server}, {len(envelopes)} envelopes, {"DRY RUN" if args.dry_run else "UPLOAD"}')
    if problems or not envelopes:
        for problem in problems or ['no envelopes found']:
            log.write(f'FAILED   {problem}')
        log.write('Nothing uploaded.')
        sys.exit(1)

    authorization = None
    if not args.dry_run:
        username = input('User name: ')
        password = getpass.getpass('Password: ')
        authorization = 'Basic ' + base64.b64encode(f'{username}:{password}'.encode()).decode()
        status, _ = request(f'{server}/artifacts/statistics', headers={'Authorization': authorization})
        if status != 200:
            log.write(f'Login failed ({status}), nothing uploaded.')
            sys.exit(1)

    counts = {'uploaded': 0, 'skipped': 0, 'planned': 0}
    for envelope in envelopes:
        files = ', '.join(f'{a["platform"]} {a["file"]}' for a in envelope['artifacts'])
        name = f'{envelope["product"]} {envelope["version"]} ({files})'
        if find_release(server, envelope['product'], envelope['version']) is not None:
            log.write(f'SKIP     {name}: version exists already')
            counts['skipped'] += 1
            continue
        if args.dry_run:
            log.write(f'PLANNED  {name}')
            counts['planned'] += 1
            continue

        status, body = upload(server, envelope['path'], authorization)
        if status != 200:
            log.write(f'FAILED   {name}: upload returned {status}: {body[:500].decode(errors="replace")}')
            sys.exit(1)
        problem = verify(server, envelope)
        if problem:
            log.write(f'FAILED   {name}: {problem}')
            sys.exit(1)
        checksums = ', '.join(a['sha256'] for a in envelope['artifacts'])
        log.write(f'OK       {name}: uploaded, /info and download verified (SHA-256 {checksums})')
        counts['uploaded'] += 1

    log.write(f'Done: {counts["uploaded"]} uploaded, {counts["skipped"]} skipped, {counts["planned"]} planned. Log: {log.path}')


if __name__ == '__main__':
    main()
