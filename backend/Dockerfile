# ============================================
# DOCKERFILE - FlliBrutti Backend API (.NET 8)
# Multi-stage build ottimizzato per produzione
# ============================================

# === STAGE 1: Build ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia i file .csproj per sfruttare la cache delle dipendenze
COPY backend.sln .
COPY FlliBrutti.Backend.API/FlliBrutti.Backend.API.csproj FlliBrutti.Backend.API/
COPY FlliBrutti.Backend.Application/FlliBrutti.Backend.Application.csproj FlliBrutti.Backend.Application/
COPY FlliBrutti.Backend.Core/FlliBrutti.Backend.Core.csproj FlliBrutti.Backend.Core/
COPY FlliBrutti.Backend.Infrastructure/FlliBrutti.Backend.Infrastructure.csproj FlliBrutti.Backend.Infrastructure/
COPY FlliBrutti.Backend.Console/FlliBrutti.Backend.Console.csproj FlliBrutti.Backend.Console/
COPY FlliBrutti.Backend.Test/FlliBrutti.Backend.Test.csproj FlliBrutti.Backend.Test/

# Restore delle dipendenze (cached se i .csproj non cambiano)
RUN dotnet restore backend.sln

# Copia tutto il codice sorgente
COPY . .

# Build in modalità Release
RUN dotnet build backend.sln -c Release --no-restore

# Pubblica l'applicazione
RUN dotnet publish FlliBrutti.Backend.API/FlliBrutti.Backend.API.csproj \
    -c Release \
    -o /app/publish \
    --no-build

# === STAGE 2: Runtime ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Installa curl per healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Crea utente non-root per sicurezza
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copia l'applicazione pubblicata
COPY --from=build /app/publish .

# Crea directory per i log
RUN mkdir -p /app/Logs && chown -R appuser:appgroup /app

# Cambia all'utente non-root
USER appuser

# Variabili d'ambiente
ENV ASPNETCORE_URLS=http://+:5000 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Esponi la porta
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f https://localhost:5000/api/v1/Login/HealthCheck || exit 1
# Entrypoint
ENTRYPOINT ["dotnet", "FlliBrutti.Backend.API.dll"]