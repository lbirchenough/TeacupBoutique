resource "azurerm_virtual_network" "teacupboutique" {
  name                = "${var.project}-vnet"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  address_space       = ["10.10.0.0/16"]
}

# Subnet for Container Apps Environment. Delegated to Microsoft.App/environments
# because we're using a Workload Profiles environment (modern Container Apps mode).
resource "azurerm_subnet" "apps" {
  name                 = "snet-apps"
  resource_group_name  = azurerm_resource_group.teacupboutique.name
  virtual_network_name = azurerm_virtual_network.teacupboutique.name
  address_prefixes     = ["10.10.0.0/23"]

  delegation {
    name = "container-apps-delegation"
    service_delegation {
      name    = "Microsoft.App/environments"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# Subnet for Postgres Flexible Server. Delegated to the Postgres service so
# the server NIC can live inside this subnet directly (no private endpoint needed).
resource "azurerm_subnet" "postgres" {
  name                 = "snet-postgres"
  resource_group_name  = azurerm_resource_group.teacupboutique.name
  virtual_network_name = azurerm_virtual_network.teacupboutique.name
  address_prefixes     = ["10.10.4.0/24"]

  # Auto-added by Flexible Server on provisioning (used for backup storage).
  # Declaring explicitly so Terraform doesn't keep trying to remove it.
  service_endpoints = ["Microsoft.Storage"]

  delegation {
    name = "postgres-delegation"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# Private DNS zone for Postgres. This is what makes the server's FQDN
# resolve to a VNet-internal IP instead of the public one.
resource "azurerm_private_dns_zone" "postgres" {
  name                = "private.postgres.database.azure.com"
  resource_group_name = azurerm_resource_group.teacupboutique.name
}

resource "azurerm_private_dns_zone_virtual_network_link" "postgres" {
  name                  = "postgres-dns-link"
  resource_group_name   = azurerm_resource_group.teacupboutique.name
  private_dns_zone_name = azurerm_private_dns_zone.postgres.name
  virtual_network_id    = azurerm_virtual_network.teacupboutique.id
}
