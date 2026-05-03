# Generated secrets — Terraform creates strong random values and writes them to Key Vault.
# Values only exist in Terraform state (encrypted in Azure Storage) and Key Vault.
# Externally-provided secrets (Stripe, Mailgun, Turnstile) are set manually via `az keyvault secret set`.

resource "random_password" "postgres_admin" {
  length           = 32
  special          = true
  override_special = "!#%&*+-._~"
  min_lower        = 2
  min_upper        = 2
  min_numeric      = 2
  min_special      = 2
}

resource "random_password" "jwt_signing_key" {
  length  = 64
  special = false
}

resource "random_password" "admin_seed_password" {
  length           = 32
  special          = true
  override_special = "!#%&*+-._~"
  min_lower        = 2
  min_upper        = 2
  min_numeric      = 2
  min_special      = 2
}

resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name         = "postgres-admin-password"
  value        = random_password.postgres_admin.result
  key_vault_id = azurerm_key_vault.teacupboutique.id

  depends_on = [azurerm_role_assignment.kv_admin_self]
}

resource "azurerm_key_vault_secret" "jwt_signing_key" {
  name         = "jwt-signing-key"
  value        = random_password.jwt_signing_key.result
  key_vault_id = azurerm_key_vault.teacupboutique.id

  depends_on = [azurerm_role_assignment.kv_admin_self]
}

resource "azurerm_key_vault_secret" "admin_seed_password" {
  name         = "admin-seed-password"
  value        = random_password.admin_seed_password.result
  key_vault_id = azurerm_key_vault.teacupboutique.id

  depends_on = [azurerm_role_assignment.kv_admin_self]
}

# Per-service Postgres connection strings. Each is the full Npgsql string
# the .NET apps already expect — no code change needed.
locals {
  postgres_connection_strings = {
    authdb      = "Host=${azurerm_postgresql_flexible_server.teacupboutique.fqdn};Database=authdb;Username=tcbadmin;Password=${random_password.postgres_admin.result};Sslmode=Require;Trust Server Certificate=true"
    inventorydb = "Host=${azurerm_postgresql_flexible_server.teacupboutique.fqdn};Database=inventorydb;Username=tcbadmin;Password=${random_password.postgres_admin.result};Sslmode=Require;Trust Server Certificate=true"
    ordersdb    = "Host=${azurerm_postgresql_flexible_server.teacupboutique.fqdn};Database=ordersdb;Username=tcbadmin;Password=${random_password.postgres_admin.result};Sslmode=Require;Trust Server Certificate=true"
    paymentsdb  = "Host=${azurerm_postgresql_flexible_server.teacupboutique.fqdn};Database=paymentsdb;Username=tcbadmin;Password=${random_password.postgres_admin.result};Sslmode=Require;Trust Server Certificate=true"
  }
}

resource "azurerm_key_vault_secret" "postgres_connection_strings" {
  for_each     = local.postgres_connection_strings
  name         = "connstr-${each.key}"
  value        = each.value
  key_vault_id = azurerm_key_vault.teacupboutique.id

  depends_on = [azurerm_role_assignment.kv_admin_self]
}
