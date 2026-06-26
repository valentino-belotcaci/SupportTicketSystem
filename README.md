# Support Ticket System

A microservices-based internal helpdesk API built with ASP.NET Core 8, PostgreSQL, RabbitMQ, and Docker.
Employees can submit support tickets, track their status, and receive automatic notifications when ticket events occur.

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client (Swagger UI)                       │
└──────────────────────┬──────────────────────────────────────────┘
                       │ HTTP
          ┌────────────▼────────────┐
          │      Tickets.Api         │
          │   (CRUD + Stats)         │
          │   localhost:5112         │
          └────────────┬────────────┘
                       │ Publishes events
                       ▼
          ┌────────────────────────┐
          │        RabbitMQ         │
          │   ticket-events queue   │
          │   localhost:5672        │
          └────────────┬───────────┘
                       │ Consumes events
          ┌────────────▼────────────┐
          │   Notifications.Api      │
          │  (Event log + Query)     │
          │   localhost:5201         │
          └─────────────────────────┘

┌─────────────────┐     ┌──────────────────────┐
│   ticketsDB      │     │   notificationsDB     │
│  PostgreSQL:5432 │     │   PostgreSQL:5433     │
└─────────────────┘     └──────────────────────┘
```

Each service owns its own database (database-per-service pattern).
Services communicate asynchronously via RabbitMQ — Tickets.Api publishes events,
Notifications.Api consumes them. If Notifications.Api goes down, ticket operations
continue unaffected.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 Web API |
| Language | C# |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL 16 |
| Message Broker | RabbitMQ 3 |
| Containerization | Docker + Docker Compose |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit, Moq, FluentAssertions |
| Architecture | Microservices, Repository Pattern |

---

## Project Structure

```
SupportTicketSystem/
├── Tickets.Api/                  # Core service — ticket management
│   ├── Controllers/              # HTTP endpoints
│   ├── Models/                   # Entity classes (DB tables)
│   ├── DTOs/                     # Request/response models
│   ├── Enums/                    # TicketStatus, Priority, Category
│   ├── Data/                     # EF Core DbContext
│   ├── Interfaces/               # Repository and publisher interfaces
│   ├── Repository/               # Database access layer
│   ├── Mappers/                  # Entity ↔ DTO mapping
│   ├── Queries/                  # Filter query models
│   ├── Messages/                 # RabbitMQ message contracts
│   ├── Services/                 # RabbitMQPublisher
│   └── Migrations/               # EF Core migration files
│
├── Notifications.Api/            # Secondary service — event logging
│   ├── Controllers/              # HTTP endpoints
│   ├── Models/                   # Notification entity
│   ├── DTOs/                     # Request/response models
│   ├── Enums/                    # NotificationType
│   ├── Data/                     # EF Core DbContext
│   ├── Interfaces/               # Repository interface
│   ├── Repository/               # Database access layer
│   ├── Mappers/                  # Entity ↔ DTO mapping
│   ├── Messages/                 # RabbitMQ message contracts
│   ├── Services/                 # RabbitMQConsumer (BackgroundService)
│   └── Migrations/               # EF Core migration files
│
├── Tickets.Api.Tests/            # Unit tests for Tickets.Api
│   ├── Controllers/              # Controller tests (Moq)
│   └── Repository/               # Repository tests (InMemory DB)
│
├── Notifications.Api.Tests/      # Unit tests for Notifications.Api
│   ├── Controllers/              # Controller tests (Moq)
│   └── Repository/               # Repository tests (InMemory DB)
│
├── docker-compose.yml            # Full stack orchestration
├── .env.example                  # Environment variable template
└── SupportTicketSystem.sln       # Solution file
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Docker](https://www.docker.com/get-started) + Docker Compose
- [DBeaver](https://dbeaver.io/) (optional — database inspection)

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/valentino-belotcaci/SupportTicketSystem.git
cd SupportTicketSystem
```

### 2. Set up environment variables

```bash
cp .env.example .env
```

Edit `.env` with your own passwords:

```
TICKETS_DB_PASSWORD=your_password_here
NOTIFICATIONS_DB_PASSWORD=your_password_here
```

### 3. Set up User Secrets (local development only)

```bash
# Tickets.Api
cd Tickets.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=ticketsDB;Username=postgres;Password=YOUR_PASSWORD"
cd ..

# Notifications.Api
cd Notifications.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5433;Database=notificationsDB;Username=postgres;Password=YOUR_PASSWORD"
cd ..
```

---

## Running the Application

### Option A — Full Docker (recommended for demo)

Runs everything in containers — both APIs, both databases, RabbitMQ.

```bash
docker compose up -d --build
```

| Service | URL |
|---------|-----|
| Tickets.Api Swagger | http://localhost:5112/swagger |
| Notifications.Api Swagger | http://localhost:5201/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

### Option B — Local development (recommended for active coding)

Starts only the databases and RabbitMQ in Docker, runs APIs with hot reload.

```bash
# Terminal 1 — start infrastructure
docker compose up -d tickets-db notifications-db rabbitmq

# Terminal 2 — Tickets.Api with hot reload
cd Tickets.Api && dotnet watch

# Terminal 3 — Notifications.Api with hot reload
cd Notifications.Api && dotnet watch
```

### Apply database migrations

```bash
cd Tickets.Api && dotnet ef database update && cd ..
cd Notifications.Api && dotnet ef database update && cd ..
```

### Stop the application

```bash
# Stop APIs: CTRL+C in each terminal
docker compose stop        # stop containers, preserve data
docker compose down        # remove containers, preserve data
docker compose down -v     # remove containers AND data (clean slate)
```

---

## API Endpoints

### Tickets.Api — `http://localhost:5112`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tickets` | Get all tickets (filterable by status, priority, category) |
| GET | `/api/tickets/{id}` | Get ticket by ID |
| POST | `/api/tickets` | Create a new ticket |
| PUT | `/api/tickets/{id}` | Update ticket details |
| PATCH | `/api/tickets/{id}/status` | Update ticket status |
| PATCH | `/api/tickets/{id}/assign` | Assign ticket to a team member |
| DELETE | `/api/tickets/{id}` | Delete a ticket |
| GET | `/api/tickets/stats` | Get ticket counts grouped by status and priority |

### Notifications.Api — `http://localhost:5201`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/notifications` | Get all notifications |
| GET | `/api/notifications/{id}` | Get notification by ID |
| POST | `/api/notifications` | Create a notification manually |
| GET | `/api/notifications/ticket/{ticketId}` | Get all notifications for a ticket |

---

## Ticket Lifecycle

```
             ┌─────────────┐
             │    OPEN      │ ← created here automatically
             └──────┬───────┘
                    │
             ┌──────▼───────┐
             │  IN PROGRESS  │
             └──────┬───────┘
                    │
             ┌──────▼───────┐
             │   RESOLVED    │
             └──────┬───────┘
                    │
             ┌──────▼───────┐
             │    CLOSED     │ ← terminal state
             └──────────────┘
```

Status transitions are validated — invalid transitions (e.g. Open → Closed) return `400 Bad Request`.

---

## Event Flow

When a ticket event occurs, Tickets.Api publishes a message to RabbitMQ.
Notifications.Api consumes the message and stores it as a notification record.

```
1. POST /api/tickets
        ↓
2. Ticket saved to ticketsDB
        ↓
3. TicketEventMessage published to "ticket-events" queue
        ↓
4. RabbitMQConsumer receives message
        ↓
5. Notification saved to notificationsDB
        ↓
6. GET /api/notifications/ticket/{id} returns the event log
```

Events that trigger notifications:
- `Created` — new ticket was created
- `StatusChanged` — ticket status was updated
- `Assigned` — ticket was assigned to a team member

---

## Running Tests

```bash
# Run all tests
dotnet test

# Run only Tickets.Api tests
dotnet test Tickets.Api.Tests

# Run only Notifications.Api tests
dotnet test Notifications.Api.Tests

# Run with detailed output
dotnet test --verbosity normal
```

### Test coverage

| Project | Controller Tests | Repository Tests |
|---------|-----------------|-----------------|
| Tickets.Api | ✅ All endpoints | ✅ All repository methods |
| Notifications.Api | ✅ All endpoints | ✅ All repository methods |

Controller tests use **Moq** to mock the repository layer — no database required.
Repository tests use **EF Core InMemory** database — no PostgreSQL required.

---

## Security

- Connection strings stored in **User Secrets** for local development
- Connection strings passed via **environment variables** in Docker
- `.env` file is **gitignored** — never committed to source control
- `.env.example` provides the variable structure without real values
- `appsettings.json` contains only empty placeholders — safe to commit

---

## Design Decisions

**Why two microservices?**
Tickets.Api handles core business logic and must always be available. Notifications.Api is a secondary concern — if it goes down, ticket operations continue unaffected. Separating them allows independent scaling and deployment.

**Why RabbitMQ instead of direct HTTP calls?**
Asynchronous messaging decouples the services. Tickets.Api doesn't wait for Notifications.Api to respond, improving performance and resilience. Messages are persisted in the queue — if Notifications.Api is temporarily down, no events are lost.

**Why database-per-service?**
Each service owns its data. There are no shared tables or foreign keys across services — TicketId in the Notifications database is a plain Guid, not a foreign key. This prevents tight coupling at the database level.

**Why Repository Pattern?**
Controllers are kept thin — they handle HTTP concerns only. All database logic lives in repositories behind interfaces, making the code testable (controllers can be tested with mocked repositories) and the database layer swappable.

---

## Author

**Valentino Belotcaci** — [GitHub](https://github.com/valentino-belotcaci) — [LinkedIn](https://www.linkedin.com/in/valentino-belotcaci-3b897738b/)