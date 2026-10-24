# Stage 1: lint, test and build the single page application
FROM node:24-alpine AS builder

WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run lint && npm test && npm run build

# Stage 2: serve the static files with nginx and forward /artifacts to the release server
FROM nginx:1.30-alpine AS final

# Address of the release server, e.g. http://release-server:5000 (set by the container environment)
ENV RELEASE_SERVER_URL=http://localhost:5000
# Use the DNS server of the container (Docker networks) for the per request resolution
ENV NGINX_ENTRYPOINT_LOCAL_RESOLVERS=1

RUN rm -f /etc/nginx/conf.d/default.conf
COPY nginx/default.conf.template /etc/nginx/templates/default.conf.template
COPY --chmod=755 nginx/10-release-server-url.envsh /docker-entrypoint.d/10-release-server-url.envsh
COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 3000
