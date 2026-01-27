# Sorgenti e configurazione

## Sorgenti applicazione

### Frontend (Angular)
- Root: `FlliBruttiFrontend`
- Sorgenti: `FlliBruttiFrontend/src`
- Principali aree:
  - `FlliBruttiFrontend/src/app/pages` (pagine)
  - `FlliBruttiFrontend/src/app/services` (servizi)
  - `FlliBruttiFrontend/src/app/models` (modelli)
  - `FlliBruttiFrontend/src/app/auth` (guard e interceptor)

### Backend (.NET 8)
- Root: `FlliBruttiBackend/FlliBruttiBackend/backend`
- Soluzione: `backend.sln`
- Progetti:
  - `FlliBrutti.Backend.API` (Web API)
  - `FlliBrutti.Backend.Application` (servizi, DTO, mapping)
  - `FlliBrutti.Backend.Core` (modelli dominio)
  - `FlliBrutti.Backend.Infrastructure` (EF Core, DB)
  - `FlliBrutti.Backend.Test` (test)

## File YAML di configurazione

### `FlliBruttiBackend/backend/docker-compose.yml`
```yaml
version: '3.8'

services:
  api:
    image: ${REGISTRY:-ghcr.io}/${IMAGE_BE}:${TAG:-latest}
    container_name: fllibrutti-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__FlliBruttiDatabase=Server=db;Port=3306;Database=FlliBrutti;User=fllibrutti;Password=${DB_PASSWORD};
      - Jwt__SecretKey=${JWT_SECRET_KEY}
      - Jwt__Issuer=FlliBrutti.Backend.API
      - Jwt__Audience=FlliBrutti.Frontend
      - Jwt__AccessTokenExpirationHours=2
      - Jwt__RefreshTokenExpirationDays=15
      - Security__Secret=${SECURITY_SECRET}
    depends_on:
      db:
        condition: service_healthy
    networks:
      - fllibrutti-network
      - app-net
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/v1/Login/HealthCheck"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 15s

  db:
    image: ${REGISTRY:-ghcr.io}/${IMAGE_DB}:${TAG:-latest}
    container_name: fllibrutti-db
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD}
      MYSQL_DATABASE: FlliBrutti
      MYSQL_USER: fllibrutti
      MYSQL_PASSWORD: ${DB_PASSWORD}
    ports:
      - "6000:3306"
    volumes:
      - mysql-data:/var/lib/mysql
    networks:
      - fllibrutti-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-p${DB_ROOT_PASSWORD}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    command: >
      --default-authentication-plugin=mysql_native_password
      --character-set-server=utf8mb4
      --collation-server=utf8mb4_unicode_ci

volumes:
  mysql-data:

networks:
  fllibrutti-network:
    driver: bridge
  app-net:
    external: true
```

### `FlliBruttiBackend/FlliBruttiBackend/backend/docker-compose.local.yml`
```yaml
version: '3.8'

services:
  # === BACKEND API ===
  api:
    build:
      context: .
      dockerfile: Dockerfile
      target: runtime
    container_name: fllibrutti-api
    ports:
      - "5001:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      # Connection string per MySQL nel container
      - ConnectionStrings__FlliBruttiDatabase=${DATABASE_URL}
      # JWT Configuration
      - Jwt__SecretKey=${JWT_SECRET_KEY:-SuperSecretKeyForJWTAuthenticationMinimum32CharactersLong!}
      - Jwt__Issuer=FlliBrutti.Backend.API
      - Jwt__Audience=FlliBrutti.Frontend
      - Jwt__AccessTokenExpirationHours=2
      - Jwt__RefreshTokenExpirationDays=15
      # Security
      - Security__Secret=${SECURITY_SECRET:-Secret-Di-Produzione-Sicuro}
    depends_on:
      db:
        condition: service_healthy
    networks:
      - fllibrutti-network  # Rete con il database
      - app-net  # Rete condivisa con il frontend
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/api/v1/Login/HealthCheck"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 15s
    volumes:
      - api-logs:/app/Logs

  # === DATABASE MYSQL ===
  db:
    image: mysql:8.0
    container_name: fllibrutti-db
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD:-root2002}
      MYSQL_DATABASE: ${DB_NAME:-FlliBrutti}
      MYSQL_USER: ${DB_USER:-fllibrutti}
      MYSQL_PASSWORD: ${DB_PASSWORD:-SecurePassword123!}
    ports:
      - "6000:3306"
    volumes:
      - mysql-data:/var/lib/mysql
      - ./init-db:/docker-entrypoint-initdb.d  # Script SQL di inizializzazione
    networks:
      - fllibrutti-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-p${DB_ROOT_PASSWORD:-root2002}"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s
    command: >
      --default-authentication-plugin=mysql_native_password
      --character-set-server=utf8mb4
      --collation-server=utf8mb4_unicode_ci

  # === PHPMYADMIN (opzionale, per debug) ===
  phpmyadmin:
    image: phpmyadmin:latest
    container_name: fllibrutti-phpmyadmin
    environment:
      PMA_HOST: db
      PMA_PORT: 3306
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD:-root2002}
    ports:
      - "8080:80"
    depends_on:
      - db
    networks:
      - fllibrutti-network
    restart: unless-stopped
    profiles:
      - debug  # Avvia solo con: docker-compose --profile debug up

# === VOLUMI ===
volumes:
  mysql-data:
    driver: local
  api-logs:
    driver: local

# === NETWORK ===
networks:
  fllibrutti-network:
    driver: bridge
  app-net:
    external: true
```

### `.github/workflows/backend-ci-cd.yml`
```yaml
# ============================================
# CI - Continuous Integration
# ============================================
name: CI - Build & Test

on:
  push:
    branches: [main, master]
    paths:
      - 'backend/**'
      - '.github/workflows/ci.yml'
  pull_request:
    branches: [main, master]

env:
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-test:
    name: Build & Test
    runs-on: ubuntu-latest

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup .NET ${{ env.DOTNET_VERSION }}
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('backend/**/*.csproj') }}
          restore-keys: |
            ${{ runner.os }}-nuget-

      - name: Restore dependencies
        run: dotnet restore backend/backend.sln

      - name: Build solution
        run: dotnet build backend/backend.sln --configuration Release --no-restore

      # - name: Run tests
      #   run: dotnet test backend/backend.sln --configuration Release --no-build --verbosity normal
      #   continue-on-error: true

  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    needs: build-and-test

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Check for vulnerable packages
        run: |
          dotnet restore backend/backend.sln
          dotnet list backend/backend.sln package --vulnerable --include-transitive 2>&1 | tee vulnerability-report.txt
          if grep -q "has the following vulnerable packages" vulnerability-report.txt; then
            echo "::warning: Vulnerable packages detected!"
          fi

  docker-build-test:
    name: Docker Build Test
    runs-on: ubuntu-latest
    needs: build-and-test

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Build Backend image (test)
        uses: docker/build-push-action@v5
        with:
          context: ./backend
          file: ./backend/Dockerfile
          push: false
          tags: fllibrutti-backend:test

      - name: Build Database image (test)
        uses: docker/build-push-action@v5
        with:
          context: ./backend
          file: ./backend/Dockerfile.db
          push: false
          tags: fllibrutti-db:test


    # ============================================
    # CD - Continuous Deployment
    # Build Backend + Database → Deploy Staging
    # ============================================
    name: CD - Build & Deploy
    
    on:
      push:
        branches: [main, master]
        paths:
          - 'backend/**'
          - '.github/workflows/cd.yml'
      workflow_dispatch:
    
    env:
      REGISTRY: ghcr.io
      IMAGE_BACKEND: ${{ github.repository_owner }}/fllibrutti-backend
      IMAGE_DB: ${{ github.repository_owner }}/fllibrutti-db
    
    jobs:
      # ============================================
      # JOB 1: Build e Push Docker Images
      # ============================================
      build-and-push:
        name: Build & Push Images
        runs-on: ubuntu-latest
        
        outputs:
          short-sha: ${{ steps.vars.outputs.short_sha }}
    
        permissions:
          contents: read
          packages: write
    
        steps:
          - name: Checkout repository
            uses: actions/checkout@v4
    
          - name: Set variables
            id: vars
            run: echo "short_sha=$(git rev-parse --short HEAD)" >> $GITHUB_OUTPUT
    
          - name: Login to GitHub Container Registry
            uses: docker/login-action@v3
            with:
              registry: ${{ env.REGISTRY }}
              username: ${{ github.actor }}
              password: ${{ secrets.GITHUB_TOKEN }}
    
          - name: Set up Docker Buildx
            uses: docker/setup-buildx-action@v3
    
          # ========== BACKEND IMAGE ==========
          - name: Extract metadata (Backend)
            id: meta-backend
            uses: docker/metadata-action@v5
            with:
              images: ${{ env.REGISTRY }}/${{ env.IMAGE_BACKEND }}
              tags: |
                type=sha,prefix=
                type=raw,value=latest
    
          - name: Build and push Backend image
            uses: docker/build-push-action@v5
            with:
              context: ./backend
              file: ./backend/Dockerfile
              push: true
              tags: ${{ steps.meta-backend.outputs.tags }}
              labels: ${{ steps.meta-backend.outputs.labels }}
              cache-from: type=gha,scope=backend
              cache-to: type=gha,mode=max,scope=backend
    
          # ========== DATABASE IMAGE ==========
          - name: Extract metadata (Database)
            id: meta-db
            uses: docker/metadata-action@v5
            with:
              images: ${{ env.REGISTRY }}/${{ env.IMAGE_DB }}
              tags: |
                type=sha,prefix=
                type=raw,value=latest
    
          - name: Build and push Database image
            uses: docker/build-push-action@v5
            with:
              context: ./backend
              file: ./backend/Dockerfile.db
              push: true
              tags: ${{ steps.meta-db.outputs.tags }}
              labels: ${{ steps.meta-db.outputs.labels }}
              cache-from: type=gha,scope=db
              cache-to: type=gha,mode=max,scope=db
    
          - name: Build Summary
            run: |
              echo "## Docker Images Built!" >> $GITHUB_STEP_SUMMARY
              echo "" >> $GITHUB_STEP_SUMMARY
              echo "| Image | Tag |" >> $GITHUB_STEP_SUMMARY
              echo "|-------|-----|" >> $GITHUB_STEP_SUMMARY
              echo "| Backend | \`${{ env.REGISTRY }}/${{ env.IMAGE_BACKEND }}:latest\` |" >> $GITHUB_STEP_SUMMARY
              echo "| Database | \`${{ env.REGISTRY }}/${{ env.IMAGE_DB }}:latest\` |" >> $GITHUB_STEP_SUMMARY
        
    
      # ============================================
      # JOB 2: Deploy to Staging
      # ============================================
      deploy-staging:
        name: Deploy to Staging
        runs-on: ubuntu-latest
        needs: build-and-push
        
        environment:
          name: staging
          url: ${{ vars.STAGING_URL }}
    
        steps:
          - name: Checkout repository
            uses: actions/checkout@v4
    
          - name: Copy docker-compose to server
            uses: appleboy/scp-action@v0.1.7
            if: ${{ vars.STAGING_HOST != '' }}
            with:
              host: ${{ vars.STAGING_HOST }}
              username: ${{ secrets.STAGING_USER }}
              key: ${{ secrets.STAGING_SSH_KEY }}
              source: "backend/docker-compose.prod.yml"
              target: "/opt/fllibrutti/"
              strip_components: 1
    
          - name: Deploy to staging server
            uses: appleboy/ssh-action@v1.0.3
            if: ${{ vars.STAGING_HOST != '' }}
            with:
              host: ${{ vars.STAGING_HOST }}
              username: ${{ secrets.STAGING_USER }}
              key: ${{ secrets.STAGING_SSH_KEY }}
              script: |
                cd /opt/fllibrutti
                
                # Login a GHCR
                echo "${{ secrets.GHCR_PAT }}" | docker login ghcr.io -u ${{ github.actor }} --password-stdin
                
                REGISTRY=ghcr.io
                IMAGE_BACKEND=${{ github.repository_owner }}/fllibrutti-backend
                IMAGE_DB=${{ github.repository_owner }}/fllibrutti-db
                TAG=latest
                DB_ROOT_PASSWORD=${{ secrets.DB_ROOT_PASSWORD }}
                DB_PASSWORD=${{ secrets.DB_PASSWORD }}
                JWT_SECRET_KEY=${{ secrets.JWT_SECRET_KEY }}
                SECURITY_SECRET=${{ secrets.SECURITY_SECRET }}
                EOF
                
                # Pull e deploy
                docker-compose -f docker-compose.prod.yml pull
                docker-compose -f docker-compose.prod.yml up -d
                
                # Cleanup
                docker image prune -f
                
                echo "Deployment completed!"
                docker-compose -f docker-compose.prod.yml ps
    
          - name: Health Check
            if: ${{ vars.STAGING_URL != '' }}
            run: |
              echo "Waiting for services to start..."
              sleep 45
              
              HEALTH_URL="${{ vars.STAGING_URL }}/api/v1/Login/HealthCheck"
              echo "Checking: $HEALTH_URL"
              
              for i in {1..5}; do
                if curl -sf "$HEALTH_URL"; then
                  echo ""
                  echo "Health check passed!"
                  exit 0
                fi
                echo "Attempt $i failed, retrying in 15s..."
                sleep 15
              done
              
              echo "Health check failed"
              exit 1
    
          - name: Deploy Summary
            run: |
              echo "## Staging Deployment" >> $GITHUB_STEP_SUMMARY
              echo "" >> $GITHUB_STEP_SUMMARY
              echo "**Status:** ${{ job.status }}" >> $GITHUB_STEP_SUMMARY
              echo "**Commit:** \`${{ needs.build-and-push.outputs.short-sha }}\`" >> $GITHUB_STEP_SUMMAR


