locals {
  postgres_databases = ["authdb", "inventorydb", "ordersdb", "paymentsdb"]
}

resource "azurerm_postgresql_flexible_server" "teacupboutique" {
  name                = var.project
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location

  version      = "16"
  sku_name     = "B_Standard_B1ms"
  storage_mb   = 32768
  storage_tier = "P4"
  zone         = "1"

  # VNet-integrated — server NIC lives inside snet-postgres.
  # No public IP, no firewall rule needed.
  public_network_access_enabled = false
  delegated_subnet_id           = azurerm_subnet.postgres.id
  private_dns_zone_id           = azurerm_private_dns_zone.postgres.id

  administrator_login    = "tcbadmin"
  administrator_password = random_password.postgres_admin.result

  authentication {
    password_auth_enabled = true
  }

  lifecycle {
    prevent_destroy = true
  }

  depends_on = [azurerm_private_dns_zone_virtual_network_link.postgres]
}

resource "azurerm_postgresql_flexible_server_database" "dbs" {
  for_each  = toset(local.postgres_databases)
  name      = each.value
  server_id = azurerm_postgresql_flexible_server.teacupboutique.id
  charset   = "UTF8"
  collation = "en_US.utf8"

  lifecycle {
    prevent_destroy = true
  }
}
