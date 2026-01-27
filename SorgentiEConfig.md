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

### `docker-compose.yml`
```yaml
services:
  frontend-build:
    image: node:20-alpine
    working_dir: /app
    volumes:
      - ./FlliBruttiFrontend:/app
      - /app/node_modules
      - frontend_dist:/app/dist
    command: sh -c "npm ci && npm run build -- --output-path=dist"
    networks:
      - app-net

  nginx:
    image: nginx:1.27-alpine
    depends_on:
      - frontend-build
    ports:
      - "0.0.0.0:8081:80"
    volumes:
      - frontend_dist:/usr/share/nginx/html:ro
      - ./nginx/default.conf:/etc/nginx/conf.d/default.conf:ro
    networks:
      - app-net

volumes:
  frontend_dist:

networks:
  app-net:
    external: true
```

### `FlliBruttiBackend/FlliBruttiBackend/backend/docker-compose.yml`
```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
      target: runtime
    container_name: fllibrutti-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__FlliBruttiDatabase=${DATABASE_URL}
      - Jwt__SecretKey=${JWT_SECRET_KEY:-SuperSecretKeyForJWTAuthenticationMinimum32CharactersLong!}
      - Jwt__Issuer=FlliBrutti.Backend.API
      - Jwt__Audience=FlliBrutti.Frontend
      - Jwt__AccessTokenExpirationHours=2
      - Jwt__RefreshTokenExpirationDays=15
      - Security__Secret=${SECURITY_SECRET:-Secret-Di-Produzione-Sicuro}
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
    volumes:
      - api-logs:/app/Logs

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
      - ./init-db:/docker-entrypoint-initdb.d
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
      - debug

volumes:
  mysql-data:
    driver: local
  api-logs:
    driver: local

networks:
  fllibrutti-network:
    driver: bridge
  app-net:
    external: true
```

### `.github/workflows/frontend-ci-cd.yml`
```yaml
name: frontend-ci-cd

on:
  push:
    branches: [main]
    paths:
      - "FlliBruttiFrontend/**"
      - "nginx/**"
      - "dockerfile"
      - "docker-compose.yml"
      - ".github/workflows/frontend-ci-cd.yml"
  pull_request:
    paths:
      - "FlliBruttiFrontend/**"
      - "nginx/**"
      - "dockerfile"
      - "docker-compose.yml"
      - ".github/workflows/frontend-ci-cd.yml"
  workflow_dispatch:

permissions:
  contents: read
  packages: write

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/fllibrutti-frontend
  FRONTEND_DIR: FlliBruttiFrontend
  NODE_VERSION: 20

jobs:
  ci:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ${{ env.FRONTEND_DIR }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: npm
          cache-dependency-path: ${{ env.FRONTEND_DIR }}/package-lock.json
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Test
        run: npm test -- --watch=false

  docker:
    needs: ci
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Login to registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - name: Build and push image
        uses: docker/build-push-action@v6
        with:
          context: .
          file: ./dockerfile
          push: true
          tags: |
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
```

### `.github/workflows/backend-ci-cd.yml`
```yaml
name: backend-ci-cd

on:
  push:
    branches: [main]
    paths:
      - "FlliBruttiBackend/**"
      - ".github/workflows/backend-ci-cd.yml"
  pull_request:
    paths:
      - "FlliBruttiBackend/**"
      - ".github/workflows/backend-ci-cd.yml"
  workflow_dispatch:

permissions:
  contents: read
  packages: write

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository_owner }}/fllibrutti-backend
  BACKEND_DIR: FlliBruttiBackend/FlliBruttiBackend/backend

jobs:
  ci:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ${{ env.BACKEND_DIR }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - name: Restore
        run: dotnet restore backend.sln
      - name: Build
        run: dotnet build backend.sln -c Release --no-restore
      - name: Test
        run: dotnet test backend.sln -c Release --no-build

  docker:
    needs: ci
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Login to registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}
      - name: Build and push image
        uses: docker/build-push-action@v6
        with:
          context: ${{ env.BACKEND_DIR }}
          file: ${{ env.BACKEND_DIR }}/Dockerfile
          push: true
          tags: |
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:sha-${{ github.sha }}
            ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
```
