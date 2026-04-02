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

## Hosting Options

### Option 1 — Single VM + Cloudflare (Recommended for V1)

Everything runs on one machine, exactly like local dev but on a cloud VM (EC2, Azure VM, DigitalOcean Droplet etc). An Nginx container serves the React SPA static build, YARP handles all API traffic. Cloudflare sits in front as a reverse proxy — handling HTTPS, DDoS protection, and CDN edge caching — so the VM itself only needs to speak HTTP internally.

All containers share the same Docker network so they reach each other by container name — same as local dev. No cloud-specific infrastructure knowledge required.

**Cost:** ~$20–40/month VM + domain (see Domain Progression below)

**Deploys:** GitHub Actions → SSH → `docker compose up --build`. Brief downtime on redeploy, acceptable for low-traffic boutique use.

**SEO:** React SPA. Google does execute JavaScript and will index the site, but with a slight delay versus server-rendered HTML. Not a meaningful issue for a local boutique hire business with low search competition.

**Tradeoffs:**
- If the VM goes down, everything goes down simultaneously
- Scaling means resizing the VM, not adding instances
- Microservice boundaries are enforced in code but not exploited operationally — you don't get independent scaling or independent deploys until V2
- Completely fine for a boutique app with low, predictable traffic

---

### Option 2 — Single VM + Vercel (frontend) + Cloudflare

Same VM for all backend services, React SPA hosted on Vercel instead of served from the VM. Vercel auto-deploys on push to main with zero downtime and handles its own HTTPS and DDoS protection. Cloudflare is still required in front of the VM to protect the backend API.

**Cost:** Same as Option 1, Vercel free tier adds nothing

**Deploys:** Frontend auto-deploys via Vercel on push. Backend still requires SSH/scripted deploy.

**SEO:** No meaningful difference from Option 1 — still a client-rendered SPA.

**Why not chosen:** Cloudflare's edge caching already covers the CDN benefit Vercel provides for static assets. Adding Vercel introduces a second external service and splits the deploy story without a meaningful gain.

---

### Option 3 — Single VM + Next.js (SSR) + Cloudflare

Migrate the React SPA to Next.js, which acts purely as a server-side rendering layer over the existing .NET backend. Next.js renders HTML on the server before sending it to the browser — product pages, home page etc arrive fully rendered. The .NET microservices are unchanged; Next.js replaces the Vite SPA as the frontend only.

**Cost:** Same as Option 2

**Deploys:** Same as Option 2

**SEO:** Best option — fully server-rendered HTML for all public pages. Crawlers receive content immediately with no JavaScript execution required.

**Why not chosen:** Requires a full frontend rewrite (TanStack Router → Next.js file routing, client-side fetching → Server Components, auth cookie handling rearchitected for SSR). The SEO benefit does not justify this effort for a local boutique hire business competing against other small local operators — not high-volume keyword competition where crawl timing materially affects rankings.

---

## V1 — Single VM (Docker Compose)

**Chosen approach: Option 1.**

**Rough cost:** ~$20–40/month for a VM with enough resources (2–4 vCPU, 4–8GB RAM).

### HTTPS and DDoS protection

Cloudflare acts as a reverse proxy in front of the VM. Point the domain's DNS A record at the VM IP with Cloudflare's proxy enabled (orange cloud). Cloudflare terminates HTTPS from the browser and forwards traffic to the VM. For full end-to-end encryption, use a Cloudflare Origin Certificate on the VM with SSL mode set to Full (Strict). Origin Certificates are free and valid for 15 years — no renewal overhead.

Cloudflare free tier includes:
- HTTPS/TLS termination
- DDoS protection
- CDN edge caching
- Real VM IP hidden from public DNS

### CI/CD

```
Push to main → GitHub Actions builds images → SSH into VM → docker compose pull + up
```

Brief downtime on redeploy. Acceptable for V1 traffic levels.

---

## V2 — Azure Managed Services + Terraform

Spread the app across proper Azure managed services. The V1 → V2 migration is a natural learning project — you have a working mental model of what the infrastructure needs to do (from V1), and V2 gives you a reason to learn Azure and Terraform properly.

### Azure services map

| Component | Azure Service | Notes |
|---|---|---|
| 4 services + gateway | Container Apps | Managed containers, scales to zero, built-in load balancing |
| React SPA | Static Web Apps | Free tier, global CDN, built-in HTTPS |
| SQL Server (per service) | Azure SQL | Managed SQL Server |
| RabbitMQ | Azure Service Bus | Swap `IMessagePublisher` implementation |
| Docker images | Container Registry | Stores built images |
| Secrets | Key Vault | Connection strings, JWT secret, Stripe keys |
| Monitoring | Application Insights | Centralised logging, distributed tracing, alerting |

### Application Insights
Application Insights is Azure's application performance monitoring (APM) service. In V2 it replaces `Console.WriteLine` with a proper observability stack:

- **Centralised log dashboard** — logs from all 5 services in one place, queryable via Kusto (KQL). No more SSHing into a VM and running `docker logs`.
- **Distributed tracing** — a single incoming request (e.g. place order) can be traced end-to-end across Gateway → Orders → Inventory → Payments → Notifications, with per-service timing.
- **Alerting** — set alerts on error rates, latency spikes, or specific log messages (e.g. page if `PaymentFailed` events exceed a threshold).
- **Metrics** — request rates, failure rates, response times, dependency call durations — all out of the box.

**How it wires in:**

The V1 checklist includes migrating from `Console.WriteLine` to `ILogger`. Once that's done, adding Application Insights in V2 is a single NuGet package + one line in each `Program.cs`:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

All existing `ILogger` calls automatically flow into Application Insights — no other code changes required. The connection string is stored in Key Vault and injected via the Container Apps environment config.

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

### CI/CD

```
Push to main → GitHub Actions builds images → push to Container Registry → az containerapp update → rolling deploy
```

Each service deploys independently with zero downtime. This is where the microservices architecture pays its operational dividends — independent scaling, independent deploys, fault isolation.

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

---

---

# Production Deployment Setup

This section documents the one-time setup steps required to get the production stack running on an Azure VM behind Cloudflare.

---

## 1. Provision the Azure VM

1. Create a VM in the Azure portal (B2als v2 or equivalent — 2 vCPU, 4GB RAM minimum given 4x SQL Server containers)
2. Choose Ubuntu as the OS
3. Enable SSH public key authentication — save the private key
4. Under **Networking**, add inbound NSG rules:
   - Port **22** (SSH) — already added by default
   - Port **80** (HTTP) — for nginx redirect to HTTPS
   - Port **443** (HTTPS) — for nginx SSL termination

---

## 2. Install Docker on the VM

SSH into the VM and run:

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
exit
```

Log back in — Docker Compose v2 is included with the official Docker install, no separate install needed.

---

## 3. Create the Secrets Directory

```bash
sudo mkdir -p /opt/teacupboutique/.secrets
sudo chown $USER:$USER /opt/teacupboutique
```

Populate each secrets file (never committed to the repo):

**`/opt/teacupboutique/.secrets/auth.env`**
```
ConnectionStrings__AuthDb=Server=sql-auth,1433;Database=authdb;User Id=SA;Password=...;TrustServerCertificate=true
Jwt__Key=
AdminSeed__Email=
AdminSeed__Password=
Turnstile__SecretKey=
```

**`/opt/teacupboutique/.secrets/gateway.env`**
```
Jwt__Key=
```

**`/opt/teacupboutique/.secrets/inventory.env`**
```
ConnectionStrings__InventoryDb=Server=sql-inventory,1433;Database=inventorydb;User Id=SA;Password=...;TrustServerCertificate=true
CLOUDINARY_URL=
```

**`/opt/teacupboutique/.secrets/orders.env`**
```
ConnectionStrings__OrdersDb=Server=sql-orders,1433;Database=ordersdb;User Id=SA;Password=...;TrustServerCertificate=true
Jwt__Key=
Turnstile__SecretKey=
```

**`/opt/teacupboutique/.secrets/payments.env`**
```
ConnectionStrings__PaymentsDb=Server=sql-payments,1433;Database=paymentsdb;User Id=SA;Password=...;TrustServerCertificate=true
Stripe__SecretKey=
Stripe__WebhookSecret=
Turnstile__SecretKey=
```

**`/opt/teacupboutique/.secrets/notifications.env`**
```
Mailgun__ApiKey=
```

**`/opt/teacupboutique/.secrets/sql-auth.env`**
**`/opt/teacupboutique/.secrets/sql-inventory.env`**
**`/opt/teacupboutique/.secrets/sql-orders.env`**
**`/opt/teacupboutique/.secrets/sql-payments.env`**
```
MSSQL_SA_PASSWORD=
```

The SA password in each sql-*.env must match the password in the corresponding service connection string.

---

## 4. Cloudflare DNS and SSL

### DNS Record

In the Cloudflare dashboard for your domain:

- Add an **A record**: Name = `teacupboutique`, Content = VM public IP, Proxy status = **Proxied** (orange cloud)

This routes `teacupboutique.lbirchen.com` through Cloudflare's proxy, hiding the VM's real IP and enabling DDoS protection and CDN caching.

### SSL/TLS Mode

Cloudflare dashboard → your domain → **SSL/TLS** → set mode to **Full (strict)**.

This means:
- Browser → Cloudflare: HTTPS (Cloudflare's public cert)
- Cloudflare → VM: HTTPS (Cloudflare Origin Certificate)

Do not use Flexible — it sends HTTP to the VM and causes redirect loops with nginx.

### Cloudflare Origin Certificate

Cloudflare dashboard → **SSL/TLS** → **Origin Server** → **Create Certificate**:

- Key type: RSA
- Hostnames: `teacupboutique.lbirchen.com`, `*.lbirchen.com` (or just the subdomain)
- Validity: 15 years

Copy the certificate and private key. Save them on the VM:

```bash
nano /opt/teacupboutique/.secrets/cloudflare-origin.crt   # paste cert
nano /opt/teacupboutique/.secrets/cloudflare-origin.key   # paste key
```

These are mounted read-only into the nginx container via `docker-compose.prod.yml`.

---

## 5. GitHub Actions Secrets

In the repository: **Settings → Secrets and variables → Actions → New repository secret**

| Secret | Value |
|--------|-------|
| `SSH_HOST` | VM public IP |
| `SSH_USER` | VM username (e.g. `azureuser`) |
| `SSH_PRIVATE_KEY` | Private key matching the VM's `authorized_keys` |

The pipeline authenticates to GHCR using `GITHUB_TOKEN` (automatic, no setup needed).

---

## 6. GHCR Authentication on the VM

The VM needs to pull Docker images from GitHub Container Registry. Create a GitHub Personal Access Token (classic) with `read:packages` scope, then on the VM:

```bash
echo "<PAT>" | docker login ghcr.io -u <github-username> --password-stdin
```

Alternatively, make all GHCR packages public (safe since images contain no secrets — all secrets are injected at runtime via env files).

---

## 7. Deploy

Push to `main`. GitHub Actions will:

1. Build all service images and push to GHCR
2. SCP `docker-compose.prod.yml` and `nginx/nginx.conf` to the VM
3. SSH into the VM, start infrastructure containers, wait for SQL Server to be ready
4. Run EF Core migrations via the `efbundle` baked into each service image
5. Bring up all services with `docker compose up -d`

The nginx container handles:
- Port 80 → 301 redirect to HTTPS
- Port 443 → SSL termination using the Cloudflare Origin Certificate → proxy to the React client container
- `/api/*`, `/payments/*`, `/webhooks/*` → proxied to the gateway

---

## Domain Progression

### Showcase (V1)

No new domain required. Add a subdomain A record to an existing Cloudflare-managed domain pointing at the VM IP, with the proxy toggled on:

```
teacupboutique.yourdomain.com  →  VM IP  (Cloudflare proxy on)
```

Each DNS record in Cloudflare has an independent proxy toggle — the subdomain being proxied has no effect on the main domain's configuration.

### Commercial (V2)

Buy a dedicated domain (e.g. `teacupboutique.com`) through Cloudflare at cost (~$10–15/year, no registrar markup). Point it at the V2 infrastructure.

Two config changes required in the codebase when cutting over:
1. **CORS** — update allowed origins in the gateway config
2. **Frontend** — update `VITE_API_URL` env var in the build pipeline

The showcase subdomain can remain live or be retired.