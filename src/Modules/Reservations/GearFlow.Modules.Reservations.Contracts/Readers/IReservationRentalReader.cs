using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Contracts.Readers;

public interface IReservationRentalReader
{
    Task<ReservationForRental?> GetConfirmedReservationAsync(Guid reservationId, CancellationToken cancellationToken);
}

public interface IReservationRentalFulfillment
{
    Task MarkAsFulfilledAsync(Guid reservationId, DateTime utcNow, CancellationToken cancellationToken);
}

public sealed record ReservationForRental(
    Guid Id,
    Guid CustomerId,
    DateRange ReservedPeriod,
    CurrencyCode Currency,
    IReadOnlyCollection<ReservationLineForRental> Lines);

public sealed record ReservationLineForRental(
    Guid Id,
    Guid ItemId,
    Guid VariantId,
    string? VariantName,
    string Brand,
    string Model,
    string? PublicNote,
    Money UnitPrice,
    PriceSource PriceSource,
    string? Size,
    Money LineTotalPrice);
