# ADR-0001: Equipment Catalog Model, Variant, and Item Boundaries

## Status

Accepted

## Context

GearFlow is a sports equipment rental system where customers should browse clean public offers instead of duplicated physical inventory rows.

A rental shop may own many physical copies of the same equipment. Some differences are customer-facing, such as ski length, snowboard length, boot size, display name, public note, or a premium/special offer price. Employees still need to manage concrete serialized items, their status, maintenance state, and internal notes.

The Catalog domain must support:

- public listing without duplicated physical items;
- customer-facing variant choices;
- physical inventory management;
- variant-level premium or special offers;
- future Availability and Maintenance modules.

## Decision

Catalog uses three separate concepts:

- `EquipmentModel` represents the public equipment family, such as brand, model name, equipment type, description, base price, and publication state.
- `EquipmentVariant` represents a customer-facing offer choice under a model, such as size, display name, public note, and optional price override.
- `EquipmentItem` represents a physical serialized copy assigned to an equipment variant.

`EquipmentVariant` belongs to `EquipmentModel` and is created through the model aggregate. A variant should not exist independently from its model.

`EquipmentItem` references an `EquipmentVariant`, but it is managed as physical inventory. Items are not the source of public catalog listing rows and should not carry public offer identity or pricing.

Public listing should be based on model and variant data, not on item rows deduplicated with `LIMIT 1`.

If a single physical copy has a customer-visible distinction, public note, special story, or different price, it should be represented as its own `EquipmentVariant`, even when that variant currently has only one `EquipmentItem`.

## Pricing

`EquipmentModel.BasePrice` is the default price for standard variants under the model.

`EquipmentVariant.PriceOverride != null` marks a variant as individually priced, premium, or special. The selected variant determines the effective reservation price:

- no override: use `EquipmentModel.BasePrice`;
- override present: use `EquipmentVariant.OverriddenPrice`.

An earlier draft considered `EquipmentItem.IndividualPrice` for premium physical items. That direction is superseded because item-level public pricing forced Reservations and Availability to reason about Catalog item details. Customer-visible and price-affecting differences belong to `EquipmentVariant`; `EquipmentItem` remains a physical operational asset.

Richer pricing policies, discounts, deposits, and seasonal pricing are outside the first Catalog slice.

## Maintenance and Rentability Facts

Catalog stores current operational facts needed for fast filtering and Availability projection, including item status and current inspection facts such as:

- `LastMaintenanceAt`;
- `NextInspectionAt`.

Full maintenance history, scheduled service windows, and repair workflows belong to a future Maintenance module.

`Active` items are rentable. Items marked as unavailable, requiring inspection, broken, lost, or retired are non-rentable.

## Open Questions

- Should equipment size remain a free-form string, or should it become an `EquipmentSize` value object?
- Should size validation depend on `EquipmentModelType`, for example ski length, boot size, clothing size, or one-size accessories?
- Should future type-specific variant specs replace the current generic `Size` field?
- Should inspection interval be global configuration, model-level policy, variant-level policy, or item-level override?
- Which module should execute scheduled inspection checks: Catalog, Maintenance, or an application-level background worker using both modules?
- Should Catalog publish item rentability changes as domain or integration events for Availability projection updates?

## Consequences

The public catalog stays clean and offer-oriented.

The domain can represent many physical copies of the same model and variant without duplicating public offers.

Availability can select concrete items behind a variant request without owning Catalog pricing rules.

Catalog remains responsible for current item facts, but does not become the owner of maintenance history or date-range service scheduling.

The model keeps room for future richer pricing, deposits, and type-specific variant rules without introducing those abstractions before they are needed.
