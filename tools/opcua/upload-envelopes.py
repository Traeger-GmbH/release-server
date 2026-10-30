#!/usr/bin/env python3
"""
Uploads the envelopes created by build-envelopes.py to a release server and verifies every upload.

- Asks for user name and password at the start (the password without echo); nothing is stored.
- Product versions that already exist on the server are skipped (never overwritten).
- --dry-run only checks which envelopes would be uploaded (read-only requests).
- After every upload: /info must contain the release with the expected platform, file name, size and
  SHA-256, and a download must have the same SHA-256 as the source file.
- The column "size" of overview.csv is optional: if it is missing or empty, the size of the original
  file is read from the envelope.
- Stops at the first failed upload or check. Everything is logged to a file in the staging folder.

Usage:
    python3 tools/opcua/upload-envelopes.py --staging ~/Desktop/opc-ua-release-server --dry-run
    python3 tools/opcua/upload-envelopes.py --staging ~/Desktop/opc-ua-release-server
    python3 tools/opcua/upload-envelopes.py --staging ... --server http://localhost:5099   # local test stack
"""

import argparse
import base64
import csv
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


def download_sha256(url):
    digest = hashlib.sha256()
    size = 0
    with urllib.request.urlopen(url, timeout=600) as response:
        for block in iter(lambda: response.read(1024 * 1024), b''):
            digest.update(block)
            size += len(block)
    return digest.hexdigest(), size


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


def original_size(envelope_path, platform):
    """Returns the size of the original file of the platform, read from the package.json of the envelope."""
    with zipfile.ZipFile(envelope_path) as envelope:
        names = [n for n in envelope.namelist() if n == 'package.json' or n.endswith('/package.json')]
        if len(names) != 1:
            raise RuntimeError(f'envelope contains {len(names)} package.json files')
        package = json.loads(envelope.read(names[0]).decode('utf-8-sig'))
        path = package.get('deployments', {}).get(platform)
        if path is None:
            raise RuntimeError(f'package.json has no deployment for {platform}')
        # Paths in deployments are relative to the folder of package.json
        folder = names[0][:-len('package.json')]
        return envelope.getinfo(folder + path).file_size


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


def verify(server, row):
    release = find_release(server, row['product'], row['version'])
    if release is None:
        return f'/info does not contain version {row["version"]}'
    artifacts = [a for a in release.get('artifacts', []) if a['platform'] == row['platform']]
    if len(artifacts) != 1:
        return f'/info has no artifact for {row["platform"]}'
    artifact = artifacts[0]
    expected = (row['file'], int(row['size']), row['sha256'])
    actual = (artifact['fileName'], artifact['size'], artifact['sha256'])
    if actual != expected:
        return f'/info artifact differs: expected {expected}, got {actual}'
    downloaded_sha, downloaded_size = download_sha256(server + artifact['downloadUrl'])
    if (downloaded_sha, downloaded_size) != (row['sha256'], int(row['size'])):
        return f'download differs: SHA-256 {downloaded_sha}, size {downloaded_size}'
    return None


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--staging', required=True, help='staging folder of build-envelopes.py')
    parser.add_argument('--server', default=DEFAULT_SERVER, help=f'release server (default {DEFAULT_SERVER})')
    parser.add_argument('--dry-run', action='store_true', help='only show what would be uploaded')
    args = parser.parse_args()

    staging = os.path.expanduser(args.staging)
    server = args.server.rstrip('/')
    with open(os.path.join(staging, 'overview.csv'), encoding='utf-8') as stream:
        rows = list(csv.DictReader(stream))

    log = Log(os.path.join(staging, f'upload-log-{datetime.datetime.now():%Y-%m-%d_%H-%M-%S}.txt'))
    log.write(f'Server {server}, {len(rows)} envelopes, {"DRY RUN" if args.dry_run else "UPLOAD"}')

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
    for row in rows:
        name = f'{row["product"]} {row["version"]} ({row["platform"]}, {row["file"]})'
        if find_release(server, row['product'], row['version']) is not None:
            log.write(f'SKIP     {name}: version exists already')
            counts['skipped'] += 1
            continue
        if not (row.get('size') or '').strip():
            try:
                row['size'] = str(original_size(os.path.join(staging, row['envelope']), row['platform']))
            except (OSError, KeyError, ValueError, RuntimeError, zipfile.BadZipFile) as error:
                log.write(f'FAILED   {name}: size not in overview.csv and not readable from the envelope: {error}')
                sys.exit(1)
        if args.dry_run:
            log.write(f'PLANNED  {name}')
            counts['planned'] += 1
            continue

        status, body = upload(server, os.path.join(staging, row['envelope']), authorization)
        if status != 200:
            log.write(f'FAILED   {name}: upload returned {status}: {body[:500].decode(errors="replace")}')
            sys.exit(1)
        problem = verify(server, row)
        if problem:
            log.write(f'FAILED   {name}: {problem}')
            sys.exit(1)
        log.write(f'OK       {name}: uploaded, /info and download verified (SHA-256 {row["sha256"]})')
        counts['uploaded'] += 1

    log.write(f'Done: {counts["uploaded"]} uploaded, {counts["skipped"]} skipped, {counts["planned"]} planned. Log: {log.path}')


if __name__ == '__main__':
    main()
