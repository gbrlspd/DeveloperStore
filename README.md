# Developer Evaluation Project

[![CI](https://github.com/gbrlspd/DeveloperStore/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/gbrlspd/DeveloperStore/actions/workflows/ci.yml)

## Implementation Summary

This repository implements the **Sales API** described in the [Use Case](#use-case) below, on top of the provided `Ambev.DeveloperEvaluation` template: .NET 8, Clean Architecture (Domain/Application/ORM/WebApi), DDD with the External Identities pattern, PostgreSQL via EF Core, and Sale domain events published through Rebus (in-memory transport, no external broker required).

Architecture decisions and their rationale are recorded in [Architecture Decisions](./.doc/decisions.md).

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL, and optionally to run the API itself)
- The [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) tool, only if you need to (re)generate migrations: `dotnet tool install --global dotnet-ef`

### Configuration

The connection string lives in `src/Ambev.DeveloperEvaluation.WebApi/appsettings.json`:

```
Host=localhost;Port=5433;Database=developer_evaluation;Username=developer;Password=ev@luAt10n
```

> **Why port 5433 and not the default 5432:** if you already have a native PostgreSQL installation on your machine, it normally occupies port 5432 and will silently intercept any connection meant for the container, producing a confusing "password authentication failed" error. The Docker Postgres in this repo is mapped to `5433` to avoid that conflict — see [Architecture Decisions](./.doc/decisions.md) for details. If port 5433 is unavailable on your machine, change the mapping in `docker-compose.yml` and the port in the connection string above to match.

### Running everything with Docker

```bash
docker compose up
```

This starts PostgreSQL, MongoDB, Redis (all pre-existing in the template, only Postgres is actually used by Sales) and the WebApi itself. Once it's up:

- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

The container serves plain HTTP — TLS is intentionally not configured inside the container (see decision #8 in [Architecture Decisions](./.doc/decisions.md)).

### Running the API locally (faster inner loop)

Only the database needs to be containerized; run the API directly for hot-reload/debugging:

```bash
docker compose up -d ambev.developerevaluation.database
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

Swagger will be available at the URL printed in the console (see `src/Ambev.DeveloperEvaluation.WebApi/Properties/launchSettings.json`).

### Applying database migrations

Migrations are generated already; to apply them to your local Postgres:

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

### Running tests

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj
```

No database is required — every test substitutes the repository/mapper/event publisher. This is the same command the [CI workflow](./.github/workflows/ci.yml) runs on every push/PR.

### Trying the API manually

- **Swagger**: browse to `/swagger` on either running mode above.
- **Postman**: import [`.doc/DeveloperStore.postman_collection.json`](./.doc/DeveloperStore.postman_collection.json) — the base URL is a collection variable (`baseUrl`, defaults to `http://localhost:5100`; adjust it to `8080` if you're using `docker compose up`). Creating a sale automatically captures its id/item id into collection variables for the requests below it.

## API Documentation

- [Sales API](./.doc/sales-api.md) — every endpoint, request/response payloads, filtering/ordering/pagination, error format.
- [General API conventions](./.doc/general-api.md) — pagination, filtering and ordering conventions this API follows.
- [Architecture Decisions](./.doc/decisions.md) — the reasoning behind the choices above.

## What I Would Improve With More Time

- **Integration tests** for `SaleRepository` against a real (containerized) Postgres, and a couple of functional/end-to-end tests hitting the API through `WebApplicationFactory` — the `Integration`/`Functional` test projects exist in the template but are currently empty; Unit tests were prioritized per the challenge's own guidance.
- **Authentication** on the Sales endpoints. The template ships a working JWT/`User` flow, but the challenge didn't ask for Sales to be protected by it, so it was left open to keep scope focused; wiring `[Authorize]` on top is a small, well-isolated addition.
- **Idempotency** for `POST /api/sales` (e.g. an idempotency key header), since retried requests from a flaky client could otherwise attempt to create duplicate sales sharing the same `saleNumber` and fail on the unique constraint rather than returning the original result.
- **Outbox pattern** for the domain events: today they're published in the same request after `SaveChangesAsync`, so a crash between the two isn't atomic. A transactional outbox would close that gap before pointing `MessagingModuleInitializer` at a real broker.

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

## API Structure
This section includes links to the detailed documentation for the Sales API:
- [General API](/.doc/general-api.md)
- [Sales API](/.doc/sales-api.md)
- [Architecture Decisions](/.doc/decisions.md)

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)
