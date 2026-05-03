data "azurerm_subscription" "current" {}

locals {
  github_federated_credential_name = replace("${var.github_repo}-${var.github_branch}", "/", "-")
}

resource "azuread_application" "github_actions" {
  display_name = "${var.project}-github-actions"
}

resource "azuread_service_principal" "github_actions" {
  client_id = azuread_application.github_actions.client_id
}

resource "azuread_application_federated_identity_credential" "github_actions_branch" {
  application_id = azuread_application.github_actions.id
  display_name   = local.github_federated_credential_name
  description    = "Allows GitHub Actions from ${var.github_owner}/${var.github_repo}:${var.github_branch} to deploy TeacupBoutique v2."
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.github_owner}/${var.github_repo}:ref:refs/heads/${var.github_branch}"
}

resource "azurerm_role_assignment" "github_actions_rg_contributor" {
  scope                = azurerm_resource_group.teacupboutique.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.github_actions.object_id
}

resource "azurerm_role_assignment" "github_actions_acr_push" {
  scope                = azurerm_container_registry.teacupboutique.id
  role_definition_name = "AcrPush"
  principal_id         = azuread_service_principal.github_actions.object_id
}
