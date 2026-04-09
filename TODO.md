# TODO

## Code Quality / Security

- **CreateOrderItemDto sends display fields from client** — `ProductName`, `ProductThemeColor`, and `ProductImageUrl` are trusted from the frontend payload rather than looked up server-side. The client should only need to send `ProductId`, `Quantity`, and `RentalDate`. The Orders service (or Inventory via the `StockReserved` event) should be the one stamping display fields from authoritative data, similar to how pricing is already handled.
