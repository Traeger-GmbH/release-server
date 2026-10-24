# OPC UA SDK downloads: envelopes and upload

Moves the OPC UA SDK downloads of the former documentation download folder to the release server.
Nothing of the data is stored in this repository.

1. `build-envelopes.py` reads the download folder (read-only) and writes one envelope per product and
   version into a new staging folder, together with `overview.csv` / `overview.md` (product, version,
   platform, date, file, size, SHA-256, number of release note entries) and `unassigned-notes.md`.
   The original files are stored byte-identical; release notes come only from the Version.txt of the ZIP
   of the same version (NEW -> added, FIXED -> fixed, CHANGED -> breaking).
2. `upload-envelopes.py` uploads the envelopes as packages (`PUT /artifacts`), skips product versions
   that already exist, and verifies `/info` and the SHA-256 of every download. It asks for the
   credentials (password without echo) and writes a log file into the staging folder.

```bash
python3 tools/opcua/build-envelopes.py --source <download folder> --output <new staging folder> [--all-patches]
python3 tools/opcua/upload-envelopes.py --staging <staging folder> --dry-run
python3 tools/opcua/upload-envelopes.py --staging <staging folder> [--server <url>]
```

Requires Python 3.9 or newer (standard library only). The release server must be version 1.1 or newer,
so the checksums are created during the upload.
