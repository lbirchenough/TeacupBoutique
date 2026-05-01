locals {
  container_app_identity_name = "${var.project}-ca-identity"
  servicebus_fqdn             = "${azurerm_servicebus_namespace.teacupboutique.name}.servicebus.windows.net"

  # These are intentionally not Terraform-managed because the values come from
  # external providers. Create them before applying the Container Apps.
  external_secret_ids = {
    cloudinary_url        = "${azurerm_key_vault.teacupboutique.vault_uri}secrets/cloudinary-url"
    mailgun_api_key       = "${azurerm_key_vault.teacupboutique.vault_uri}secrets/mailgun-api-key"
    stripe_secret_key     = "${azurerm_key_vault.teacupboutique.vault_uri}secrets/stripe-secret-key"
    stripe_webhook_secret = "${azurerm_key_vault.teacupboutique.vault_uri}secrets/stripe-webhook-secret"
    turnstile_secret_key  = "${azurerm_key_vault.teacupboutique.vault_uri}secrets/turnstile-secret-key"
  }

  service_container_apps = {
    auth = {
      image        = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-auth:${var.container_image_tag}"
      min_replicas = 0
      ingress      = true
      env = {
        ASPNETCORE_ENVIRONMENT              = "Production"
        ASPNETCORE_HTTP_PORTS               = "8080"
        AdminSeed__Email                    = var.admin_seed_email
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        ClientUrl                           = local.frontend_url
        DataProtection__BlobUri             = "${azurerm_storage_account.dataprotection.primary_blob_endpoint}${azurerm_storage_container.dataprotection.name}/${local.auth_data_protection_blob_name}"
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__AuthDbPostgres = "connstr-authdb"
        Jwt__Key                          = "jwt-signing-key"
        AdminSeed__Password               = "admin-seed-password"
        Turnstile__SecretKey              = "turnstile-secret-key"
      }
      secrets = {
        connstr-authdb       = azurerm_key_vault_secret.postgres_connection_strings["authdb"].id
        jwt-signing-key      = azurerm_key_vault_secret.jwt_signing_key.id
        admin-seed-password  = azurerm_key_vault_secret.admin_seed_password.id
        turnstile-secret-key = local.external_secret_ids.turnstile_secret_key
      }
    }

    inventory = {
      image        = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-inventory:${var.container_image_tag}"
      min_replicas = 0
      ingress      = true
      env = {
        ASPNETCORE_ENVIRONMENT              = "Production"
        ASPNETCORE_HTTP_PORTS               = "8080"
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__InventoryDbPostgres = "connstr-inventorydb"
        CLOUDINARY_URL                         = "cloudinary-url"
      }
      secrets = {
        connstr-inventorydb = azurerm_key_vault_secret.postgres_connection_strings["inventorydb"].id
        cloudinary-url      = local.external_secret_ids.cloudinary_url
      }
    }

    orders = {
      image        = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-orders:${var.container_image_tag}"
      min_replicas = 0
      ingress      = true
      env = {
        ASPNETCORE_ENVIRONMENT              = "Production"
        ASPNETCORE_HTTP_PORTS               = "8080"
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__OrdersDbPostgres = "connstr-ordersdb"
        Turnstile__SecretKey                = "turnstile-secret-key"
      }
      secrets = {
        connstr-ordersdb     = azurerm_key_vault_secret.postgres_connection_strings["ordersdb"].id
        turnstile-secret-key = local.external_secret_ids.turnstile_secret_key
      }
    }

    payments = {
      image        = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-payments:${var.container_image_tag}"
      min_replicas = 0
      ingress      = true
      env = {
        ASPNETCORE_ENVIRONMENT              = "Production"
        ASPNETCORE_HTTP_PORTS               = "8080"
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__PaymentsDbPostgres = "connstr-paymentsdb"
        Stripe__SecretKey                     = "stripe-secret-key"
        Stripe__WebhookSecret                 = "stripe-webhook-secret"
        Turnstile__SecretKey                  = "turnstile-secret-key"
      }
      secrets = {
        connstr-paymentsdb    = azurerm_key_vault_secret.postgres_connection_strings["paymentsdb"].id
        stripe-secret-key     = local.external_secret_ids.stripe_secret_key
        stripe-webhook-secret = local.external_secret_ids.stripe_webhook_secret
        turnstile-secret-key  = local.external_secret_ids.turnstile_secret_key
      }
    }

    notifications = {
      image        = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-notifications:${var.container_image_tag}"
      min_replicas = 0
      ingress      = false
      env = {
        DOTNET_ENVIRONMENT                  = "Production"
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        FrontendUrl                         = local.frontend_url
        Mailgun__Domain                     = var.mailgun_domain
        Mailgun__FromAddress                = var.mailgun_from_address
        Mailgun__FromName                   = var.mailgun_from_name
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        Mailgun__ApiKey = "mailgun-api-key"
      }
      secrets = {
        mailgun-api-key = local.external_secret_ids.mailgun_api_key
      }
    }
  }

  # KEDA queue scale rules, only used by services without HTTP ingress
  # (notifications). Services with HTTP ingress wake via gateway /api/wake instead.
  # Queue names are formatted "<consumer-service>.<event-name>" so we group by prefix.
  scale_rules_by_service = {
    for service_name in keys(local.service_container_apps) :
    service_name => [
      for queue_name in local.servicebus_queues :
      queue_name
      if startswith(queue_name, "${service_name}.")
    ]
  }

  migration_jobs = {
    auth = {
      image = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-auth:${var.container_image_tag}"
      env = {
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__AuthDbPostgres = "connstr-authdb"
      }
      secrets = {
        connstr-authdb = azurerm_key_vault_secret.postgres_connection_strings["authdb"].id
      }
    }

    inventory = {
      image = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-inventory:${var.container_image_tag}"
      env = {
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__InventoryDbPostgres = "connstr-inventorydb"
      }
      secrets = {
        connstr-inventorydb = azurerm_key_vault_secret.postgres_connection_strings["inventorydb"].id
      }
    }

    orders = {
      image = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-orders:${var.container_image_tag}"
      env = {
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__OrdersDbPostgres = "connstr-ordersdb"
      }
      secrets = {
        connstr-ordersdb = azurerm_key_vault_secret.postgres_connection_strings["ordersdb"].id
      }
    }

    payments = {
      image = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-payments:${var.container_image_tag}"
      env = {
        AZURE_CLIENT_ID                     = azurerm_user_assigned_identity.container_apps.client_id
        Messaging__Provider                 = "ServiceBus"
        ServiceBus__FullyQualifiedNamespace = local.servicebus_fqdn
      }
      secret_env = {
        ConnectionStrings__PaymentsDbPostgres = "connstr-paymentsdb"
      }
      secrets = {
        connstr-paymentsdb = azurerm_key_vault_secret.postgres_connection_strings["paymentsdb"].id
      }
    }
  }
}

resource "azurerm_container_app_environment" "teacupboutique" {
  name                = "${var.project}-cae"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location

  log_analytics_workspace_id = azurerm_log_analytics_workspace.teacupboutique.id
  infrastructure_subnet_id   = azurerm_subnet.apps.id

  # External environment: gateway gets public ingress; service apps use internal
  # ingress; worker apps have no ingress.
  internal_load_balancer_enabled = false

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  lifecycle {
    ignore_changes = [
      infrastructure_resource_group_name,
      workload_profile,
    ]
  }
}

resource "azurerm_user_assigned_identity" "container_apps" {
  name                = local.container_app_identity_name
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
}

resource "azurerm_role_assignment" "container_apps_acr_pull" {
  scope                = azurerm_container_registry.teacupboutique.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}

resource "azurerm_role_assignment" "container_apps_kv_secrets" {
  scope                = azurerm_key_vault.teacupboutique.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}

resource "azurerm_role_assignment" "container_apps_servicebus_sender" {
  scope                = azurerm_servicebus_namespace.teacupboutique.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}

resource "azurerm_role_assignment" "container_apps_servicebus_receiver" {
  scope                = azurerm_servicebus_namespace.teacupboutique.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_user_assigned_identity.container_apps.principal_id
}

resource "azurerm_container_app" "services" {
  for_each = local.service_container_apps

  name                         = "${var.project}-${each.key}"
  container_app_environment_id = azurerm_container_app_environment.teacupboutique.id
  resource_group_name          = azurerm_resource_group.teacupboutique.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.teacupboutique.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  dynamic "secret" {
    for_each = each.value.secrets
    content {
      name                = secret.key
      key_vault_secret_id = secret.value
      identity            = azurerm_user_assigned_identity.container_apps.id
    }
  }

  template {
    min_replicas = each.value.min_replicas
    max_replicas = 1

    container {
      name   = each.key
      image  = each.value.image
      cpu    = 0.25
      memory = "0.5Gi"

      dynamic "env" {
        for_each = each.value.env
        content {
          name  = env.key
          value = env.value
        }
      }

      dynamic "env" {
        for_each = each.value.secret_env
        content {
          name        = env.key
          secret_name = env.value
        }
      }
    }

    # Queue-based KEDA wake-up. Only applied to notifications, which has no
    # HTTP ingress and so cannot be woken via gateway /api/wake. Other backends
    # (auth, inventory, orders, payments) wake via HTTP from gateway and rely
    # on the warm-up window to drain their queues during a user session.
    dynamic "custom_scale_rule" {
      for_each = each.key == "notifications" ? toset(local.scale_rules_by_service[each.key]) : toset([])
      content {
        name             = "sb-${replace(custom_scale_rule.value, ".", "-")}"
        custom_rule_type = "azure-servicebus"
        metadata = {
          namespace    = azurerm_servicebus_namespace.teacupboutique.name
          queueName    = custom_scale_rule.value
          messageCount = "1"
        }
        identity_id = azurerm_user_assigned_identity.container_apps.id
      }
    }

    dynamic "custom_scale_rule" {
      for_each = each.key == "payments" ? [1] : []
      content {
        name             = "sb-stripe-webhook"
        custom_rule_type = "azure-servicebus"
        metadata = {
          namespace    = azurerm_servicebus_namespace.teacupboutique.name
          queueName    = local.servicebus_queue_name
          messageCount = "1"
        }
        identity_id = azurerm_user_assigned_identity.container_apps.id
      }
    }
  }

  dynamic "ingress" {
    for_each = each.value.ingress ? [1] : []
    content {
      external_enabled = false
      target_port      = 8080
      transport        = "http"

      traffic_weight {
        percentage      = 100
        latest_revision = true
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.container_apps_acr_pull,
    azurerm_role_assignment.container_apps_data_protection_blob,
    azurerm_role_assignment.container_apps_kv_secrets,
    azurerm_role_assignment.container_apps_servicebus_sender,
    azurerm_role_assignment.container_apps_servicebus_receiver,
    azurerm_servicebus_queue.queues,
    azurerm_servicebus_queue.stripe_webhook,
  ]

  # Image is managed by the GitHub Actions deploy pipeline (az containerapp
  # update), which rolls each app to a SHA-pinned tag after every build.
  # Terraform only seeds :latest on first provision.
  lifecycle {
    ignore_changes = [
      template[0].container[0].image,
    ]
  }
}

resource "azurerm_container_app" "gateway" {
  name                         = "${var.project}-gateway"
  container_app_environment_id = azurerm_container_app_environment.teacupboutique.id
  resource_group_name          = azurerm_resource_group.teacupboutique.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.teacupboutique.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  secret {
    name                = "jwt-signing-key"
    key_vault_secret_id = azurerm_key_vault_secret.jwt_signing_key.id
    identity            = azurerm_user_assigned_identity.container_apps.id
  }

  template {
    min_replicas = 1
    max_replicas = 1

    container {
      name   = "gateway"
      image  = "${azurerm_container_registry.teacupboutique.login_server}/teacupboutique-gateway:${var.container_image_tag}"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "ASPNETCORE_HTTP_PORTS"
        value = "8080"
      }

      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.container_apps.client_id
      }

      env {
        name  = "Cors__AllowedOrigins"
        value = local.frontend_url
      }

      env {
        name        = "Jwt__Key"
        secret_name = "jwt-signing-key"
      }

      env {
        name  = "ReverseProxy__Clusters__auth-cluster__Destinations__destination1__Address"
        value = "https://${azurerm_container_app.services["auth"].ingress[0].fqdn}/"
      }

      env {
        name  = "ReverseProxy__Clusters__inventory-cluster__Destinations__destination1__Address"
        value = "https://${azurerm_container_app.services["inventory"].ingress[0].fqdn}/"
      }

      env {
        name  = "ReverseProxy__Clusters__orders-cluster__Destinations__destination1__Address"
        value = "https://${azurerm_container_app.services["orders"].ingress[0].fqdn}/"
      }

      env {
        name  = "ReverseProxy__Clusters__payments-cluster__Destinations__destination1__Address"
        value = "https://${azurerm_container_app.services["payments"].ingress[0].fqdn}/"
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  depends_on = [
    azurerm_role_assignment.container_apps_acr_pull,
    azurerm_role_assignment.container_apps_kv_secrets,
    azurerm_container_app.services,
  ]

  # Image is managed by the GitHub Actions deploy pipeline (az containerapp
  # update). Terraform only seeds :latest on first provision.
  lifecycle {
    ignore_changes = [
      template[0].container[0].image,
    ]
  }
}

resource "azurerm_container_app_job" "migrations" {
  for_each = local.migration_jobs

  name                         = "${var.project}-${each.key}-migrations"
  location                     = azurerm_resource_group.teacupboutique.location
  resource_group_name          = azurerm_resource_group.teacupboutique.name
  container_app_environment_id = azurerm_container_app_environment.teacupboutique.id
  workload_profile_name        = "Consumption"

  replica_timeout_in_seconds = 1800
  replica_retry_limit        = 0

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.container_apps.id]
  }

  registry {
    server   = azurerm_container_registry.teacupboutique.login_server
    identity = azurerm_user_assigned_identity.container_apps.id
  }

  dynamic "secret" {
    for_each = each.value.secrets
    content {
      name                = secret.key
      key_vault_secret_id = secret.value
      identity            = azurerm_user_assigned_identity.container_apps.id
    }
  }

  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  template {
    container {
      name    = "${each.key}-migrations"
      image   = each.value.image
      command = ["./efbundle"]
      cpu     = 0.25
      memory  = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      dynamic "env" {
        for_each = each.value.env
        content {
          name  = env.key
          value = env.value
        }
      }

      dynamic "env" {
        for_each = each.value.secret_env
        content {
          name        = env.key
          secret_name = env.value
        }
      }
    }
  }

  depends_on = [
    azurerm_role_assignment.container_apps_acr_pull,
    azurerm_role_assignment.container_apps_kv_secrets,
  ]

  # Image is managed by the GitHub Actions deploy pipeline (az containerapp
  # job update). Terraform only seeds :latest on first provision.
  lifecycle {
    ignore_changes = [
      template[0].container[0].image,
    ]
  }
}
