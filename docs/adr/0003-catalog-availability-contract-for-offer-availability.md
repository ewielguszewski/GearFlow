# ADR 0003: Catalog-Availability contract for offer availability

## Context

Catalog owns offer definitions, variants and physical equipment items.
Availability owns time-based item bookings and allocation rules.

To calculate bookable offers and allocate an item for a reservation draft, Availability needs to know which physical items are currently eligible for booking.

Initially, passing only `ActiveItemCount` from Catalog seemed sufficient for the available offers query.
However, this is not enough because Availability may contain bookings for items that are no longer active, damaged, retired or otherwise notcurrently reservable.

## Decision

For the MVP, Catalog exposes active item ids through internal application contracts.

This applies to:

- `GetAvailableOffers` query, where Availability calculates available counts only
  for active item ids passed by Catalog.
- `AddReservationLine` command, where Availability receives active item ids for
  the selected variant, chooses one available item, and creates a draft hold.

Availability remains the owner of time-based booking records and allocation.
Catalog remains the owner of equipment item status and catalog/offer data.

## Consequences

### Positive

- Availability counts only currently reservable physical items.
- We avoid introducing Availability-owned item projections too early.
- The implementation remains simple enough for the first vertical slice.
- Allocation still happens inside Availability, not in Reservations or Catalog.

### Negative

- Physical item ids leak through module contracts.
- Catalog and Availability are more coupled than the target design.
- Available offer paging is harder because final filtering depends on both modules.
- Allocation depends on a fresh Catalog read.

## Future direction

Introduce an Availability-owned projection of bookable items:

- `ItemId`
- `VariantId`
- `IsBookable`
- relevant operational status
- optionally location/warehouse in the future

This projection can later be updated from events such as:

- `EquipmentItemActivated`
- `EquipmentItemDamaged`
- `EquipmentItemRetired`

After that, `GetAvailableOffers` can pass only variant ids or criteria, and `AddReservationLine` will no longer need active item ids from Catalog.
