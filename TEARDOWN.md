# Tearing down and rebuilding the Azure stack

Everything in `tcb-rg` is Terraform-managed (`infra/terraform/`), so the whole stack can be destroyed and recreated. The Terraform state lives in a separate resource group (`tcb-tfstate-rg`) which is **never** destroyed — that's what makes the rebuild possible.

The only things *not* automated are the Cloudflare DNS records and the two custom domains (see [Custom domains](#custom-domains-manual)).

## Prerequisites (both directions)

```bash
cd infra/terraform
az login                              # Terraform and the image builds use this session
export GITHUB_TOKEN=$(gh auth token)  # lets Terraform write the GitHub Actions secrets
```

`terraform.tfvars` (gitignored) must contain the subscription ID and the five external provider secrets — see `terraform.tfvars.example`. If you've lost it, fetch the values again from Stripe, Cloudflare Turnstile, Mailgun and Cloudinary.

## Teardown

1. Pause the deploy workflow so pushes to `main` don't trigger failing deploys:
   ```bash
   gh workflow disable "Deploy V2 to Azure"
   ```
2. In the Stripe dashboard, **disable** (don't delete) the webhook endpoint for `https://tcbapi.lbirchen.com`. Deleting it would issue a new signing secret on rebuild.
3. Destroy:
   ```bash
   terraform destroy
   ```
   Takes ~20–30 minutes (the Container Apps environment is slow to delete). If it errors on the Postgres subnet or the Container Apps environment, wait a few minutes and run it again.

**All database data is lost.** Postgres has no `prevent_destroy` on purpose.

Afterwards only `tcb-tfstate-rg` remains (a storage account, cents per month).

## Rebuild

1. Bring everything back:
   ```bash
   terraform apply
   ```
   This, in one go:
   - creates all infrastructure and writes every Key Vault secret (generated ones plus the five from `terraform.tfvars`);
   - builds all six backend images in Azure with `az acr build` and pushes them as `:latest` (`images.tf`) before the Container Apps are created — this only runs when the registry is new;
   - writes `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` and `AZURE_STATIC_WEB_APPS_API_TOKEN` to the GitHub repo secrets (`github.tf`).
2. Re-enable and run the deploy workflow with migrations, which creates the database tables (including the admin account and the four sample products) and deploys the frontend:
   ```bash
   gh workflow enable "Deploy V2 to Azure"
   gh workflow run "Deploy V2 to Azure" --ref main -f run_migrations=true
   ```
   The backend apps may restart a few times before the migrations finish — that's expected.
3. Do the [custom domains](#custom-domains-manual) steps.
4. Re-enable the Stripe webhook endpoint.

The admin password is in Key Vault: `az keyvault secret show --vault-name tcb-kv -n admin-seed-password --query value -o tsv`.

## Custom domains (manual)

The frontend is built against `https://tcbapi.lbirchen.com`, and the gateway only accepts requests from `https://tcb.lbirchen.com`, so the app doesn't work on the raw Azure URLs. After a rebuild the Azure hostnames change, so:

1. **Find the new targets**
   ```bash
   az containerapp show -n tcb-gateway -g tcb-rg --query properties.configuration.ingress.fqdn -o tsv
   az staticwebapp show -n tcb-frontend -g tcb-rg --query defaultHostname -o tsv
   az containerapp show -n tcb-gateway -g tcb-rg --query properties.customDomainVerificationId -o tsv
   ```
2. **Update Cloudflare DNS** (set both records to *DNS only* / grey cloud for now — Azure can't validate through the proxy):
   - `tcbapi` CNAME → gateway FQDN
   - `asuid.tcbapi` TXT → the verification ID (usually unchanged, it's per subscription)
   - `tcb` CNAME → Static Web App hostname
3. **Bind the domains in Azure**
   ```bash
   az containerapp hostname add  -n tcb-gateway -g tcb-rg --hostname tcbapi.lbirchen.com
   az containerapp hostname bind -n tcb-gateway -g tcb-rg --hostname tcbapi.lbirchen.com \
     --environment tcb-cae --validation-method CNAME
   az staticwebapp hostname set -n tcb-frontend -g tcb-rg --hostname tcb.lbirchen.com
   ```
   The managed certificate can take 5–15 minutes.
4. Once both show as bound, switch `tcbapi` back to *Proxied* (orange cloud) in Cloudflare.

## Why it's set up this way

- **Images are seeded by Terraform, deployed by CI.** The Container Apps can't be created without an image, and the deploy workflow can't run until they exist. `images.tf` breaks that loop once; after that GitHub Actions owns images and Terraform ignores them (`lifecycle.ignore_changes`).
- **External secrets come from `terraform.tfvars`.** Their values also end up in Terraform state, which already holds the generated secrets (DB password, JWT key) and is stored in Azure Storage.
- **No `prevent_destroy` on Postgres.** Removed deliberately so `terraform destroy` works; the cost is that destroy deletes all data.
