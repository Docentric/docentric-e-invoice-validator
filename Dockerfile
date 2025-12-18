# ======================================================
#  Build Stage
# ======================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy and restore
COPY src/Mustang-Project-RestServer/Mustang-Project-RestServer.csproj src/Mustang-Project-RestServer/
RUN dotnet restore src/Mustang-Project-RestServer/Mustang-Project-RestServer.csproj

# Copy full source and publish
COPY src/Mustang-Project-RestServer/ src/Mustang-Project-RestServer/
RUN dotnet publish src/Mustang-Project-RestServer/Mustang-Project-RestServer.csproj \
    -c Release -o /app/publish /p:UseAppHost=false


# ======================================================
#  Runtime Stage
# ======================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# -----------------------------
#  Environment variables
# -----------------------------
# REST API port (default 8080)
ARG RESTAPI_PORT=8080
ENV RESTAPI_PORT=${RESTAPI_PORT}
ENV ASPNETCORE_URLS=http://+:${RESTAPI_PORT}

# Mustang CLI version (default = latest stable release)
ARG MUSTANG_VERSION
# Fallback to latest known release if not specified
ENV MUSTANG_VERSION=${MUSTANG_VERSION:-2.20.0}

# -----------------------------
#  Install runtime dependencies
# -----------------------------
RUN apk add --no-cache openjdk17-jre-headless curl

# -----------------------------
#  Download Mustang CLI JAR
# -----------------------------
RUN echo "Downloading Mustang CLI version: ${MUSTANG_VERSION}" \
 && mkdir -p /opt \
 && curl -fsSL -o /opt/Mustang-CLI.jar \
      "https://www.mustangproject.org/deploy/Mustang-CLI-${MUSTANG_VERSION}.jar" \
 || { echo "⚠️ Warning: Unable to fetch specified version. Check version number or site availability."; exit 1; }

# -----------------------------
#  Copy published .NET app
# -----------------------------
COPY --from=build /app/publish ./

EXPOSE ${RESTAPI_PORT}

ENTRYPOINT ["dotnet", "Mustang-Project-RestServer.dll"]
