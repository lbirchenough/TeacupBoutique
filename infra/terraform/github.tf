# Repository secrets read by .github/workflows/deploy-v2.yml. Terraform writes
# them so a rebuild (new OIDC app registration, new Static Web App) doesn't
# leave GitHub Actions with stale credentials.
locals {
  github_actions_secrets = {
    AZURE_CLIENT_ID                 = azuread_application.github_actions.client_id
    AZURE_TENANT_ID                 = data.azurerm_client_config.current.tenant_id
    AZURE_SUBSCRIPTION_ID           = data.azurerm_subscription.current.subscription_id
    AZURE_STATIC_WEB_APPS_API_TOKEN = azurerm_static_web_app.frontend.api_key
  }
}

resource "github_actions_secret" "deploy" {
  for_each    = nonsensitive(toset(keys(local.github_actions_secrets)))
  repository  = var.github_repo
  secret_name = each.key
  value       = local.github_actions_secrets[each.key]
}
