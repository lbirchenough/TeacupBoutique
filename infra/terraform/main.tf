terraform {
  required_version = ">= 1.9"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
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

resource "azurerm_resource_group" "main" {
  name     = "${var.project}-rg"
  location = var.location
}
