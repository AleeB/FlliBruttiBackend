# FlliBrutti Backend API

> Sistema gestionale completo per impresa NCC (Noleggio Con Conducente) sviluppato con .NET 8

##  Panoramica

FlliBrutti Backend è un sistema gestionale che offre:
- Gestione utenti e autenticazione sicura
- Tracciamento presenze dipendenti
- Gestione preventivi NCC

L'architettura segue i principi della **Clean Architecture**, garantendo separazione delle responsabilità, testabilità e facilità di manutenzione. Il backend espone API RESTful protette da autenticazione JWT.

---

##  Architettura

Il progetto adotta una struttura a layer organizzata in cinque progetti distinti:

| Layer | Descrizione |
|-------|-------------|
| **API** | Presentation layer con Controller ASP.NET Core e configurazione servizi |
| **Application** | Business logic con servizi, interfacce, DTO, mappers e validazione |
| **Core** | Domain layer con entità, enumerazioni e modelli di base |
| **Infrastructure** | Data access layer con DbContext di Entity Framework |
| **Test** | Unit test basati su xUnit |

---

##  Stack Tecnologico

### Framework e Runtime
- **.NET 8.0** — Framework di sviluppo principale (LTS)
- **ASP.NET Core 8** — Web API RESTful
- **Entity Framework Core 8.0.22** — ORM con approccio Code-First
- **Pomelo.EntityFrameworkCore.MySql 8.0.3** — Provider MySQL per EF Core

### Database
- **MySQL 8.0** — Database relazionale

### Sicurezza
- **JWT Bearer Authentication** — Autenticazione stateless
- **Refresh Token Rotation** — Rinnovo token con invalidazione automatica
- **Argon2id** — Hashing password
- **Role-Based Authorization** — Controllo accessi (Admin, Dipendente)

### Logging
- **Serilog** — Logging strutturato con output su console e file

---

##  Funzionalità

### Gestione Utenti
Due tipologie supportate:
- **Utenti autenticati** — Dipendenti e amministratori
- **Utenti non autenticati** — Clienti che richiedono preventivi

Gli amministratori possono creare account, modificare ruoli e gestire informazioni personali.

### Autenticazione
- **Access Token** — Validità 2 ore
- **Refresh Token** — Validità 15 giorni
- Protezione contro l'accumulo di token tramite eliminazione automatica

### Gestione Presenze (Firme)
- Registrazione entrata/uscita dipendenti
- Verifica automatica firme aperte
- Tracciamento con timestamp precisi

### Preventivi NCC
- Richiesta quotazioni da parte dei clienti
- Calcolo costi con eventuali extra
- Gestione stati: "da elaborare" → "completato"

---

##  Modello Dati

| Entità | Descrizione |
|--------|-------------|
| `Person` | Dati anagrafici base (nome, cognome, data di nascita) |
| `User : Person` | Utenti autenticati con email, password hashata e ruolo |
| `UserNotAuthenticated : Person` | Clienti che richiedono preventivi senza registrazione |
| `Firma` | Registrazioni entrata/uscita dipendenti |
| `PreventivoNCC` | Richieste preventivo con partenza, arrivo, costo |
| `PreventivoExtra` | Voci extra aggiuntive ai preventivi |
| `RefreshToken` | Token di refresh per gestione sessioni |

---

##  API Endpoints

Base URL: `/v1/api/`

### LoginController
| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| POST | `/login` | Autenticazione utente |
| POST | `/refresh` | Rinnovo token |
| POST | `/revoke` | Revoca token |
| POST | `/logout` | Logout |

### UserController
| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| POST | `/` | Crea nuovo utente |
| PATCH | `/UpdatePassword` | Aggiorna password |
| PATCH | `/UpdateType` | Modifica ruolo utente |

### PersonController
| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/` | Ottieni dati persona |
| PATCH | `/` | Aggiorna dati persona |

### FirmaController
| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/` | Lista firme |
| POST | `/Entry` | Registra entrata |
| POST | `/Exit` | Registra uscita |

### PreventivoNCCController
| Metodo | Endpoint | Descrizione |
|--------|----------|-------------|
| GET | `/` | Lista preventivi |
| POST | `/` | Crea preventivo |
| GET | `/ToExamine` | Preventivi da esaminare |
| PATCH | `/AddPreventivoCalculated` | Aggiungi preventivo calcolato |

---

## 🚀 Getting Started

### Prerequisiti
- .NET 8.0 SDK
- MySQL 8.0

### Installazione

```bash
# Clona il repository
git clone https://github.com/username/FlliBrutti-Backend.git

# Naviga nella directory
cd FlliBrutti-Backend

# Ripristina le dipendenze
dotnet restore

# Applica le migrazioni
dotnet ef database update

# Avvia l'applicazione
dotnet run --project API
```

### Configurazione

Configura le seguenti variabili in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FlliBrutti;User=root;Password=..."
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "FlliBrutti",
    "Audience": "FlliBrutti"
  }
}
```

---

## 🧪 Testing

```bash
dotnet test
```
