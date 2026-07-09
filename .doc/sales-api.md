[Back to README](../README.md)

### Sales

Sales follow the DDD **External Identities** pattern: `Customer`, `Branch` and each item's `Product` belong to other domains, so only their id and a denormalized name are stored and returned alongside the sale.

#### Business Rules (quantity-based discount)

| Quantity | Discount |
|---|---|
| < 4 | None |
| 4 – 9 | 10% |
| 10 – 20 | 20% |
| > 20 | Rejected (400) |

These rules are enforced in the domain (`SaleItem`), not just at the API layer, so they apply the same way on create, update and to every item independently.

#### Response envelope

Every response is wrapped the same way as the rest of this API (see the existing `Users`/`Auth` endpoints), **not** the generic `{ type, error, detail }` shape sketched in [General API](./general-api.md) — that shape was never actually implemented for any resource in this codebase, so Sales stays consistent with what's really there:

```json
{
  "data": { },
  "success": true,
  "message": "string",
  "errors": []
}
```

Error responses omit `data` and set `success: false`, with `errors` populated for validation failures:

```json
{
  "success": false,
  "message": "Validation Failed",
  "errors": [
    { "error": "LessThanOrEqualValidator", "detail": "It's not possible to sell above 20 identical items." }
  ]
}
```

| Status | Meaning |
|---|---|
| 200 / 201 | Success |
| 400 | Request validation failure or business rule violation (e.g. quantity > 20, already cancelled) |
| 404 | Sale (or item) not found |

---

#### POST /api/sales
- Description: Creates a new sale. Each item's discount is computed from its quantity.
- Request Body:
  ```json
  {
    "saleNumber": "string",
    "saleDate": "string (date-time)",
    "customerId": "guid",
    "customerName": "string",
    "branchId": "guid",
    "branchName": "string",
    "items": [
      {
        "productId": "guid",
        "productName": "string",
        "quantity": "integer (1-20)",
        "unitPrice": "number"
      }
    ]
  }
  ```
- Response `201 Created` (`Location` resolves to `GET /api/sales/{id}`):
  ```json
  {
    "data": {
      "id": "guid",
      "saleNumber": "string",
      "saleDate": "string (date-time)",
      "customerId": "guid",
      "customerName": "string",
      "branchId": "guid",
      "branchName": "string",
      "items": [
        {
          "id": "guid",
          "productId": "guid",
          "productName": "string",
          "quantity": "integer",
          "unitPrice": "number",
          "discountPercentage": "number",
          "discount": "number",
          "totalAmount": "number",
          "isCancelled": "boolean"
        }
      ],
      "totalAmount": "number",
      "isCancelled": "boolean",
      "createdAt": "string (date-time)",
      "updatedAt": "string (date-time) | null"
    },
    "success": true,
    "message": "",
    "errors": []
  }
  ```
- `400`: empty `saleNumber`/`items`, an item quantity outside 1-20, or a non-positive `unitPrice`.

#### GET /api/sales/{id}
- Description: Retrieves a sale by id, including its items.
- Path Parameters: `id` — Sale id (guid).
- Response `200 OK`: same shape as the `POST` response's `data`.
- `404`: no sale with that id.

#### GET /api/sales
- Description: Retrieves a paginated, ordered and filtered list of sales.
- Query Parameters:
  - `_page` (optional, default `1`)
  - `_size` (optional, default `10`, max `100`)
  - `_order` (optional): comma-separated, e.g. `saleDate desc, saleNumber`
  - Any other query key is treated as a filter: `saleNumber`, `customer`/`customerName`, `branch`/`branchName` (all support a leading/trailing `*` for a partial, case-insensitive match), `isCancelled`, `_minSaleDate`/`_maxSaleDate`, `_minTotalAmount`/`_maxTotalAmount`
- Example: `GET /api/sales?_page=1&_size=10&_order=saleDate desc&branchName=Downtown*`
- Response `200 OK`:
  ```json
  {
    "currentPage": "integer",
    "totalPages": "integer",
    "totalCount": "integer",
    "data": [ /* same item shape as GET /api/sales/{id} */ ],
    "success": true,
    "message": "",
    "errors": []
  }
  ```

#### PUT /api/sales/{id}
- Description: Updates a sale's details and fully replaces its items — quantities are re-validated and re-discounted exactly as on creation.
- Path Parameters: `id` — Sale id (guid).
- Request Body: same as `POST`, without `saleNumber` (it's immutable once a sale is created).
- Response `200 OK`: same shape as `GET /api/sales/{id}`.
- `400`: same validation/business rules as `POST`, plus attempting to update an already-cancelled sale.
- `404`: no sale with that id.

#### DELETE /api/sales/{id}
- Description: Deletes a sale.
- Path Parameters: `id` — Sale id (guid).
- Response `200 OK`:
  ```json
  { "success": true, "message": "Sale deleted successfully", "errors": [] }
  ```
- `404`: no sale with that id.

#### PATCH /api/sales/{id}/cancel
- Description: Cancels the entire sale. Raises `SaleCancelled`.
- Path Parameters: `id` — Sale id (guid).
- Response `200 OK`:
  ```json
  { "success": true, "message": "Sale cancelled successfully", "errors": [] }
  ```
- `400`: sale is already cancelled.
- `404`: no sale with that id.

#### PATCH /api/sales/{id}/items/{itemId}/cancel
- Description: Cancels a single item; the sale's `totalAmount` is recalculated from the remaining active items. Raises `ItemCancelled`.
- Path Parameters: `id` — Sale id (guid); `itemId` — Item id (guid).
- Response `200 OK`:
  ```json
  { "success": true, "message": "Item cancelled successfully", "errors": [] }
  ```
- `400`: item is already cancelled.
- `404`: sale or item not found.

#### Domain events

`SaleCreated`, `SaleModified`, `SaleCancelled` and `ItemCancelled` are published on every corresponding action (see [Architecture Decisions](./decisions.md)). No message broker is required to observe them: each is logged by its handler, e.g.

```
Event SaleCreatedEvent published: {"SaleId": "...", "SaleNumber": "SALE-0001", "OccurredAt": "..."}
```

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./general-api.md">Previous: General API</a>
  <a href="./decisions.md">Next: Architecture Decisions</a>
</div>
