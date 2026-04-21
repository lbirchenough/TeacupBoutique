data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "teacupboutique" {
  name                = "${var.project}-kv"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  rbac_authorization_enabled = true
  purge_protection_enabled   = false
  soft_delete_retention_days = 7
}

resource "azurerm_role_assignment" "kv_admin_self" {
  scope                = azurerm_key_vault.teacupboutique.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}
