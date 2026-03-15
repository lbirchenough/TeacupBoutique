# Branch: feature/gateway-auth

## What this branch does
Routes all frontend traffic through the API gateway (single entry point on port 5054) and adds JWT-based authorization to booking endpoints, validated at the gateway level only — no changes to downstream services.

---

## Changes made

### Gateway
- `src/gateway/appsettings.Development.json` — added routes and clusters for inventory, orders, and payments. Booking route has `"AuthorizationPolicy": "default"` so YARP rejects unauthenticated requests with 401 before forwarding.
- `src/gateway/Program.cs` — added JWT Bearer authentication and `UseAuthentication()` / `UseAuthorization()` middleware wired into YARP.
- `src/gateway/gateway.csproj` — added `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet package.
- `docker-compose.override.yml` — added cluster destination overrides for inventory, orders, payments; added `env_file` reference to `.secrets/gateway.env`.
- `.secrets/gateway.env` *(gitignored)* — JWT key/issuer/audience for Docker. Run `dotnet user-secrets` commands below for local dev.

### Frontend
- `src/client/src/lib/inventoryApi.ts` — base URL fallback changed from `localhost:5035` to `localhost:5054` (gateway).
- `src/client/src/lib/ordersApi.ts` — base URL fallback changed from `localhost:5079` to `localhost:5054`.
- `src/client/src/lib/paymentsApi.ts` — base URL fallback changed from `localhost:5127` to `localhost:5054`.
- `src/client/src/lib/bookingsApi.ts` — base URL updated to gateway + added `Authorization: Bearer <token>` header to all 5 booking calls via `authHeaders()` helper pulling from `authStore`.

### Also on this branch (committed separately)
- `src/services/inventory/Program.cs` — auto-migrate on startup in Development.
- `src/services/orders/Program.cs` — auto-migrate on startup in Development.
- `src/services/payments/Program.cs` — auto-migrate on startup in Development.

---

## Setup required after cloning (local dev)

The `.secrets/` folder is gitignored. Recreate it:

```bash
# SQL Server passwords
echo "MSSQL_SA_PASSWORD=@Password1" > .secrets/sql-auth.env
echo "MSSQL_SA_PASSWORD=@Password1" > .secrets/sql-inventory.env
echo "MSSQL_SA_PASSWORD=@Password1" > .secrets/sql-orders.env
echo "MSSQL_SA_PASSWORD=@Password1" > .secrets/sql-payments.env

# Service connection strings
cat > .secrets/auth.env <<EOF
ConnectionStrings__AuthDb=Server=sql-auth,1433;Database=authdb;User Id=SA;Password=@Password1;TrustServerCertificate=true
Jwt__Key=super-secret-dev-key-change-in-production
Jwt__Issuer=AuthService
Jwt__Audience=CommerceSpa
EOF

cat > .secrets/inventory.env <<EOF
ConnectionStrings__InventoryDb=Server=sql-inventory,1433;Database=inventorydb;User Id=SA;Password=@Password1;TrustServerCertificate=true
RabbitMq__Host=rabbitmq
EOF

cat > .secrets/orders.env <<EOF
ConnectionStrings__OrdersDb=Server=sql-orders,1433;Database=ordersdb;User Id=SA;Password=@Password1;TrustServerCertificate=true
RabbitMq__Host=rabbitmq
EOF

cat > .secrets/payments.env <<EOF
ConnectionStrings__PaymentsDb=Server=sql-payments,1433;Database=paymentsdb;User Id=SA;Password=@Password1;TrustServerCertificate=true
RabbitMq__Host=rabbitmq
Stripe__SecretKey=<your key>
Stripe__WebhookSecret=<your secret>
EOF

cat > .secrets/gateway.env <<EOF
Jwt__Key=super-secret-dev-key-change-in-production
Jwt__Issuer=AuthService
Jwt__Audience=CommerceSpa
EOF
```

dotnet user-secrets (for running services locally without Docker):
```bash
dotnet user-secrets set "ConnectionStrings:AuthDb" "Server=localhost,1433;Database=authdb;User Id=SA;Password=@Password1;TrustServerCertificate=true" --project src/services/auth
dotnet user-secrets set "Jwt:Key" "super-secret-dev-key-change-in-production" --project src/services/auth
dotnet user-secrets set "Jwt:Issuer" "AuthService" --project src/services/auth
dotnet user-secrets set "Jwt:Audience" "CommerceSpa" --project src/services/auth

dotnet user-secrets set "Jwt:Key" "super-secret-dev-key-change-in-production" --project src/gateway
dotnet user-secrets set "Jwt:Issuer" "AuthService" --project src/gateway
dotnet user-secrets set "Jwt:Audience" "CommerceSpa" --project src/gateway
```

---

## Verification

```
GET  http://localhost:5054/api/products        → 200 (inventory, no auth needed)
GET  http://localhost:5054/api/orders          → 200 (orders, no auth needed)
POST http://localhost:5054/payments/create-intent → reaches payments

GET  http://localhost:5054/api/bookings        → 401 (no token)
GET  http://localhost:5054/api/bookings        → 200 (with Authorization: Bearer <token>)
```

---

## What's left / next session
- Test end-to-end with all services running
- Consider protecting product write endpoints (POST/PUT/DELETE) with same pattern
- Consider protecting order creation for logged-in users
