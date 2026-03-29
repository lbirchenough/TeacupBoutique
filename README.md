# TeacupBoutique

TeacupBoutique is a multi-service web application built with:

- ASP.NET Core (.NET)
- SQL Server
- React (Vite)
- Docker Compose

The backend consists of multiple services (e.g. `auth`, `gateway`) and a SQL Server database.  
The project is designed to run either with Docker Compose or via local development tools.

---

# Local Development Setup

## 1. Create Secrets

Secrets are **not committed to the repository**.  
Each developer must create their own local secret files.

Create a `.secrets` directory in the project root:

mkdir .secrets

---

## Auth Service Secrets

Create the file:

.secrets/auth.env

Example contents:

Jwt__Issuer=AuthService  
Jwt__Audience=CommerceSpa  
Jwt__Key=YOUR_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS  
ConnectionStrings__AuthDb=Server=sql-auth,1433;Database=authdb;User Id=SA;Password=[AUTHDB_PASSWORD];TrustServerCertificate=true

Notes:

- `Jwt__Key` **must be at least 32 characters** for HS256 signing.
- Environment variables use `__` to represent nested configuration values in ASP.NET.

Example mapping:

Jwt__Key → Jwt:Key  
ConnectionStrings__AuthDb → ConnectionStrings:AuthDb

---

## SQL Server Secrets

Create:

.secrets/sql-auth.env

Contents:

MSSQL_SA_PASSWORD=[AUTHDB_PASSWORD]

---


# Running the Application

## Run the Full Stack with Docker

Start all services:

docker compose up --build

Services will be available at:

| Service | URL |
|------|------|
| Gateway | http://localhost:5054 |
| Auth API | http://localhost:5194 |
| SQL Server | localhost:1433 |

The first startup may take **10–20 seconds** while SQL Server initializes.

---

# Database

SQL Server runs inside Docker.

Data is persisted using a Docker volume:

sql-auth-data

To completely reset the database:

docker compose down -v

This removes the containers **and the database volume**.

---

# Running Services Individually (Optional)

You can also run services individually during development.

Start SQL Server:

docker compose up sql-auth

Run backend services locally:

dotnet run

Run the SPA:

npm run dev

---

---

# .NET User Secrets (Local Development)

When running backend services with `dotnet run`, secrets are stored using **.NET user-secrets** instead of `.env` files.

User secrets are stored **outside the repository on your machine**, so sensitive values are never committed to source control.

---

## Initialize User Secrets

From the service project directory (example for the auth service):

```bash
cd src/services/auth
dotnet user-secrets init

dotnet user-secrets set "Jwt:Issuer" "AuthService"
dotnet user-secrets set "Jwt:Audience" "CommerceSpa"
dotnet user-secrets set "Jwt:Key" "YOUR_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS"
dotnet user-secrets set "ConnectionStrings:AuthDb" "Server=localhost,1433;Database=authdb;User Id=SA;Password=[AUTHDB_PASSWORD];TrustServerCertificate=true"
```


# Development Notes

- Entity Framework migrations run automatically in **Development mode**.
- Configuration values are loaded from:
  - appsettings.json
  - appsettings.Development.json
  - Environment variables
  - .NET user secrets (when using `dotnet run`)

Secrets are **never committed to Git**.

---

# Recommended Workflow

1. Create `.secrets` directory
2. Add the required `.env` files
3. Start services

docker compose up --build

---

# Resetting the Environment

To fully reset the development environment:

docker compose down -v  
docker compose up --build

This recreates containers and resets the database.

---

# Configuration & Secrets Management

Configuration is split between non-sensitive values committed in `appsettings.Development.json` and secrets which are never committed.

There are two pathways depending on how you run the app.

---

## Pathway 1 — Docker Compose (.secrets/*.env files)

Used when running via `docker compose up`. Each service reads its secrets from a `.env` file in the `.secrets/` directory (gitignored).

### .secrets/auth.env
```
ConnectionStrings__AuthDb=
Jwt__Key=
AdminSeed__Email=
AdminSeed__Password=
Turnstile__SecretKey=
```

### .secrets/gateway.env
```
Jwt__Key=
```

### .secrets/orders.env
```
ConnectionStrings__OrdersDb=
Jwt__Key=
Turnstile__SecretKey=
```

### .secrets/payments.env
```
ConnectionStrings__PaymentsDb=
Stripe__SecretKey=
Stripe__WebhookSecret=
Turnstile__SecretKey=
```

### .secrets/inventory.env
```
ConnectionStrings__InventoryDb=
CLOUDINARY_URL=
```

### .secrets/notifications.env
```
Mailgun__ApiKey=
```

### .secrets/sql-auth.env
### .secrets/sql-inventory.env
### .secrets/sql-orders.env
### .secrets/sql-payments.env
```
MSSQL_SA_PASSWORD=
```

---

## Pathway 2 — dotnet run (.NET User Secrets)

Used when running services individually with `dotnet run`. Secrets are stored outside the repo via the .NET user-secrets system (single colon separator instead of double underscore).

```bash
cd src/services/auth
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AuthDb" ""
dotnet user-secrets set "Jwt:Key" ""
dotnet user-secrets set "AdminSeed:Email" ""
dotnet user-secrets set "AdminSeed:Password" ""
dotnet user-secrets set "Turnstile:SecretKey" ""
```

```bash
cd src/services/orders
dotnet user-secrets set "ConnectionStrings:OrdersDb" ""
dotnet user-secrets set "Jwt:Key" ""
dotnet user-secrets set "Turnstile:SecretKey" ""
```

```bash
cd src/services/payments
dotnet user-secrets set "ConnectionStrings:PaymentsDb" ""
dotnet user-secrets set "Stripe:SecretKey" ""
dotnet user-secrets set "Stripe:WebhookSecret" ""
dotnet user-secrets set "Turnstile:SecretKey" ""
```

```bash
cd src/services/inventory
dotnet user-secrets set "ConnectionStrings:InventoryDb" ""
dotnet user-secrets set "CLOUDINARY_URL" ""
```

```bash
cd src/services/notifications
dotnet user-secrets set "Mailgun:ApiKey" ""
```

```bash
cd src/services/gateway
dotnet user-secrets set "Jwt:Key" ""
```

---

## Frontend (Vite)

`src/client/.env.development` is committed and contains non-sensitive dev config.

`src/client/.env.development` contains:

```
VITE_TURNSTILE_SITE_KEY_MANAGED=    ← public, safe to commit
VITE_STRIPE_PUBLISHABLE_KEY=        ← public, safe to commit
```

For local overrides, create `src/client/.env.local` (gitignored):

```
VITE_TURNSTILE_SITE_KEY_MANAGED=
VITE_STRIPE_PUBLISHABLE_KEY=
```

In CI/production, set these as environment variables before running `npm run build`.

---

# Production Environment Strategy

## V1 — Single VM (Docker Compose)

The simplest viable deployment. Everything runs on one machine, exactly like local dev but on a cloud VM (EC2, Azure VM, DigitalOcean Droplet etc).

All containers share the same Docker network so they reach each other by container name — same as local dev. RabbitMQ, SQL Server, and all services are just containers in the same Compose stack. No cloud-specific infrastructure knowledge required.

**Rough cost:** ~$20–40/month for a VM with enough resources (2–4 vCPU, 4–8GB RAM).

**Tradeoffs:**
- If the VM goes down, everything goes down simultaneously
- Scaling means resizing the VM, not adding instances
- Completely fine for a boutique app with low, predictable traffic

---

## V2 — Azure Managed Services + Terraform

Spread the app across proper Azure managed services. The V1 → V2 migration is a natural learning project — you have a working mental model of what the infrastructure needs to do (from V1), and V2 gives you a reason to learn Azure and Terraform properly.

### Azure services map

| Component | Azure Service | Notes |
|---|---|---|
| 4 services + gateway | Container Apps | Managed containers, scales to zero, built-in load balancing |
| React SPA | Static Web Apps | Free tier, global CDN, built-in HTTPS |
| SQL Server (per service) | Azure SQL | Managed SQL Server |
| RabbitMQ | Azure Service Bus | Swap `IMessagePublisher` implementation. CloudAMQP is a zero-code-change alternative. |
| Docker images | Container Registry | Stores built images |
| Secrets | Key Vault | Connection strings, JWT secret, Stripe keys |
| Monitoring | Application Insights | Replaces Console.WriteLines |

### Traffic flow
```
Browser
  ↓
Static Web Apps (React SPA)
  ↓ API calls
Container Apps — Gateway (YARP)   ← only internet-facing entry point
  ↓ internal routing
Container Apps — Auth / Orders / Inventory / Payments
  ↓                    ↓
Azure SQL         Service Bus
```

### Why Container Apps scale-to-zero matters
Container Apps can spin down idle services and spin them back up on demand. For a boutique app that's quiet overnight, this means you're not paying for compute when nobody's using the site. Services are already stateless so scale-out (multiple instances under load) also works without code changes.

### Terraform
Manages all interconnected Azure resources as code. Benefits:
- Spin up a full staging environment with one command
- Tear it down when not in use (cost saving)
- Track infrastructure changes in git alongside app code
- Reproducible — no clicking around the Azure portal

**Rough cost:** ~$35–55/month, potentially less with scale-to-zero at low traffic.

### Migration path V1 → V2
1. Ship V1 on a single VM, validate the app works in production
2. Set up Azure Container Registry, push images via GitHub Actions
3. Write Terraform to provision Azure resources starting with a staging environment
4. Migrate databases to Azure SQL
5. Migrate messaging to Azure Service Bus (swap `IMessagePublisher` implementation)
6. Deploy services to Container Apps, Static Web Apps for the frontend
7. Cut over DNS, decommission the VM