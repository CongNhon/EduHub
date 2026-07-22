# syntax=docker/dockerfile:1.7
FROM node:24-alpine AS base

ENV PNPM_HOME=/pnpm
ENV PATH=$PNPM_HOME:$PATH
RUN corepack enable && corepack prepare pnpm@11.13.0 --activate
WORKDIR /workspace

FROM base AS dependencies

COPY package.json pnpm-lock.yaml pnpm-workspace.yaml tsconfig.base.json ./
COPY apps/site/package.json apps/site/package.json
COPY apps/portal/package.json apps/portal/package.json
COPY packages/api-client/package.json packages/api-client/package.json
COPY packages/auth/package.json packages/auth/package.json
COPY packages/ui/package.json packages/ui/package.json
COPY packages/validation/package.json packages/validation/package.json
RUN pnpm install --frozen-lockfile

FROM dependencies AS build

ARG APP
ARG EDUHUB_API_URL=http://eduhub-api:8080
ARG NEXT_PUBLIC_SITE_URL=http://localhost:3000
ARG NEXT_PUBLIC_PORTAL_URL=http://localhost:3001
ARG NEXT_PUBLIC_SIGNALR_URL=/hubs/notifications
ENV EDUHUB_API_URL=$EDUHUB_API_URL
ENV NEXT_PUBLIC_SITE_URL=$NEXT_PUBLIC_SITE_URL
ENV NEXT_PUBLIC_PORTAL_URL=$NEXT_PUBLIC_PORTAL_URL
ENV NEXT_PUBLIC_SIGNALR_URL=$NEXT_PUBLIC_SIGNALR_URL

COPY . .
RUN test "$APP" = "site" -o "$APP" = "portal"
RUN --mount=type=secret,id=devexpress_license \
    if [ "$APP" = "portal" ]; then \
      test -s /run/secrets/devexpress_license; \
      export DevExpress_License="$(cat /run/secrets/devexpress_license)"; \
    fi; \
    pnpm --filter "@eduhub/$APP" build
RUN mkdir -p /standalone/apps/$APP/.next \
    && cp -R apps/$APP/.next/standalone/. /standalone/ \
    && cp -R apps/$APP/.next/static /standalone/apps/$APP/.next/static

FROM node:24-alpine AS runtime

ARG APP
ENV APP=$APP
ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000
ENV NEXT_TELEMETRY_DISABLED=1
WORKDIR /app

COPY --from=build --chown=node:node /standalone ./

USER node
EXPOSE 3000
CMD ["sh", "-c", "exec node apps/${APP}/server.js"]
