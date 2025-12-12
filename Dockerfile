# Stage 1: Build (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Install build dependencies
RUN apk add --no-cache icu-libs krb5-libs libgcc libssl3 libstdc++ zlib postgresql-client

# Copy csproj and restore (cached layer)
COPY ExamApp.csproj ./
RUN dotnet restore ExamApp.csproj

# Copy source and build
COPY . .
WORKDIR /src
RUN dotnet publish ExamApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Install runtime dependencies
RUN apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libssl3 \
    libstdc++ \
    zlib \
    postgresql-client \
    curl \
    jq

# Copy built app
COPY --from=build /app/publish .

# Create appsettings.json from APP_SETTINGS_JSON environment variable
RUN echo "\$APP_SETTINGS_JSON" > /app/appsettings.json
RUN chmod 644 /app/appsettings.json

# ENTRYPOINT script that generates appsettings.json from env var
COPY <<EOF /app/entrypoint.sh
#!/bin/sh
set -e

# Generate appsettings.json from APP_SETTINGS_JSON environment variable
if [ -n "\$APP_SETTINGS_JSON" ]; then
    echo "\$APP_SETTINGS_JSON" > /app/appsettings.json
    echo "Loaded appsettings.json from APP_SETTINGS_JSON (\$(( \$(echo \$APP_SETTINGS_JSON | wc -c ) - 1 )) bytes)"
fi

# Run the app
exec dotnet ExamApp.dll "\$@"
EOF

RUN chmod +x /app/entrypoint.sh

# Create non-root user
RUN addgroup -g 1000 -S appgroup && \
    adduser -u 1000 -S appuser -G appgroup && \
    chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=30s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/health || exit 1

ENTRYPOINT ["/app/entrypoint.sh"]
