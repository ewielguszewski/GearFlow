# ADR-0002: Reservation Draft Holds and Line Snapshots

## Status

Accepted

## Context

GearFlow reservations start as a customer cart (draft), but selected equipment must be held so another customer cannot reserve the same physical item for an overlapping period.

The customer selects a public offer variant, while the system holds a concrete item internally. Reservation history must remain understandable even if Catalog data changes later.

## Decision

A draft reservation acts as the cart and temporary hold. Draft reservations are editable only while their TTL is valid.

The first slice keeps TTL policy in the domain:

- initial hold: 5 minutes;
- hold extension after line changes: 3 minutes;
- maximum lifetime: 15 minutes from creation.

`ReservationLine` stores a snapshot of the selected offer, variant, and held item. Reservations do not reference Catalog entities directly.

Snapshot identifiers use reservation-facing names:

- `OfferVariantId` for the selected customer-facing variant;
- `ItemId` for the concrete item held internally.

The snapshot stores customer-facing text and values needed for historical display, such as brand, model, size, variant display name, public note, unit price, and price source.

Prices are snapshotted on the line. Later Catalog price changes must not mutate existing reservations.

Reservation price metadata should use a local Reservations enum, not raw Catalog state:

- `PriceSource.CatalogModel` when the line uses `EquipmentModel.BasePrice`;
- `PriceSource.CatalogVariantOverride` when the line uses `EquipmentVariant.OverriddenPrice`.

The first slice does not need a separate customer price-preference enum. Public price differences are represented as selected variants, so Availability receives the selected `OfferVariantId` and allocates a rentable item under that variant.

Adding a reservation line is an application use case that coordinates:

- Reservations: load draft reservation and add the line;
- Catalog: validate that the selected variant is reservable and return snapshot/price data;
- Availability: atomically allocate a concrete item for the variant and reservation period.

## Consequences

Reservation history can show what the customer reserved at the time of booking.

The customer UI can navigate back to the public offer/variant, while employee tools can navigate to the held physical item.

Reservations remain decoupled from Catalog entities while still storing enough identifiers for navigation.

The first slice keeps payment, deposits, currency conversion, and cancellation policy intentionally simple.

Catalog read, Availability allocation, and Reservation update should be wrapped in one Unit of Work/database transaction while these modules share one database.
If the modules were split into independent services later, the add-line use case would need explicit idempotency, retries, timeouts, outbox/inbox processing, and compensation/release handling for allocated items.

## Open Questions

- What is the final cancellation policy for confirmed or fulfilled reservations?
- How should deposits and payment confirmation affect `PendingPayment` TTL?
- Should future multi-currency support convert line prices before reservation creation or reject mixed currencies?
- Should `ItemId` ever be reassigned before pickup if the original item becomes unavailable?
- Should physical identifiers such as serial number or asset tag be snapshotted only in Rentals/pickup, not Reservations?
