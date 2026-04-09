# TeacupBoutique

TeacupBoutique is a multi-service web application built with:

- ASP.NET Core (.NET 8)
- PostgreSQL 16
- React (Vite + TanStack Router)
- RabbitMQ
- Docker Compose

The backend consists of five services (`auth`, `gateway`, `inventory`, `orders`, `payments`) plus a `notifications` service, all communicating via RabbitMQ. The project runs either with Docker Compose or via local development tools.

---

# Local Development Setup

## Configuration & Secrets Overview

Configuration is split between non-sensitive values committed in `appsettings.Development.json` and secrets which are never committed.

There are two pathways depending on how you run the app:

- **Pathway 1 — Docker Compose:** secrets live in `.secrets/*.env` files (gitignored). Used with `docker compose up`.
- **Pathway 2 — dotnet run:** secrets are stored via .NET User Secrets outside the repo. Used when running services individually.

The frontend (`src/client`) uses `.env.development` / `.env.production` for public keys (safe to commit), and an optional gitignored `.env.local` for local overrides.

See [Configuration & Secrets Management](#configuration--secrets-management) for the full reference.

---

## 1. Create Secrets

Secrets are **not committed to the repository**.  
Each developer must create their own local secret files.

```bash
mkdir .secrets
```

Populate each file below. All `.secrets/` files are gitignored.

---

### PostgreSQL passwords

```bash
echo "POSTGRES_PASSWORD=yourpassword" > .secrets/postgres-auth.env
echo "POSTGRES_PASSWORD=yourpassword" > .secrets/postgres-inventory.env
echo "POSTGRES_PASSWORD=yourpassword" > .secrets/postgres-orders.env
echo "POSTGRES_PASSWORD=yourpassword" > .secrets/postgres-payments.env
```

---

### Service secrets

**`.secrets/auth.env`**
```
ConnectionStrings__AuthDb=Host=postgres-auth;Port=5432;Database=authdb;Username=postgres;Password=yourpassword
Jwt__Key=your-long-random-secret-at-least-32-chars
Jwt__Issuer=teacupboutique
Jwt__Audience=teacupboutique
AdminSeed__Email=admin@example.com
AdminSeed__Password=Admin@Password1
Turnstile__SecretKey=your-turnstile-secret
```

**`.secrets/inventory.env`**
```
ConnectionStrings__InventoryDb=Host=postgres-inventory;Port=5432;Database=inventorydb;Username=postgres;Password=yourpassword
Cloudinary__Url=cloudinary://api_key:api_secret@cloud_name
```

**`.secrets/orders.env`**
```
ConnectionStrings__OrdersDb=Host=postgres-orders;Port=5432;Database=ordersdb;Username=postgres;Password=yourpassword
Jwt__Key=your-long-random-secret-at-least-32-chars
Jwt__Issuer=teacupboutique
Jwt__Audience=teacupboutique
Turnstile__SecretKey=your-turnstile-secret
```

**`.secrets/payments.env`**
```
ConnectionStrings__PaymentsDb=Host=postgres-payments;Port=5432;Database=paymentsdb;Username=postgres;Password=yourpassword
Stripe__SecretKey=sk_test_...
Stripe__WebhookSecret=whsec_...
Turnstile__SecretKey=your-turnstile-secret
```

**`.secrets/notifications.env`**
```
Mailgun__ApiKey=your-mailgun-key
```

**`.secrets/gateway.env`**
```
Jwt__Key=your-long-random-secret-at-least-32-chars
Jwt__Issuer=teacupboutique
Jwt__Audience=teacupboutique
```

Notes:
- `Jwt__Key` **must be at least 32 characters** for HS256 signing.
- The same `Jwt__Key` value must be used in `auth`, `orders`, and `gateway`.
- Environment variables use `__` (double underscore) to represent nested ASP.NET config: `Jwt__Key` → `Jwt:Key`.

---

# Running the Application

## Run the Full Stack with Docker

```bash
docker compose up --build
```

Services will be available at:

| Service | URL |
|---------|-----|
| Gateway | http://localhost:5054 |
| Client (Vite dev) | http://localhost:5173 |
| RabbitMQ Management | http://localhost:15672 |
| Auth DB (PostgreSQL) | localhost:5432 |
| Inventory DB | localhost:5433 |
| Orders DB | localhost:5434 |
| Payments DB | localhost:5435 |

The first startup may take **10–20 seconds** while PostgreSQL and RabbitMQ initialise.

---

# Database

PostgreSQL 16 runs inside Docker (one container per service database).

Data is persisted using named Docker volumes:

```
postgres-auth-data
postgres-inventory-data
postgres-orders-data
postgres-payments-data
```

EF Core migrations run automatically on service startup in Development mode.

To completely reset all databases:

```bash
docker compose down -v
```

This removes containers **and** all database volumes.

---

# Running Services Individually (Optional)

You can run services individually during development. Start only the infrastructure:

```bash
docker compose up postgres-auth postgres-inventory postgres-orders postgres-payments rabbitmq
```

Then run a backend service locally:

```bash
cd src/services/auth
dotnet run
```

Run the frontend:

```bash
cd src/client
npm run dev
```

When running with `dotnet run`, use .NET User Secrets instead of `.env` files (see below).

---

# .NET User Secrets (Local Development)

When running backend services with `dotnet run`, store secrets using **.NET user-secrets** instead of `.env` files. User secrets are stored outside the repository on your machine.

```bash
cd src/services/auth
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AuthDb" "Host=localhost;Port=5432;Database=authdb;Username=postgres;Password=yourpassword"
dotnet user-secrets set "Jwt:Key" "your-long-random-secret-at-least-32-chars"
dotnet user-secrets set "Jwt:Issuer" "teacupboutique"
dotnet user-secrets set "Jwt:Audience" "teacupboutique"
dotnet user-secrets set "AdminSeed:Email" "admin@example.com"
dotnet user-secrets set "AdminSeed:Password" "Admin@Password1"
dotnet user-secrets set "Turnstile:SecretKey" "your-turnstile-secret"
```

```bash
cd src/services/inventory
dotnet user-secrets set "ConnectionStrings:InventoryDb" "Host=localhost;Port=5433;Database=inventorydb;Username=postgres;Password=yourpassword"
dotnet user-secrets set "Cloudinary:Url" "cloudinary://api_key:api_secret@cloud_name"
```

```bash
cd src/services/orders
dotnet user-secrets set "ConnectionStrings:OrdersDb" "Host=localhost;Port=5434;Database=ordersdb;Username=postgres;Password=yourpassword"
dotnet user-secrets set "Jwt:Key" "your-long-random-secret-at-least-32-chars"
dotnet user-secrets set "Jwt:Issuer" "teacupboutique"
dotnet user-secrets set "Jwt:Audience" "teacupboutique"
dotnet user-secrets set "Turnstile:SecretKey" "your-turnstile-secret"
```

```bash
cd src/services/payments
dotnet user-secrets set "ConnectionStrings:PaymentsDb" "Host=localhost;Port=5435;Database=paymentsdb;Username=postgres;Password=yourpassword"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "Turnstile:SecretKey" "your-turnstile-secret"
```

```bash
cd src/services/notifications
dotnet user-secrets set "Mailgun:ApiKey" "your-mailgun-key"
```

```bash
cd src/gateway
dotnet user-secrets set "Jwt:Key" "your-long-random-secret-at-least-32-chars"
dotnet user-secrets set "Jwt:Issuer" "teacupboutique"
dotnet user-secrets set "Jwt:Audience" "teacupboutique"
```

Note: when connecting from `dotnet run` (outside Docker), use `localhost` with the **host-mapped port** (5432–5435) rather than the container name.

---

# Development Notes

- EF Core migrations run automatically in **Development** mode on startup.
- Configuration is loaded in this order (later sources win):
  1. `appsettings.json`
  2. `appsettings.Development.json`
  3. Environment variables
  4. .NET user secrets (when running with `dotnet run`)

Secrets are **never committed to git**.

---

# Recommended Workflow

1. Create `.secrets` directory and populate all env files (see above)
2. Start the backend + infrastructure:

```bash
docker compose up --build
```

3. In a separate terminal, start the frontend dev server:

```bash
cd src/client
npm run dev
```

Client available at http://localhost:5173 with hot reload.

4. When you change a backend service, rebuild just that service:

```bash
docker compose up --build auth
```

---

# Running a Production Build Locally

Use `docker-compose.prod.local.yml` to run a full production-like stack locally — all services built from source, `ASPNETCORE_ENVIRONMENT=Production`, PostgreSQL, RabbitMQ, and a compiled React client. No nginx, no SSL — the gateway is exposed directly on port 5054.

```bash
docker compose -f docker-compose.prod.local.yml up --build
```

| Endpoint | URL |
|----------|-----|
| Client (compiled) | http://localhost:3000 |
| Gateway API | http://localhost:5054 |
| RabbitMQ Management | http://localhost:15672 |

**Hybrid mode** — backend via prod-local, frontend via Vite dev server (hot reload):

```bash
# Terminal 1 — start all services except the client container
docker compose -f docker-compose.prod.local.yml up --build --scale client=0

# Terminal 2 — run frontend in dev mode
cd src/client && npm run dev
```

Client runs at http://localhost:5173, pointing to the gateway at http://localhost:5054.

---

# Resetting the Environment

To fully reset the development environment:

```bash
docker compose down -v
docker compose up --build
```

This removes containers and all database volumes, then rebuilds from scratch.

---

# Configuration & Secrets Management

## Pathway 1 — Docker Compose (`.secrets/*.env` files)

Used when running via `docker compose up`. Each service reads its secrets from a `.env` file in the `.secrets/` directory (gitignored). See the [Create Secrets](#1-create-secrets) section above for the full file contents.

Summary of required files:

| File | Used by |
|------|---------|
| `.secrets/postgres-auth.env` | PostgreSQL auth container |
| `.secrets/postgres-inventory.env` | PostgreSQL inventory container |
| `.secrets/postgres-orders.env` | PostgreSQL orders container |
| `.secrets/postgres-payments.env` | PostgreSQL payments container |
| `.secrets/auth.env` | Auth service |
| `.secrets/inventory.env` | Inventory service |
| `.secrets/orders.env` | Orders service |
| `.secrets/payments.env` | Payments service |
| `.secrets/notifications.env` | Notifications service |
| `.secrets/gateway.env` | Gateway |

---

## Pathway 2 — `dotnet run` (.NET User Secrets)

Used when running services individually with `dotnet run`. Secrets are stored outside the repo via the .NET user-secrets system (single colon separator instead of double underscore). See the [.NET User Secrets](#net-user-secrets-local-development) section above.

---

## Frontend (Vite)

`src/client/.env.development` and `src/client/.env.production` are committed and contain only **public** keys safe to expose:

```
VITE_TURNSTILE_SITE_KEY_MANAGED=    ← public site key, safe to commit
VITE_STRIPE_PUBLISHABLE_KEY=        ← public publishable key, safe to commit
```

For local overrides, create `src/client/.env.local` (gitignored):

```
VITE_TURNSTILE_SITE_KEY_MANAGED=
VITE_STRIPE_PUBLISHABLE_KEY=
```

---

# Production

See [DEPLOYMENT.md](DEPLOYMENT.md) for production environment strategy, V1/V2 deployment plans, and the one-time VM setup guide.
