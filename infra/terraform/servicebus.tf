locals {
  servicebus_queue_name = "payments.process-webhook"

  # Per-event-consumer queues. The name format is "<consumer-service>.<event-name>"
  # (mirrors the previous subscription names). Producers publish directly to these
  # queues; the EventTargets map in each producer's appsettings.json drives the
  # logical-event -> queue(s) routing, including publisher-side fan-out for events
  # that previously had multiple topic subscribers (orders.OrderCancelled and
  # payments.PaymentSucceeded).
  servicebus_queues = toset([
    "inventory.order-placed",
    "inventory.order-cancelled",
    "inventory.payment-succeeded",
    "orders.stock-reserved",
    "orders.stock-unavailable",
    "orders.payment-succeeded",
    "orders.payment-failed",
    "orders.booking-cancelled",
    "orders.booking-completed",
    "orders.refund-succeeded",
    "orders.refund-failed",
    "orders.return-assessed-missing",
    "payments.ready-for-payment",
    "payments.refund-requested",
    "notifications.order-confirmed",
    "notifications.payment-failed",
    "notifications.order-cancelled",
    "notifications.order-completed",
    "notifications.email-verification",
    "notifications.password-reset",
    "notifications.email-changed",
    "notifications.email-change-verification",
    "notifications.password-changed",
  ])
}

resource "azurerm_servicebus_namespace" "teacupboutique" {
  name                = "${var.project}-servicebus"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  sku                 = "Basic"

  # Basic SKU: queues only (no topics/subscriptions). ~$0.05 per million ops,
  # no monthly base. Sufficient for this site since publisher-side fan-out
  # replaces the few events that previously had multiple topic subscribers.
}

resource "azurerm_servicebus_queue" "queues" {
  for_each = local.servicebus_queues

  name               = each.key
  namespace_id       = azurerm_servicebus_namespace.teacupboutique.id
  max_delivery_count = 10
  lock_duration      = "PT1M"
  # Basic SKU caps default_message_ttl at 14 days. Standard's default is
  # effectively infinite, which Azure rejects on a Standard->Basic downgrade.
  default_message_ttl = "P14D"
}

# Stripe webhook buffer - point-to-point queue, distinct from the per-event queues.
resource "azurerm_servicebus_queue" "stripe_webhook" {
  name                = local.servicebus_queue_name
  namespace_id        = azurerm_servicebus_namespace.teacupboutique.id
  max_delivery_count  = 10
  lock_duration       = "PT1M"
  default_message_ttl = "P14D"
}

# Namespace-level SAS auth rule - listen + send, no manage.
# Used for the connection string local dev needs. Container Apps will use managed
# identity (RBAC roles on the namespace) instead once provisioned.
resource "azurerm_servicebus_namespace_authorization_rule" "app" {
  name         = "app-send-listen"
  namespace_id = azurerm_servicebus_namespace.teacupboutique.id
  listen       = true
  send         = true
  manage       = false
}

resource "azurerm_key_vault_secret" "servicebus_connection_string" {
  name         = "servicebus-connection-string"
  value        = azurerm_servicebus_namespace_authorization_rule.app.primary_connection_string
  key_vault_id = azurerm_key_vault.teacupboutique.id

  depends_on = [azurerm_role_assignment.kv_admin_self]
}
