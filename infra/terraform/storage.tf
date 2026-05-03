locals {
  data_protection_container_name = "dataprotection"
  auth_data_protection_blob_name = "auth-key-ring.xml"
}

resource "azurerm_storage_account" "dataprotection" {
  name                     = "teacupboutiquedpkeys"
  resource_group_name      = azurerm_resource_group.teacupboutique.name
  location                 = azurerm_resource_group.teacupboutique.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  allow_nested_items_to_be_public = false
}

resource "azurerm_storage_container" "dataprotection" {
  name                  = local.data_protection_container_name
  storage_account_id    = azurerm_storage_account.dataprotection.id
  container_access_type = "private"
}

resource "azurerm_role_assignment" "container_apps_data_protection_blob" {
  scope                = azurerm_storage_account.dataprotection.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}
