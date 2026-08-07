# syntax=docker/dockerfile:1

FROM node:25-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

ARG NUXT_PUBLIC_API_BASE_URL=https://api.example.com
ARG NUXT_PUBLIC_REALTIME_URL=https://api.example.com
ARG NUXT_PUBLIC_STORAGE_URI=https://storage.example.com

ENV NODE_ENV=production
ENV NUXT_PUBLIC_API_BASE_URL=$NUXT_PUBLIC_API_BASE_URL
ENV NUXT_PUBLIC_REALTIME_URL=$NUXT_PUBLIC_REALTIME_URL
ENV NUXT_PUBLIC_STORAGE_URI=$NUXT_PUBLIC_STORAGE_URI

RUN npm run generate

FROM alpine:3.20 AS export
WORKDIR /srv
COPY --from=build /app/.output/public/ /srv/
