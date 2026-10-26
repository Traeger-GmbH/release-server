#!/usr/bin/env python3
"""
Builds upload envelopes for the release server from the download folder of an SDK (former FTP folder of
the documentation): OPC UA SDK (--sdk opcua, default) or S7 SDK / TLX-S LINK .NET (--sdk s7). The source
folder is only read.

Every envelope is a package upload for PUT /artifacts: a ZIP file with
    <Product>-v<version>/package.json
    <Product>-v<version>/files/<original file name>   (the original file, byte-identical)

Release notes are taken only from the Version.txt in the ZIP of the same version (English; "de" stays
empty). Lines that cannot be assigned clearly (NEW -> added, FIXED -> fixed, CHANGED -> breaking) are not
guessed but listed in unassigned-notes.md.

Usage:
    python3 tools/opcua/build-envelopes.py --source ~/Desktop/opc-ua/net --output ~/Desktop/opc-ua-release-server
    python3 tools/opcua/build-envelopes.py --sdk s7 --source ~/Desktop/releases/s7/net --output ~/Desktop/s7-release-server
    python3 tools/opcua/build-envelopes.py ... --all-patches     # all patches instead of the latest per minor version

Output: one envelope per product and version, overview.csv / overview.md and unassigned-notes.md.
"""

import argparse
import csv
import gzip
import hashlib
import json
import os
import re
import sys
import tarfile
import zipfile

DATE = r'(\d{4}-\d{2}-\d{2})'
FILE_VERSION = r'(\d+\.\d+\.\d+\.\d+(?:-[0-9A-Za-z.]+)?)'

# Product IDs, platforms and file name patterns per SDK (decision of the management, see the release server
# docs). "line" groups the ZIP and NuGet products of the same SDK, so the NuGet package gets the release notes
# of the ZIP with the same version. "policy" selects the versions, "notes" says whether the ZIP of the product
# (or of its line for NuGet packages) contains a Version.txt with release notes.
OPCUA_PRODUCTS = [
    {'id': 'Opc.UaFx.Advanced', 'platform': 'any/any', 'line': 'advanced', 'kind': 'zip', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^opc\.uafx\.advanced\.{DATE}-{FILE_VERSION}\.zip$'},
    {'id': 'Opc.UaFx.Advanced.nupkg', 'platform': 'any/any', 'line': 'advanced', 'kind': 'nupkg', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^opc\.uafx\.advanced\.[0-9A-Za-z.\-]+?\.{DATE}-{FILE_VERSION}\.nupkg$'},
    {'id': 'Opc.UaFx.Client', 'platform': 'any/any', 'line': 'client', 'kind': 'zip', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^opc\.uafx\.client\.{DATE}-{FILE_VERSION}\.zip$'},
    {'id': 'Opc.UaFx.Client.nupkg', 'platform': 'any/any', 'line': 'client', 'kind': 'nupkg', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^opc\.uafx\.client\.[0-9A-Za-z.\-]+?\.{DATE}-{FILE_VERSION}\.nupkg$'},
    {'id': 'Opc.UaFx.Client.Unity', 'platform': 'any/any', 'line': 'unity', 'kind': 'unitypackage', 'policy': 'all', 'notes': False,
     'pattern': rf'^opc\.uafx\.client\.[0-9.]+\.{DATE}-{FILE_VERSION}\.unitypackage$'},
    {'id': 'Opc.UaFx.Client.LabVIEW', 'platform': 'windows/any', 'line': 'labview', 'kind': 'zip', 'policy': 'all', 'notes': True,
     'pattern': rf'^opc\.uafx\.client\.labview\.{DATE}-{FILE_VERSION}\.zip$'},
    {'id': 'OpcWatch', 'platform': 'windows/any', 'line': 'opcwatch', 'kind': 'zip', 'policy': 'sdk', 'notes': False,
     'pattern': rf'^opcwatch\.{DATE}-{FILE_VERSION}\.zip$'},
    {'id': 'opc-core-components-redistributables', 'platform': 'windows/any', 'line': 'core', 'kind': 'zip', 'policy': 'all', 'notes': False,
     'pattern': r'^opc-core-components-redistributables-(\d+\.\d+\.\d+)-(\d{8})\.zip$'},
]

S7_PRODUCTS = [
    {'id': 'IPS7LnkNet.Advanced', 'platform': 'any/any', 'line': 'advanced', 'kind': 'zip', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^ips7lnknet\.advanced\.{DATE}-{FILE_VERSION}\.zip$'},
    {'id': 'IPS7LnkNet.Advanced.nupkg', 'platform': 'any/any', 'line': 'advanced', 'kind': 'nupkg', 'policy': 'sdk', 'notes': True,
     'pattern': rf'^ips7lnknet\.advanced\.[0-9A-Za-z.\-]+?\.{DATE}-{FILE_VERSION}\.nupkg$'},
    {'id': 'S7Watch', 'platform': 'windows/any', 'line': 's7watch', 'kind': 'zip', 'policy': 'sdk', 'notes': False,
     'pattern': rf'^s7watch\.{DATE}-{FILE_VERSION}\.zip$'},
]

# "minimum": lowest minor version of the "sdk" policy (None: all versions). "legacy_folders": sub folders with
# legacy SDKs, of which only the "legacy_versions" (per line) are uploaded, here the .NET 3.5 SDK (net35).
SDKS = {
    'opcua': {'products': OPCUA_PRODUCTS, 'minimum': (2, 50), 'legacy_folders': ['net35'],
              'legacy_versions': {'advanced': ['2.0.1.1']}},
    's7': {'products': S7_PRODUCTS, 'minimum': None, 'legacy_folders': [], 'legacy_versions': {}},
}

ASSIGNMENT = {'NEW': 'added', 'FIXED': 'fixed', 'CHANGED': 'breaking'}

MODERN_HEADER = re.compile(r'^= (\d+\.\d+\.\d+\.\d+(?:-[0-9A-Za-z.]+)?) \(released ([^)]*)\) =\s*$')
# The closing " =" is missing in some files (e.g. S7 SDK 1.9.1.1: "= 13.09.2019 - V1.9.1.1")
LEGACY_HEADER = re.compile(r'^= (\d{2}\.\d{2}\.\d{4}) - V(\d+\.\d+\.\d+\.\d+)(?: =)?\s*$')
MODERN_ENTRY = re.compile(r'^([A-Z][A-Z ]*[A-Z]):\s*(.*)$')
LEGACY_ENTRY = re.compile(r'^\* (New|Fixed|Changed|Renamed|Moved|Removed|Added)\s+(.*)$')
LEGACY_ASSIGNMENT = {'New': 'added', 'Fixed': 'fixed', 'Changed': 'breaking'}


def version_tuple(version):
    numbers = version.split('-')[0].split('.')
    return tuple(int(number) for number in numbers)


def server_version(file_version):
    """2.57.1.0 -> 2.57.1, 2.51.4.1 -> 2.51.4.1 (like the NuGet version in the .nuspec)."""
    parts = file_version.split('.')
    if len(parts) == 4 and parts[3] == '0':
        parts = parts[:3]
    return '.'.join(parts)


def sha256(path):
    digest = hashlib.sha256()
    with open(path, 'rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    return digest.hexdigest()


def check_archive(path, kind):
    """Returns an error text if the file is damaged, otherwise None."""
    try:
        if kind in ('zip', 'nupkg'):
            with zipfile.ZipFile(path) as archive:
                broken = archive.testzip()
                return f'damaged entry {broken}' if broken else None
        if kind == 'unitypackage':
            with tarfile.open(path, 'r:gz') as archive:
                for _ in archive:
                    pass
            return None
    except (zipfile.BadZipFile, tarfile.TarError, gzip.BadGzipFile, OSError, EOFError) as error:
        return str(error)
    return None


def find_files(source, sdk):
    found = []
    for directory in [source] + [os.path.join(source, folder) for folder in sdk['legacy_folders']]:
        if not os.path.isdir(directory):
            continue
        for name in sorted(os.listdir(directory)):
            path = os.path.join(directory, name)
            if not os.path.isfile(path):
                continue
            for product in sdk['products']:
                match = re.match(product['pattern'], name, re.IGNORECASE)
                if not match:
                    continue
                if product['line'] == 'core':
                    version, date = match.group(1), match.group(2)
                    date = f'{date[0:4]}-{date[4:6]}-{date[6:8]}'
                    file_version = version
                else:
                    date, file_version = match.group(1), match.group(2)
                found.append({'product': product, 'path': path, 'name': name, 'date': date,
                              'file_version': file_version, 'legacy': directory != source})
                break
    return found


def select(files, sdk, all_patches):
    selected, skipped = [], []
    for product in sdk['products']:
        candidates = [f for f in files if f['product'] is product]
        releases = [f for f in candidates if '-' not in f['file_version']]
        skipped += [(f, 'preview') for f in candidates if '-' in f['file_version']]

        if product['policy'] == 'all':
            selected += releases
            continue

        legacy = sdk['legacy_versions'].get(product['line'], [])
        minimum = sdk['minimum'] or (0, 0)
        chosen = [f for f in releases if f['legacy'] and server_version(f['file_version']) in legacy]
        modern = [f for f in releases if not f['legacy'] and version_tuple(f['file_version'])[:2] >= minimum]
        if all_patches:
            chosen += modern
        else:
            by_minor = {}
            for f in modern:
                key = version_tuple(f['file_version'])[:2]
                if key not in by_minor or version_tuple(f['file_version']) > version_tuple(by_minor[key]['file_version']):
                    by_minor[key] = f
            chosen += by_minor.values()
        skipped += [(f, 'not selected') for f in releases if f not in chosen]
        selected += chosen
    return selected, skipped


def read_version_txt(zip_path):
    with zipfile.ZipFile(zip_path) as archive:
        names = [name for name in archive.namelist() if name.lower() == 'version.txt']
        if not names:
            return None
        return archive.read(names[0]).decode('utf-8-sig', errors='replace')


def parse_notes(text, file_version, context, unassigned):
    """Returns (changes, info) for the section of the version, assigning only unambiguous lines."""
    lines = text.splitlines()
    wanted = version_tuple(file_version)
    section, legacy = None, False
    for index, line in enumerate(lines):
        modern = MODERN_HEADER.match(line)
        old = LEGACY_HEADER.match(line)
        if modern and version_tuple(modern.group(1)) == wanted and '-' not in modern.group(1):
            section, legacy = index, False
            break
        if old and version_tuple(old.group(2)) == wanted:
            section, legacy = index, True
            break
    if section is None:
        return None, 'no section for this version in Version.txt'

    changes = {'added': [], 'fixed': [], 'breaking': [], 'deprecated': []}
    last = None
    for line in lines[section + 1:]:
        if line.startswith('= '):
            break
        if not line.strip():
            continue
        if line[:1] in (' ', '\t'):
            # Continuation of the previous entry (wrapped text); listed in the report for review
            if last is not None:
                last[0][last[1]] += ' ' + line.strip()
                unassigned.append((context, line.rstrip(), 'continuation, joined with the previous entry'))
            else:
                unassigned.append((context, line.rstrip(), 'continuation without entry'))
            continue
        match = (LEGACY_ENTRY if legacy else MODERN_ENTRY).match(line)
        target = None
        if match:
            target = (LEGACY_ASSIGNMENT if legacy else ASSIGNMENT).get(match.group(1))
        if target:
            changes[target].append(match.group(2).strip())
            last = (changes[target], len(changes[target]) - 1)
        else:
            unassigned.append((context, line.rstrip(), 'prefix not assigned'))
            last = None
    info = 'Version.txt (legacy format: "* New/Fixed/Changed")' if legacy else 'Version.txt'
    return changes, info


def build_release_notes(item, zips_by_line_version, unassigned):
    product = item['product']
    context = f"{product['id']} {server_version(item['file_version'])}"
    source_zip = None
    if product['kind'] == 'zip' and product['notes']:
        source_zip = item['path']
    elif product['kind'] == 'nupkg' and product['notes']:
        source_zip = zips_by_line_version.get((product['line'], version_tuple(item['file_version'])))

    changes, info = None, 'no Version.txt'
    if source_zip:
        text = read_version_txt(source_zip)
        if text is not None:
            changes, info = parse_notes(text, item['file_version'], context, unassigned)
            if source_zip != item['path']:
                info += f' of {os.path.basename(source_zip)}'
    elif product['kind'] == 'nupkg':
        info = 'no ZIP of the same version'

    entries = [changes] if changes and any(changes.values()) else []
    notes = {
        'releaseDate': item['date'],
        'isPreviewRelease': False,
        'isSecurityPatch': False,
        'changes': {'de': [], 'en': entries},
    }
    counts = {key: len(value) for key, value in (changes or {}).items()}
    return notes, info, counts


def write_envelope(output, item, notes):
    product = item['product']
    version = server_version(item['file_version'])
    root = f"{product['id']}-v{version}"
    package = {
        'identifier': product['id'],
        'version': version,
        'releaseNotes': notes,
        'deployments': {product['platform']: f"files/{item['name']}"},
    }
    directory = os.path.join(output, product['id'])
    os.makedirs(directory, exist_ok=True)
    envelope = os.path.join(directory, f'{product["id"]}-{version}.zip')
    with zipfile.ZipFile(envelope, 'w') as archive:
        archive.writestr(f'{root}/package.json', json.dumps(package, indent=2, ensure_ascii=False), zipfile.ZIP_DEFLATED)
        # The original file is stored without compression, the download is byte-identical
        archive.write(item['path'], f'{root}/files/{item["name"]}', zipfile.ZIP_STORED)
    return envelope


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--sdk', choices=sorted(SDKS), default='opcua', help='SDK of the download folder (default opcua)')
    parser.add_argument('--source', required=True, help='download folder (only read)')
    parser.add_argument('--output', required=True, help='new or empty staging folder outside of the repository')
    parser.add_argument('--all-patches', action='store_true', help='all patches instead of the latest per minor version (OPC UA: from 2.50)')
    args = parser.parse_args()

    source = os.path.expanduser(args.source)
    output = os.path.expanduser(args.output)
    if os.path.exists(output) and os.listdir(output):
        sys.exit(f'The output folder {output} is not empty.')
    os.makedirs(output, exist_ok=True)

    sdk = SDKS[args.sdk]
    files = find_files(source, sdk)
    selected, skipped = select(files, sdk, args.all_patches)
    zips_by_line_version = {(f['product']['line'], version_tuple(f['file_version'])): f['path']
                            for f in files if f['product']['kind'] == 'zip'}

    unassigned, rows, damaged = [], [], []
    for item in sorted(selected, key=lambda f: (f['product']['id'], version_tuple(f['file_version']))):
        problem = check_archive(item['path'], item['product']['kind'])
        if problem:
            damaged.append((item['name'], problem))
            continue
        notes, info, counts = build_release_notes(item, zips_by_line_version, unassigned)
        envelope = write_envelope(output, item, notes)
        rows.append({
            'product': item['product']['id'],
            'version': server_version(item['file_version']),
            'platform': item['product']['platform'],
            'date': item['date'],
            'file': item['name'],
            'size': os.path.getsize(item['path']),
            'sha256': sha256(item['path']),
            'added': counts.get('added', 0),
            'fixed': counts.get('fixed', 0),
            'breaking': counts.get('breaking', 0),
            'notes_source': info,
            'envelope': os.path.relpath(envelope, output),
        })

    with open(os.path.join(output, 'overview.csv'), 'w', newline='', encoding='utf-8') as stream:
        writer = csv.DictWriter(stream, fieldnames=list(rows[0].keys()))
        writer.writeheader()
        writer.writerows(rows)

    with open(os.path.join(output, 'overview.md'), 'w', encoding='utf-8') as stream:
        stream.write(f'# Upload overview ({len(rows)} envelopes, {sum(r["size"] for r in rows) / 1024 / 1024:.1f} MB)\n\n')
        stream.write('| Product | Version | Platform | Date | File | Size | SHA-256 | added/fixed/breaking | Notes |\n|---|---|---|---|---|---|---|---|---|\n')
        for r in rows:
            stream.write(f"| {r['product']} | {r['version']} | {r['platform']} | {r['date']} | {r['file']} | {r['size']} | {r['sha256']} | {r['added']}/{r['fixed']}/{r['breaking']} | {r['notes_source']} |\n")
        stream.write('\n## Not selected\n\n')
        for f, reason in sorted(skipped, key=lambda s: s[0]['name'].lower()):
            stream.write(f"- {f['name']} ({reason})\n")
        stream.write('\n## Damaged source files\n\n' + ('\n'.join(f'- {n}: {p}' for n, p in damaged) or '- none') + '\n')

    with open(os.path.join(output, 'unassigned-notes.md'), 'w', encoding='utf-8') as stream:
        stream.write(f'# Release note lines that were not assigned or were joined ({len(unassigned)})\n\n')
        stream.write('Assigned are only NEW -> added, FIXED -> fixed, CHANGED -> breaking (legacy: * New/Fixed/Changed).\n\n')
        for context, line, reason in unassigned:
            stream.write(f'- **{context}** ({reason}): `{line}`\n')

    print(f'{len(rows)} envelopes, {len(unassigned)} unassigned note lines, {len(damaged)} damaged files -> {output}')


if __name__ == '__main__':
    main()
