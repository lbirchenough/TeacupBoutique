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

# Security

Sensitive configuration such as:

- database credentials
- JWT signing keys
- environment-specific configuration

is stored **locally in `.env` files or .NET user secrets**, and excluded from source control via `.gitignore`.