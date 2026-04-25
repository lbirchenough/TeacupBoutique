resource "azurerm_static_web_app" "frontend" {
  name                = "${var.project}-frontend"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = var.static_web_app_location
  sku_tier            = "Free"
  sku_size            = "Free"
}

locals {
  frontend_url = coalesce(
    var.frontend_url_override,
    "https://${azurerm_static_web_app.frontend.default_host_name}"
  )
}
