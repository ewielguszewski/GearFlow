# GearFlow

GearFlow is a modular monolith backend API for equipment rental workflows.
The project models catalog browsing, item availability, reservation drafts, reservation confirmation, rental pickup/return, user authentication and role-based access.

The current implementation covers the first clickable catalog-to-return lifecycle. Payment, richer pricing, maintenance, notifications and production hardening remain future slices.

## Table of contents

- [Current status](#current-status)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Architecture decisions](#architecture-decisions)
- [Modules](#modules)
- [Reservation draft flow](#reservation-draft-flow)
- [Business rules currently implemented](#business-rules-currently-implemented)
- [Cross-module flow](#cross-module-flow)
- [Local development](#local-development)
- [Running tests](#running-tests)
- [CI](#ci)
- [Roadmap](#roadmap)
- [Notes](#notes)

## Current status

### Implemented

- Modular monolith structure
- Catalog module
- Availability module
- Reservations module
- Users/auth module
- JWT authentication with refresh tokens
- Customer, employee and admin authorization rules
- Reservation draft lifecycle
- Current draft flow for authenticated users
- Admin reservation browsing
- Offer search based on current draft period
- Item allocation and release through Availability
- Expired draft cleanup background worker
- PostgreSQL persistence
- Docker Compose local environment
- GitHub Actions CI
- Unit and integration tests
- Rental pickup and return HTTP flow for employee/admin accounts

### In progress / planned

- Rental integration tests and operational hardening
- Pricing breakdown
- Payment/deposit flow
- Damaged item handling on return
- Domain events and notifications
- Outbox processing

## Tech stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker / Docker Compose
- JWT authentication
- xUnit
- Testcontainers
- GitHub Actions

## Architecture

GearFlow is built as a modular monolith.

```text
src/
  Api/
    GearFlow.Api

  Modules/
    Catalog/
      GearFlow.Modules.Catalog.Domain
      GearFlow.Modules.Catalog.Application
      GearFlow.Modules.Catalog.Infrastructure
      GearFlow.Modules.Catalog.Contracts

    Availability/
      GearFlow.Modules.Availability.Core
      GearFlow.Modules.Availability.Application
      GearFlow.Modules.Availability.Infrastructure
      GearFlow.Modules.Availability.Contracts

    Reservations/
      GearFlow.Modules.Reservations.Domain
      GearFlow.Modules.Reservations.Application
      GearFlow.Modules.Reservations.Infrastructure
      GearFlow.Modules.Reservations.Contracts

    Rentals/
      GearFlow.Modules.Rentals.Domain
      GearFlow.Modules.Rentals.Application
      GearFlow.Modules.Rentals.Infrastructure

    Users/
      GearFlow.Modules.Users.Core

  Shared/
    GearFlow.Shared.Abstractions
    GearFlow.Shared.Infrastructure
```

The API project is only the composition root. Business logic is placed inside modules.

Each module owns its own model and persistence concerns. Cross-module communication is done through explicit contracts instead of direct access to another module's internals.

## Architecture decisions

Key design decisions are documented as ADRs in [`docs/adr`](docs/adr).

Current ADRs cover the catalog model/variant/item split, reservation draft holds with line snapshots and the Catalog-Availability contract used for offer availability.

## Modules

### Catalog

The Catalog module stores equipment models, variants and physical items.

Main concepts:

- `EquipmentModel`
- `EquipmentVariant`
- `EquipmentItem`
- `EquipmentItemStatus`

Catalog is responsible for the current rentable state of physical items and public offer data.

A variant can use either the model base price or its own overridden price. Reservation and rental lines store item snapshots so already-created business records are not affected by later catalog price changes.

### Availability

The Availability module tracks item bookings over date ranges.

Main concept:

- `ItemBooking`

The booking model already supports sources such as:

- `Reservation`
- `Rental`
- `Maintenance`
- `ManualBlock`

The current implemented flow creates reservation bookings for held items. The rental workflow consumes confirmed reservations; separate rental, maintenance and manual block availability writes remain future slices.

Availability is responsible for checking whether a physical item is already blocked in a given period. Reservation creation and modification use Availability to allocate or release item holds.

### Reservations

The Reservations module manages reservation drafts and confirmed reservations.

Main concepts:

- `Reservation`
- `ReservationLine`
- `ItemSnapshot`
- `ReservationStatus`
- `CancellationReason`
- `PaymentMethod`

Current reservation lifecycle:

```text
Draft
  -> PendingPayment
  -> Confirmed
  -> Fulfilled

Draft
  -> Cancelled
```

Draft reservations have a TTL. When the draft expires, a background worker cancels it and releases allocated item bookings.

Only one active draft per customer is allowed. Creating a new draft for the same customer cancels the previous active draft and releases its allocations.

Payment handling is simplified at this stage. Confirming a draft stores the selected payment method and marks the reservation as confirmed without a real payment provider.

### Users

The Users module handles authentication and user identity.

Supported roles:

- `Customer`
- `Employee`
- `Admin`

Customers can manage only their own reservation draft. Employees and admins can act on behalf of a target customer.

Development seed users:

```text
admin@admin.com / password
employee@employee.com / password
```

These accounts are for local development only.

## Reservation draft flow

Typical customer flow:

```text
1. Sign up / sign in
2. Create reservation draft
3. Search available offers for the draft period
4. Add offer to current draft
5. Availability allocates a specific item
6. Reservation line stores an item snapshot
7. Remove line or confirm draft
8. Expired drafts are cancelled by background cleanup
```

Current API flow:

```http
POST   /api/auth/sign-up
POST   /api/auth/sign-in
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/auth/me

POST   /api/reservations/drafts
GET    /api/reservations/drafts/current
GET    /api/reservations/drafts/current/offers
POST   /api/reservations/drafts/current/lines
DELETE /api/reservations/drafts/current/lines/{lineId}
POST   /api/reservations/drafts/current/confirm

GET    /api/reservations/upcoming
GET    /api/admin/reservations

GET    /api/admin/rentals
GET    /api/admin/rentals/{rentalId}
POST   /api/admin/rentals/from-reservation/{reservationId}
POST   /api/admin/rentals/{rentalId}/return
```

## Business rules currently implemented

- Reservation start date cannot be in the past.
- Reservation end date cannot be before start date.
- Reservation draft expires after a short TTL.
- Adding or removing a line extends the draft TTL up to a maximum limit.
- Only draft reservations can be modified.
- A customer can have only one active draft.
- Creating a new draft cancels the previous active draft.
- Reservation line currency must match reservation currency.
- Item and price facts are snapshotted into reservation and rental lines.
- Availability prevents overlapping bookings for the same item.
- Expired drafts release their reserved item allocations.
- Customers can access only their own reservation data.
- Employees and admins can work with target customer reservations.
- Admin reservation browsing requires employee/admin policy.

## Cross-module flow

Adding a reservation line uses three modules:

```text
Reservations
  -> asks Catalog for a reservable offer
  -> asks Availability to allocate a physical item
  -> stores selected item and price snapshot in ReservationLine
```

Expired draft cleanup also crosses module boundaries:

```text
Reservations
  -> finds expired drafts
  -> asks Availability to release reservation allocations
  -> marks drafts as cancelled with DraftExpired reason
```

Cross-module commands are executed inside a shared PostgreSQL transaction.

Starting a rental also crosses module boundaries:

```text
Rentals
  -> asks Reservations.Contracts for a confirmed reservation snapshot
  -> creates an active rental with line pickup state
  -> marks the source reservation as Fulfilled
```

## Local development

Requirements:

- .NET 8 SDK
- Docker
- Docker Compose

Run the application:

```bash
docker compose up --build
```

The API is exposed on:

```text
http://localhost:5000
```

Swagger is available in development mode:

```text
http://localhost:5000/swagger
```

PostgreSQL runs in Docker and is configured by `docker-compose.yml`.

In Development mode, the API applies EF migrations and seeds catalog/admin users automatically on startup.

## Running tests

Run all tests:

```bash
dotnet test GearFlow.sln
```

Run only integration tests:

```bash
dotnet test tests/Integration/GearFlow.Tests.Integration/GearFlow.Tests.Integration.csproj
```

The integration test suite covers the main reservation draft flow, item allocation/release, overlapping booking protection and expired draft cleanup.

## CI

GitHub Actions runs:

- solution restore
- release build
- unit tests
- integration tests

Workflow file:

```text
.github/workflows/ci.yml
```

## Roadmap

The first confirmed-reservation-to-rental flow is implemented. The next milestone is hardening it with integration tests, richer condition handling, availability updates after return and a complete pricing breakdown.

### Rentals module

Current basic flow:

```text
Confirmed reservation
  -> checkout
  -> active rental
  -> return inspection
  -> rental closed
```

The Reservation module should not become responsible for the full rental lifecycle. A confirmed reservation should be consumed by the Rentals module when equipment is handed over to the customer.

### Pricing

Planned pricing model:

- base rental price
- deposit
- late return fee
- damage fee
- total due
- paid amount
- remaining amount

The current implementation stores reservation totals and line prices. A richer pricing breakdown will be introduced after the rental lifecycle is established.

### Payments

Payments are intentionally simplified at this stage.

Planned direction:

- fake payment/deposit provider first
- payment intent for card payments
- cash-on-pickup support
- payment completion event
- later possibility to replace fake provider with a real external integration

### Return inspection and damaged items

Planned return flow:

```text
Rental returned
  -> inspect returned items
  -> mark items as OK, damaged or missing
  -> calculate additional charges
  -> update item availability/maintenance state
  -> publish ItemDamagedOnReturn event when needed
```

### Events and notifications

Planned domain events:

- `ReservationConfirmed`
- `RentalStarted`
- `RentalReturned`
- `ItemDamagedOnReturn`
- `PaymentCompleted`
- `PaymentFailed`

Planned notification use cases:

- reservation confirmation
- rental checkout confirmation
- return confirmation
- damaged item report
- payment reminders

Outbox processing may be added once domain events become part of the main workflow.

## Notes

This project is not intended to be a production-ready rental platform yet.

The goal is to model a realistic backend domain with clear module boundaries, testable business logic, PostgreSQL persistence and incremental architecture decisions.
