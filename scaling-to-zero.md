# Container Apps Scale To Zero Notes

## Current v2 Settings

From `infra/terraform/containerapps.tf`:

| App | Current `min_replicas` | Notes |
| --- | ---: | --- |
| `tcb-gateway` | 0 | Public API entrypoint. Can currently sleep. |
| `tcb-auth` | 0 | HTTP service. Can sleep and wake on request. |
| `tcb-inventory` | 1 | Kept warm. Mostly HTTP request/response. |
| `tcb-orders` | 1 | Kept warm. HTTP endpoints plus Service Bus consumers. |
| `tcb-payments` | 1 | Kept warm. HTTP/webhook endpoints plus background processing. |
| `tcb-notifications` | 1 | Kept warm. Background Service Bus consumer, no ingress. |

Migration jobs are separate Container App Jobs. They only run when triggered by the GitHub workflow and are not part of the always-on app scaling question.

## Mental Model

`min_replicas = 0` means Azure Container Apps is allowed to stop all replicas when the app is idle.

`min_replicas = 1` means Azure should keep one replica running all the time.

HTTP traffic and Service Bus messages are different wake-up signals:

- HTTP apps with ingress can wake when an HTTP request arrives.
- Background consumers do not automatically wake from Service Bus messages unless a Service Bus/KEDA scale rule is configured.

If a service is at zero replicas, its code is not running. That means its worker loop is not connected to Service Bus and cannot wake itself.

For background consumers, the proper scale-to-zero model is:

1. Azure/KEDA watches the queue or topic subscription.
2. When messages exist, Azure starts the container app.
3. The app's worker connects to Service Bus and processes messages.
4. When the backlog is empty long enough, Azure scales the app back to zero.

Without that Service Bus scale rule, a sleeping background worker may never wake just because a message arrived.

## Service-by-Service Notes

### Gateway

The gateway is the public API front door. The frontend calls the gateway, and the gateway calls internal services.

Keeping this warm probably gives the best user experience because every API request enters through it.

Likely future setting:

```text
gateway: min_replicas = 1
```

### Auth

Auth is mostly HTTP-driven: register, login, refresh, email verification, password reset.

It can scale to zero and wake on HTTP traffic, but cold starts can be noticeable for login/register/email verification.

Likely future setting:

```text
auth: min_replicas = 0
```

Potential exception: keep it at `1` if auth cold starts feel bad.

### Inventory

Inventory is mostly request/response behind the gateway.

Likely future setting:

```text
inventory: min_replicas = 0
```

### Orders

Orders has HTTP endpoints, but it also consumes Service Bus events such as payment-related messages.

It can wake from HTTP requests if ingress is enabled and the gateway calls it. It should not be scaled to zero for background processing until Service Bus scale rules are added.

Near-term safe setting:

```text
orders: min_replicas = 1
```

Future setting after Service Bus/KEDA scale rules:

```text
orders: min_replicas = 0
```

### Payments

Payments has HTTP endpoints/webhooks and background processing.

Stripe webhook HTTP calls can wake it through ingress, but internal queue/background work needs Service Bus scale rules before scale-to-zero is safe.

Near-term safe setting:

```text
payments: min_replicas = 1
```

Future setting after Service Bus/KEDA scale rules:

```text
payments: min_replicas = 0
```

### Notifications

Notifications has no ingress and exists to consume Service Bus messages.

With no HTTP front door, there is nothing to wake it unless a Service Bus/KEDA scale rule is configured.

Near-term safe setting:

```text
notifications: min_replicas = 1
```

Future setting after Service Bus/KEDA scale rules:

```text
notifications: min_replicas = 0
```

## Practical Future Plan

Conservative staging/runtime setup:

```text
gateway: 1
auth: 0
inventory: 0
orders: 1
payments: 1
notifications: 1
```

More aggressive setup after Service Bus scale rules:

```text
gateway: 1
auth: 0
inventory: 0
orders: 0
payments: 0
notifications: 0
```

To change these values, edit `min_replicas` in `infra/terraform/containerapps.tf`, then run:

```powershell
terraform plan
terraform apply
```

No Docker rebuild or GitHub deployment workflow is needed for scaling-only changes. Terraform updates the Container App runtime configuration in Azure, and Azure reconciles the app/revision as needed.

## Related Follow-Up

Before making `auth` heavily scale-to-zero/redeploy-safe, persist ASP.NET Core Data Protection keys. Email verification, password reset, and email-change tokens can become invalid across auth restarts/revisions if the Data Protection key ring is only stored inside the container.
