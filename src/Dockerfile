# Stage 1: Build the React SPA
FROM node:20-alpine AS spa-build
WORKDIR /src/WebUI

# Update Alpine packages to latest security patches
RUN apk update && \
    apk upgrade --no-cache && \
    rm -rf /var/cache/apk/*

# Copy package files and install dependencies
COPY WebUI/package*.json ./
RUN npm ci && \
    npm cache clean --force

# Copy the rest of the SPA source code
COPY WebUI/ ./

# Build the SPA for production
RUN npm run build

# Stage 2: Build the .NET application - use latest Noble variant - Better security
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS dotnet-build
WORKDIR /src

# Copy project file and restore dependencies
COPY Server/RestServer.csproj Server/

# Pass IsDockerBuild=true so the .esproj reference is skipped
RUN dotnet restore Server/RestServer.csproj \
    /p:IsDockerBuild=true

# Copy the rest of the application source code
COPY Server/ Server/

# Copy the built SPA from the previous stage to wwwroot
COPY --from=spa-build /src/WebUI/dist /src/Server/wwwroot

# Build and publish the application (skip SPA build targets, mark Docker build)
WORKDIR /src/Server
RUN dotnet publish RestServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:RunSpaInDebug=false \
    /p:BuildProjectReferences=false \
    /p:IsDockerBuild=true

# Stage 3: Runtime image with Java and .NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

# First install security updates, then OpenJDK 17 (required for Mustang CLI) and curl (for health check)
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y --no-install-recommends \
    openjdk-17-jre-headless \
    curl \
    ca-certificates && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# OCI Image labels (standardized metadata)
ARG VERSION="25.1.0"
ARG BUILD_DATE="$(date -u +'%Y-%m-%dT%H:%M:%SZ')"
ARG VCS_REF="$(git rev-parse --short HEAD)"

LABEL org.opencontainers.image.title="Docentric Electronic Invoice Validator" \
      org.opencontainers.image.description=".NET 10 REST API wrapper for Mustang-Project CLI to validate Electronic Invoice (ZuGFeRD, Factur-X and UBL)." \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.vendor="Docentric" \
      org.opencontainers.image.url="https://hub.docker.com/r/docentric/e-invoice-validator" \
      org.opencontainers.image.source="https://github.com/docentric/docentric-e-invoice-validator" \
      org.opencontainers.image.licenses="MIT" \
      maintainer="klemen.novak@docentric.com"

# Set Java environment variables
ENV JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
ENV PATH="$JAVA_HOME/bin:$PATH"

# Create non-root user
RUN groupadd -r zugferdappuser --gid=1001 && \
    useradd -r -g zugferdappuser --uid=1001 --home-dir=/app --shell=/sbin/nologin zugferdappuser

# Create app directory
WORKDIR /app

# Copy published application from build stage
COPY --from=dotnet-build --chown=zugferdappuser:zugferdappuser /app/publish .

# Ensure wwwroot is copied (should already be there from publish)
# COPY --from=spa-build /src/WebUI/dist ./wwwroot

# Create logs directory with appropriate permissions
RUN mkdir -p /app/logs && \
    chown zugferdappuser:zugferdappuser /app/logs && \
    chmod 750 /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
# Switch to non-root user
USER zugferdappuser

# Expose HTTP
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail --silent --max-time 3 http://localhost:8080/api/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Docentric.EInvoice.Validator.RestServer.dll"]
