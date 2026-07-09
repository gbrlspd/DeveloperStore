[Back to README](../README.md)

## Architecture Decisions

Lightweight ADR-style log of the decisions made while implementing the Sales API, in the order they came up.

---

### 1. Backend only, no Angular front-end

**Decision:** implement only the Sales REST API described in the challenge; no Angular SPA.

**Why:** the challenge statement asks specifically for a Sales CRUD API. Building a front-end on top would spread effort away from what's actually being evaluated (domain modeling, API design, testing, Git history) within a 4-day window. Manual verification is done through Swagger and the Postman collection in this folder.

---

### 2. Promote the solution to the repository root

**Decision:** move the provided `template/backend/` solution to the repository root (`src/`, `tests/`, `.sln`).

**Why:** the challenge's own [Project Structure](./project-structure.md) expects `src/`, `tests/` and `README.md` at the root. Keeping the application nested under a folder literally named `template` was confusing to navigate and hard to justify.

---

### 3. External Identities with denormalized value objects

**Decision:** `Customer`, `Branch` and `Product` are referenced from `Sale`/`SaleItem` as owned value objects (`CustomerReference`, `BranchReference`, `ProductReference`) holding only `Id` + `Name`, mapped as EF Core owned types.

**Why:** this is the pattern the challenge explicitly asks for (DDD, External Identities, denormalized descriptions), since Customer/Branch/Product belong to other bounded contexts this API has no direct access to. Denormalizing the name avoids a runtime dependency on those other services just to render a sale.

---

### 4. Business rules live in the domain, not the handlers

**Decision:** the quantity-based discount policy and the 20-item limit are enforced inside `SaleItem`'s constructor, and `Sale` only exposes behavior (`AddItem`, `CancelItem`, `Cancel`, `ClearItems`, `UpdateDetails`) — no public setters.

**Why:** a rule this central to the challenge deserves to be impossible to bypass. If it lived in the Application handlers instead, a future handler (or a script, or a migration) could construct a `Sale` that violates it. Application-layer validators (`CreateSaleValidator`, etc.) duplicate the quantity check for fast, friendly 400s — the domain check is what actually guarantees the invariant.

---

### 5. PostgreSQL + EF Core

**Decision:** PostgreSQL as the relational store, via the Npgsql EF Core provider — already scaffolded in the template.

**Why:** lowest-friction path consistent with the job description (SQL Server/PostgreSQL + EF Core) and the existing `User`/`Auth` reference implementation.

---

### 6. Sale number is client-supplied, not generated

**Decision:** `saleNumber` is a required field on `CreateSaleRequest`, validated as unique (unique index in Postgres), not generated server-side.

**Why:** in a real deployment, sale numbers typically come from an upstream system (a POS, a checkout flow, an order sequence already owned elsewhere). Generating one here would be guessing a business process this API doesn't own. If a real numbering authority existed, swapping this for a generated value is a small, isolated change in `CreateSaleHandler`.

---

### 7. Domain events via Rebus, in-memory transport

**Decision:** `SaleCreated`, `SaleModified`, `SaleCancelled` and `ItemCancelled` are published through Rebus configured with an in-memory transport; each has a handler that logs what it received.

**Why:** the challenge explicitly allows logging instead of a real broker. Rebus was chosen over publishing directly through the application's own logger because it keeps the *shape* of a real integration: handlers subscribe to typed messages through an abstraction (`ISaleEventPublisher`), so swapping `UseInMemoryTransport` for `UseAzureServiceBus` later is a one-line change in `MessagingModuleInitializer` — nothing in `CreateSaleHandler` or the handlers would need to change.

---

### 8. HTTP only inside Docker; TLS terminates at the edge

**Decision:** the WebApi container serves plain HTTP; `ASPNETCORE_HTTPS_PORTS` and the HTTPS dev-certificate volume mounts were removed from `docker-compose.yml`.

**Why:** Kestrel's HTTPS endpoint needs a trusted dev certificate, which isn't reliably available inside a container on Windows — it made the container exit almost immediately with no visible error. Beyond fixing that, this actually mirrors real production: TLS is terminated at the platform edge (e.g. Azure Application Gateway or App Service), not inside the application container itself.

---

### 9. Dedicated Postgres port (5433) for local Docker

**Decision:** the Postgres container is mapped to host port `5433`, not the default `5432`.

**Why:** a native PostgreSQL installation already listens on `5432` on the development machine and silently intercepted every connection meant for the container, producing a confusing "password authentication failed" error unrelated to this project. A dedicated port avoids that class of conflict for anyone else in the same situation.

---

### 10. Pagination/filtering as explicit field mapping, not dynamic LINQ

**Decision:** `SaleRepository.GetPagedAsync` maps `_order`/filter fields to LINQ expressions through an explicit `switch`, rather than a dynamic-LINQ library.

**Why:** the set of filterable/orderable fields is small and known (`saleNumber`, `customer`, `branch`, `isCancelled`, dates, totals). An explicit mapping is just as easy to extend, has zero extra dependencies, and — importantly — makes the supported fields something a reviewer can read directly in the code rather than infer from a generic string-parsing library.

---

### 11. Error responses reuse the existing `ApiResponse` envelope

**Decision:** validation failures, domain rule violations (`DomainException` → 400) and not-found (`KeyNotFoundException` → 404) all go through one `ExceptionHandlingMiddleware`, producing `{ success, message, errors }` — the same shape already used by `Users`/`Auth`.

**Why:** [General API](./general-api.md) sketches a different `{ type, error, detail }` error shape, but that shape was never actually implemented anywhere in this codebase. Matching the format that's real and already in use beats matching a doc that was never wired up, and keeps every endpoint in this API consistent with every other one.

---

### 12. Keep AutoMapper 13.0.1 despite the known NU1903 advisory

**Decision:** stay on AutoMapper 13.0.1 (as shipped by the template) rather than upgrading to silence the `GHSA-rvv3-g6hj-g44x` NuGet advisory.

**Why:** the advisory is a real, high-severity DoS: mapping a self-referencing object graph 25,000+ levels deep exhausts the stack and crashes the process. But every mapping in this API is a flat DTO (`Sale → SaleResult`, `Request → Command`, ...) — there's no recursive/self-referencing type anywhere in the graph, and ASP.NET Core's `System.Text.Json` already rejects a request body nested past its default `MaxDepth` (64) before it ever reaches AutoMapper. The exploit's precondition isn't reachable through this API's actual attack surface.

The fix requires AutoMapper ≥ 15.1.1 (there is no patched 13.x release), which is a 2-major-version jump, and — checked directly against the NuGet package metadata — AutoMapper moved off the permissive MIT license (13.0.1) to a custom, non-OSI license requiring explicit acceptance starting with these later versions. Adopting a different license is a decision with real consequences beyond this codebase; it isn't something to change as a drive-by side effect of clearing a build warning, and was confirmed explicitly rather than assumed.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="./sales-api.md">Previous: Sales API</a>
  <a href="../README.md">Back to README</a>
</div>
