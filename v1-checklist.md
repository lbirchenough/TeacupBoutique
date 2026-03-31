# v1 Pre-Deployment Checklist

## Critical (Security / Data Integrity)
- [x] Fix inventory race condition (atomic stock check + booking creation)
- [x] Add price validation on order submission against DB product prices
- [x] Fix CAPTCHA fail-open (TurnstileService should fail closed when Cloudflare unreachable)
- [x] Implement refund pathway for cancelled-after-payment orders

## High Priority (Production Readiness)
- [x] Make CORS configurable per environment
- [x] Move JWT/refresh token expiry to config
- [x] Remove WeatherForecast template controller from auth service
- [ ] Add production migration strategy — add a migration step to the GitHub Actions deploy pipeline that runs `dotnet ef database update` via a short-lived container for each service before deploying updated service images. Auto-migrate on startup stays in place for local dev. This is implemented as part of setting up the VM and CI/CD pipeline, not before.
- [x] Replace Console.WriteLine with ILogger throughout all services

## Medium Priority (Functionality Gaps)
- [x] Add admin refund/manual refund trigger endpoint
- [x] Return DTO from GET /api/products/{id} instead of full entity
- [x] Remove commented-out code (OrdersController update endpoint)

## Lower Priority (Polish / Maintenance)
- [ ] Add test coverage for critical paths (JWT flow, booking logic, Stripe webhooks)
- [ ] Tighten password strength requirements
- [ ] Add TTL/cleanup job for StripeEvents deduplication table
- [ ] Standardize OpenAPI/Swagger across all services
- [ ] Write production deployment documentation
