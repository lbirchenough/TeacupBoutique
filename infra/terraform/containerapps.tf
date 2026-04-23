resource "azurerm_container_app_environment" "teacupboutique" {
  name                = "${var.project}-cae"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location

  log_analytics_workspace_id = azurerm_log_analytics_workspace.teacupboutique.id
  infrastructure_subnet_id   = azurerm_subnet.apps.id

  # External = environment has a public ingress IP. Individual apps still
  # control their own visibility via ingress.external_enabled (true for gateway,
  # false for everything else). External mode also gives outbound internet
  # access for free, so apps can reach Stripe/Mailgun/etc.
  internal_load_balancer_enabled = false

  # Workload Profiles environment — modern mode, smaller subnet requirement,
  # same pricing as Consumption-only when using the Consumption profile.
  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  lifecycle {
    # Azure auto-generates the infrastructure RG name ("ME_<env>_<rg>_<location>") on
    # first create, and auto-populates min/max counts on the Consumption workload_profile
    # (both 0; meaningless for Consumption). Neither is in our config, so Terraform
    # otherwise treats them as drift — force-replacing the CAE on every plan.
    ignore_changes = [
      infrastructure_resource_group_name,
      workload_profile,
    ]
  }
}
