# ReadmeIstruzioni - Deploy locale

Queste istruzioni permettono di avviare il progetto in locale usando Docker (consigliato). Sono presenti anche note per esecuzione in modalita sviluppo.

## Prerequisiti
- Docker Desktop con Docker Compose v2

## Deploy locale con Docker

### 1) Crea la rete condivisa
La configurazione usa una rete Docker esterna chiamata `app-net` condivisa tra backend e frontend.

```powershell
# Da eseguire una sola volta
docker network create app-net
```     

### 2) Avvia il backend (API + MySQL)

```powershell
cd FlliBruttiBackend\FlliBruttiBackend\backend
```

Inserire il file `.env` ottenuto via Email dentro .\FlliBruttiBackend\backend


Avvia i container:

```powershell
docker compose up -d --build
```

(Opzionale) Avvia phpMyAdmin:

```powershell
docker compose --profile debug up -d
```

### 3) Avvia il frontend (Angular build + Nginx)

```powershell
cd ..\..\..
```

```powershell
docker compose up -d 
```

### 4) Verifica
- Frontend: http://localhost:80
- Backend API: http://localhost:5000/api/v1/Login/HealthCheck
- MySQL: localhost:6000
- phpMyAdmin (se attivo): http://localhost:8080

### 5) Stop e cleanup
Esegui `docker compose down` nella cartella backend e nella root del progetto.
Esegui `docker compose down -v` per cancellare i volumi, nella cartella backend e nella root del progetto.

## Modalita sviluppo (opzionale)

### Frontend
```powershell
cd FlliBruttiFrontend
npm ci
npm start
```

### Backend
```powershell
cd FlliBruttiBackend\FlliBruttiBackend\backend

dotnet restore
# Avvio profilo https
dotnet run --project FlliBrutti.Backend.API --launch-profile https
```

## Note
- Il database viene inizializzato dagli script in `FlliBruttiBackend\FlliBruttiBackend\backend\init-db`.
- Le porte possono essere cambiate nei rispettivi `docker-compose.yml`.


