# Release client

Administration UI of the release server: explore, download and delete releases, upload packages,
view statistics, create and restore backups. It is a single page application (Vue 3, Vite,
Tailwind CSS) served by nginx, which also forwards `/artifacts` to the release server.

## Development

Requires Node.js 22.12 or newer and a running release server (default `http://localhost:5000`).

```bash
npm install
RELEASE_SERVER_URL=http://localhost:5000 npm run dev   # http://localhost:3000
npm test                                               # unit and component tests (Vitest)
npm run lint
npm run build                                          # output in dist/
```

## Docker image

```bash
docker build -t release-client .
docker run -p 3000:3000 -e RELEASE_SERVER_URL=http://release-server:5000 release-client
```

| Variable | Description | Default |
|---|---|---|
| `RELEASE_SERVER_URL` | Address of the release server as seen from the client container | `http://localhost:5000` |

The UI requires a login with the credentials of the release server. They are kept in the session
storage of the browser tab only and are removed when the tab is closed.
