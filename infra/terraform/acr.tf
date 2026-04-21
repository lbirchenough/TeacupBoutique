resource "azurerm_container_registry" "teacupboutique" {
  name                = "teacupboutiqueacr"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  sku                 = "Basic"
  admin_enabled       = false
}