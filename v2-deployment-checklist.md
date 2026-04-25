# v2 Azure Deployment Checklist

## Current State

- [x] Provision Azure resource group, Log Analytics workspace, VNet, subnets, and private DNS
- [x] Provision Azure Container Registry
- [x] Provision one PostgreSQL Flexible Server with `authdb`, `inventorydb`, `ordersdb`, and `paymentsdb`
- [x] Provision Key Vault with generated Postgres, JWT, and admin seed secrets
- [x] Provision Azure Service Bus namespace, topics, subscriptions, and Stripe webhook queue
- [x] Provision Container Apps Environment
- [x] Provision six Container Apps: `gateway`, `auth`, `inventory`, `orders`, `payments`, and `notifications`
- [x] Configure Container Apps managed identity access to ACR, Key Vault, and Service Bus
- [x] Add Service Bus messaging implementation alongside RabbitMQ

## Infrastructure Remaining

- [x] Add Azure Static Web App Terraform resource for the React frontend
- [ ] Decide frontend/custom domain shape for v2
- [ ] Decide gateway API domain shape: default Container App FQDN or custom API domain
- [x] Add Terraform outputs for important deployment values: ACR login server, gateway URL, and Static Web App frontend URL
- [ ] Review Container Apps replica settings before launch, especially `min_replicas = 0` for public-facing services
- [ ] Decide whether Service Bus Standard public endpoint is acceptable for v2, or document Premium/private endpoint as a future hardening item

## Secrets and Runtime Configuration

- [ ] Manually populate externally-owned Key Vault secrets: `cloudinary-url`, `mailgun-api-key`, `stripe-secret-key`, `stripe-webhook-secret`, and `turnstile-secret-key`
- [ ] Verify all services receive matching JWT config: `Jwt__Key`, `Jwt__Issuer`, and `Jwt__Audience`
- [ ] Verify `gateway` CORS uses the final Static Web App/custom frontend URL
- [ ] Verify `auth` `ClientUrl` and `notifications` `FrontendUrl` use the final frontend URL for email links
- [ ] Verify frontend production config uses the v2 gateway URL through `VITE_API_URL`
- [ ] Verify Turnstile allowed hostnames include the v2 frontend domain
- [ ] Verify Stripe webhook endpoint points to the v2 gateway URL: `/webhooks/stripe`

## Backend CI/CD

- [x] Create a new v2 GitHub Actions workflow separate from the existing V1 VM/GHCR/SSH workflow
- [x] Add Azure login via GitHub OIDC instead of long-lived credentials
- [x] Build backend images for `gateway`, `auth`, `inventory`, `orders`, `payments`, and `notifications`
- [x] Push backend images to Azure Container Registry
- [x] Use immutable image tags, preferably the Git SHA, instead of relying only on `latest`
- [x] Update each Azure Container App to the newly pushed image
- [ ] Add environment protection or branch filtering so V1 and V2 workflows do not both deploy unintentionally

## Database Migrations

- [x] Choose Azure migration execution strategy
- [x] Prefer Container Apps Jobs for EF migration bundles for `auth`, `inventory`, `orders`, and `payments`
- [x] Give migration jobs the same managed identity, Key Vault access, VNet access, and connection-string secrets as their matching services
- [x] Run migration jobs before updating service Container Apps during deployment
- [x] Confirm failed migrations stop the deployment before app rollout

## Frontend CI/CD

- [x] Create Static Web App deployment workflow for `src/client`
- [x] Use committed `.env.production` for frontend build values: `VITE_API_URL`, `VITE_TURNSTILE_SITE_KEY_MANAGED`, and `VITE_STRIPE_PUBLISHABLE_KEY`
- [x] Decide whether frontend deployment is triggered by the same workflow as backend or by a separate workflow
- [ ] Verify client-side routes work correctly on Static Web App refresh/deep links

## End-to-End Validation

- [ ] Smoke test gateway routing to `auth`, `inventory`, `orders`, and `payments`
- [ ] Smoke test login/register/email verification flow
- [ ] Smoke test product browsing and admin product management
- [ ] Smoke test booking/order creation through gateway
- [ ] Smoke test Service Bus flow: order placed to inventory reservation
- [ ] Smoke test Service Bus flow: ready for payment to payments service
- [ ] Smoke test Stripe payment succeeded and failed webhook processing
- [ ] Smoke test notifications for order, auth, and payment events
- [ ] Confirm private Postgres connectivity from Container Apps and migration jobs
- [ ] Confirm no service depends on RabbitMQ in v2 production config

## Observability and Operations

- [ ] Confirm Container App logs are flowing to Log Analytics
- [ ] Add Application Insights/OpenTelemetry integration or document as post-launch hardening
- [ ] Add basic health checks or operational smoke-test commands
- [ ] Document rollback process for Container App revisions/images
- [ ] Document how to inspect failed Service Bus messages and dead-letter queues
- [ ] Document how to rerun migrations safely

## Cutover

- [ ] Deploy backend to Azure Container Apps from the v2 workflow
- [ ] Deploy frontend to Static Web App
- [ ] Configure final DNS/custom domains
- [ ] Update Stripe webhook endpoint to v2
- [ ] Update Turnstile hostnames to v2
- [ ] Verify CORS and email links after DNS propagation
- [ ] Run full production smoke test
- [ ] Decide when to disable or retire the V1 VM deployment workflow
