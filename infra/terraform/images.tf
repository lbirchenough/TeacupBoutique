# Seeds the registry with :latest images when it is first created, so a fresh
# `terraform apply` can create the Container Apps without waiting on CI.
# Day-to-day image deploys stay with GitHub Actions (deploy-v2.yml); this only
# re-runs if the registry itself is replaced (e.g. after a full destroy).
locals {
  seed_image_dockerfiles = {
    gateway       = "src/gateway/Dockerfile"
    auth          = "src/services/auth/Dockerfile"
    inventory     = "src/services/inventory/Dockerfile"
    orders        = "src/services/orders/Dockerfile"
    payments      = "src/services/payments/Dockerfile"
    notifications = "src/services/notifications/Dockerfile"
  }
}

resource "terraform_data" "seed_images" {
  for_each         = local.seed_image_dockerfiles
  triggers_replace = azurerm_container_registry.teacupboutique.id

  # Builds in Azure (ACR Tasks) from the repo root, so no local Docker is needed.
  # Requires an `az login` session.
  provisioner "local-exec" {
    working_dir = "${path.module}/../.."
    command     = "az acr build --registry ${azurerm_container_registry.teacupboutique.name} --image teacupboutique-${each.key}:latest --file ${each.value} ."
  }
}
