# ============================================
# DOCKERFILE - FlliBrutti Backend API
# Solo l'applicazione .NET 8 - Il DB è separato
# ============================================

# === STAGE 1: Build ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia i file .csproj per cache delle dipendenze
COPY backend.sln .
COPY FlliBrutti.Backend.API/FlliBrutti.Backend.API.csproj FlliBrutti.Backend.API/
COPY FlliBrutti.Backend.Application/FlliBrutti.Backend.Application.csproj FlliBrutti.Backend.Application/
COPY FlliBrutti.Backend.Core/FlliBrutti.Backend.Core.csproj FlliBrutti.Backend.Core/
COPY FlliBrutti.Backend.Infrastructure/FlliBrutti.Backend.Infrastructure.csproj FlliBrutti.Backend.Infrastructure/
COPY FlliBrutti.Backend.Console/FlliBrutti.Backend.Console.csproj FlliBrutti.Backend.Console/
COPY FlliBrutti.Backend.Test/FlliBrutti.Backend.Test.csproj FlliBrutti.Backend.Test/

# Restore dipendenze
RUN dotnet restore backend.sln

# Copia tutto il codice
COPY . .

# Build
RUN dotnet build backend.sln -c Release --no-restore

# Publish
RUN dotnet publish FlliBrutti.Backend.API/FlliBrutti.Backend.API.csproj \
    -c Release \
    -o /app/publish \
    --no-build

# === STAGE 2: Runtime ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Curl per healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Utente non-root
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copia app pubblicata
COPY --from=build /app/publish .

# Permessi
RUN mkdir -p /app/Logs && chown -R appuser:appgroup /app

USER appuser

# Configurazione
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:5000/api/v1/Login/HealthCheck || exit 1

ENTRYPOINT ["dotnet", "FlliBrutti.Backend.API.dll"]