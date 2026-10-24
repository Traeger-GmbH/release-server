![Build Status](https://github.com/Traeger-GmbH/release-server/workflows/build/badge.svg?branch=master)

# Release server

A server for publishing your own release artifacts. It provides REST endpoints to

- list products and their releases (version, release date, platforms, change log),
- download releases (a specific version or the latest one per platform),
- upload, delete, back up and restore releases (authenticated).

The repository contains the server (`sources/ReleaseServer`, ASP.NET Core / .NET 10) and an
administration UI (`sources/release-client`, Vue 3, served by nginx).

The API is documented with Swagger: `http://<server>:5000/swagger` (the document is also available
in [docs/api/swagger.json](docs/api/swagger.json)).

## Setup with Docker Compose

Use the [docker-compose.yml](docker-compose.yml) of this repository as a template:

1. Set your own `CREDENTIALS_USERNAME` and `CREDENTIALS_PASSWORD`.
2. Start the stack: `docker compose up -d`

The server is reachable at http://localhost:5000 (Swagger UI at http://localhost:5000/swagger), the
administration UI at http://localhost:3000.

### Images

| Image | Description |
|---|---|
| `ghcr.io/traeger-gmbh/release-server:<version>` | Release server, listens on port 5000 |
| `ghcr.io/traeger-gmbh/release-client:<version>` | Administration UI, listens on port 3000 |

### Data

The server stores all data relative to its working directory `/app`:

| Path in the container | Content |
|---|---|
| `/app/artifacts` | All products and releases. **Mount a volume, otherwise the data is lost when the container is recreated.** |
| `/app/backups` | Backups created with `GET /artifacts/backup` (the newest ones are kept). |

Layout of the artifact directory (unchanged since the first version):

```
artifacts/<product>/<os>/<architecture>/<version>/
    deployment.json      { "ArtifactFileName": "...", "ReleaseNotesFileName": "releaseNotes.json" }
    releaseNotes.json    release date, preview and security flags, change log per language
    <artifact file>
```

The server runs as `root` in the container, like previous versions, so existing volumes stay
writable.

### Configuration

| Setting | Description | Default |
|---|---|---|
| `CREDENTIALS_USERNAME`, `CREDENTIALS_PASSWORD` | Credentials for all authenticated endpoints (HTTP Basic). Without them, all write access is denied. | none |
| `ArtifactRootDirectory` | Directory of the artifacts | `./artifacts` |
| `BackupRootDirectory` | Directory of the backups | `./backups` |
| `BackupRetentionCount` | Number of backups kept in the backup directory | `5` |
| `Cors__WriteOrigins__0`, `__1`, ... | Origins that may call the authenticated endpoints from a browser (CORS). The public endpoints can be read by any website. | none |

The settings are read from `/app/appsettings.json` and can be overridden by environment variables.

Client:

| Setting | Description | Default |
|---|---|---|
| `RELEASE_SERVER_URL` | Address of the release server as seen from the client container | `http://localhost:5000` |

## Usage

The public endpoints do not need authentication:

```bash
# List all products
curl http://localhost:5000/artifacts

# List the releases of a product (newest first; filter, sort and paging see Swagger)
curl "http://localhost:5000/artifacts/<product>/info"
curl "http://localhost:5000/artifacts/<product>/info?operatingSystems=windows&architectures=x64"

# Download a specific release or the latest release for a platform
curl -OJ http://localhost:5000/artifacts/<product>/<os>/<architecture>/<version>
curl -OJ http://localhost:5000/artifacts/<product>/<os>/<architecture>/latest
```

Example response of `GET /artifacts/<product>/info`:

```json
{
  "identifier": "example-product",
  "releases": [
    {
      "version": "1.2.0",
      "platforms": [ "windows/x64", "linux/arm64" ],
      "releaseDate": "2026-01-31T00:00:00",
      "isPreviewRelease": false,
      "isSecurityPatch": false,
      "changes": {
        "en": [ { "platforms": null, "added": [ "New feature" ], "fixed": [], "breaking": [], "deprecated": [] } ]
      }
    }
  ],
  "totalReleaseCount": 1,
  "nextOffset": null
}
```

### Uploading a package (several platforms)

A package is a ZIP file containing a folder with a `package.json` and the files of all platforms. See
the [example package](examples/package-foobar-v2.0.0-preview-2).

```bash
curl -X PUT http://localhost:5000/artifacts -u '<user>:<password>' \
  -F 'package=@<path/to/package.zip>;type=application/zip'
```

If the version already exists, the upload is rejected; add `?force=true` to replace it. You can also
use [tools/uploadPackage.sh](tools/uploadPackage.sh).

### Uploading a single artifact (one platform)

The ZIP file contains `deployment.json`, the release notes and the artifact. See the
[example artifact](examples/artifact-foobar-v2.0.0-preview-2_windows-x64).

```bash
curl -X PUT http://localhost:5000/artifacts/<product>/<os>/<architecture>/<version> -u '<user>:<password>' \
  -F 'artifact=@<path/to/artifact.zip>;type=application/zip'
```

Names of products, operating systems and architectures may contain letters, digits, `-`, `_` and `.`.
Use `any` for platform independent files (e.g. `any/any`).

### Backup and restore

```bash
curl -OJ -u '<user>:<password>' http://localhost:5000/artifacts/backup
curl -X PUT -u '<user>:<password>' -F 'backupFile=@backup.zip' http://localhost:5000/artifacts/restore
```

A restore verifies the backup before it replaces any data. If the backup is invalid, the existing data
stays unchanged and the response describes the problems.

## Development

```bash
# Server and tests (.NET 10 SDK)
cd sources/ReleaseServer && dotnet test

# Client (Node.js 22.12 or newer), see sources/release-client/README.md
cd sources/release-client && npm install && npm test

# End-to-end checks with Docker: compatibility with a previous image and the complete stack
docker build -t release-server:local . && docker build -t release-client:local sources/release-client
node tools/compat/compat-check.mjs --old ghcr.io/traeger-gmbh/release-server:<previous version> --new release-server:local
node tools/compat/stack-check.mjs --server release-server:local --client release-client:local
```

The integration tests pin the public API with golden files (`Integration/Golden`) and with a snapshot
of the production API (`Integration/LiveSnapshot`). Changing them requires a deliberate decision,
because customers and websites use the API. Record changed golden files with
`RELEASE_SERVER_UPDATE_GOLDEN=1 dotnet test`.
