terraform {
  required_version = ">= 1.9"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "tcb-tfstate-rg"
    storage_account_name = "tcbtfstate"
    container_name       = "tfstate"
    key                  = "teacupboutique.tfstate"
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

provider "azuread" {}

resource "azurerm_resource_group" "teacupboutique" {
  name     = "${var.project}-rg"
  location = var.location
}

resource "azurerm_log_analytics_workspace" "teacupboutique" {
  name                = "${var.project}-logs"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
}
