locals {
  servicebus_queue_name = "payments.process-webhook"

  # Map of subscription name => physical Service Bus event topic.
  # Subscription names mirror the RabbitMQ queue names, and topic names mirror the
  # lowercased routing keys passed to IMessagePublisher.PublishAsync(...).
  #
  # Most events have one subscription. Fanout is just multiple subscriptions on
  # the same topic, using Service Bus's default "$Default" rule (SqlFilter 1=1).
  servicebus_subscriptions = {
    "inventory.order-placed"                  = "orders.orderplaced"
    "inventory.order-cancelled"               = "orders.ordercancelled"
    "inventory.payment-succeeded"             = "payments.paymentsucceeded"
    "orders.stock-reserved"                   = "inventory.stockreserved"
    "orders.stock-unavailable"                = "inventory.stockunavailable"
    "orders.payment-succeeded"                = "payments.paymentsucceeded"
    "orders.payment-failed"                   = "payments.paymentfailed"
    "orders.booking-cancelled"                = "inventory.bookingcancelled"
    "orders.booking-completed"                = "inventory.bookingcompleted"
    "orders.refund-succeeded"                 = "payments.refundsucceeded"
    "orders.refund-failed"                    = "payments.refundfailed"
    "orders.return-assessed-missing"          = "inventory.returnassessedwithmissingitems"
    "payments.ready-for-payment"              = "orders.readyforpayment"
    "payments.refund-requested"               = "orders.refundrequested"
    "notifications.order-confirmed"           = "orders.orderconfirmed"
    "notifications.payment-failed"            = "orders.paymentfailed"
    "notifications.order-cancelled"           = "orders.ordercancelled"
    "notifications.order-completed"           = "orders.ordercompleted"
    "notifications.email-verification"        = "auth.emailverificationrequested"
    "notifications.password-reset"            = "auth.passwordresetrequested"
    "notifications.email-changed"             = "auth.emailchanged"
    "notifications.email-change-verification" = "auth.emailchangeverificationrequested"
    "notifications.password-changed"          = "auth.passwordchanged"
  }

  servicebus_topics = toset(values(local.servicebus_subscriptions))
}

resource "azurerm_servicebus_namespace" "teacupboutique" {
  name                = "${var.project}-servicebus"
  resource_group_name = azurerm_resource_group.teacupboutique.name
  location            = azurerm_resource_group.teacupboutique.location
  sku                 = "Standard"

  # Standard SKU has a public endpoint, auth-gated (SAS or managed identity).
  # Premium would unlock private endpoints + VNet-only reach (~$650/mo floor).
  # Lock down later with azurerm_servicebus_namespace_network_rule_set if desired.
}

resource "azurerm_servicebus_topic" "events" {
  for_each = local.servicebus_topics

  name         = each.key
  namespace_id = azurerm_servicebus_namespace.teacupboutique.id
}

resource "azurerm_servicebus_subscription" "subs" {
  for_each = local.servicebus_subscriptions

  name               = each.key
  topic_id           = azurerm_servicebus_topic.events[each.value].id
  max_delivery_count = 10
  lock_duration      = "PT1M"
}

# Stripe webhook buffer - point-to-point queue, separate from the pub/sub topics.
resource "azurerm_servicebus_queue" "stripe_webhook" {
  name               = local.servicebus_queue_name
  namespace_id       = azurerm_servicebus_namespace.teacupboutique.id
  max_delivery_count = 10
  lock_duration      = "PT1M"
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
