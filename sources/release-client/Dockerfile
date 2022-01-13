# Stage 1 - build
FROM mhart/alpine-node:16 AS builder
RUN apk add --no-cache vips-dev alpine-sdk

COPY ./ /app
WORKDIR /app
RUN yarn install && \
    yarn run build

# Stage 2 - production
FROM mhart/alpine-node:16 AS final

COPY --from=builder /app /
WORKDIR /app

EXPOSE 80
CMD [ "yarn", "run", "start" ]
