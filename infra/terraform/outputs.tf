output "acr_login_server" {
  description = "Azure Container Registry login server used by CI/CD image pushes."
  value       = azurerm_container_registry.teacupboutique.login_server
}

output "frontend_url" {
  description = "Public frontend URL. Defaults to the Azure Static Web Apps generated hostname unless frontend_url_override is set."
  value       = local.frontend_url
}

output "gateway_url" {
  description = "Public gateway URL for API calls."
  value       = "https://${azurerm_container_app.gateway.ingress[0].fqdn}"
}

output "github_actions_azure_client_id" {
  description = "Client ID to store in GitHub secret AZURE_CLIENT_ID for OIDC Azure login."
  value       = azuread_application.github_actions.client_id
}

output "github_actions_azure_tenant_id" {
  description = "Tenant ID to store in GitHub secret AZURE_TENANT_ID for OIDC Azure login."
  value       = data.azurerm_client_config.current.tenant_id
}

output "github_actions_azure_subscription_id" {
  description = "Subscription ID to store in GitHub secret AZURE_SUBSCRIPTION_ID for OIDC Azure login."
  value       = data.azurerm_subscription.current.subscription_id
}
