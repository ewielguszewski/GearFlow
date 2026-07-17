using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Contracts.Readers;
using GearFlow.Modules.Reservations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Reservations.Infrastructure.DAL.Readers;

internal sealed class ReservationRentalReader : IReservationRentalReader, IReservationRentalFulfillment
{
    private readonly ReservationsDbContext _dbContext;

    public ReservationRentalReader(ReservationsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservationForRental?> GetConfirmedReservationAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations
            .Include(x => x.ReservationLines)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == reservationId && x.Status == ReservationStatus.Confirmed,
                cancellationToken);

        if (reservation is null)
            return null;

        return new ReservationForRental(
            reservation.Id,
            reservation.CustomerId,
            reservation.ReservedPeriod,
            reservation.Currency,
            reservation.ReservationLines.Select(line => new ReservationLineForRental(
                line.Id,
                line.Item.ItemId,
                line.Item.VariantId,
                line.Item.VariantName,
                line.Item.Brand,
                line.Item.Model,
                line.Item.PublicNote,
                line.Item.UnitPrice,
                line.Item.PriceSource,
                line.Item.Size,
                line.LineTotalPrice)).ToArray());
    }

    public async Task MarkAsFulfilledAsync(Guid reservationId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var reservation = await _dbContext.Reservations
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);

        if (reservation is null || reservation.Status != ReservationStatus.Confirmed)
            throw new ReservationNotFoundException(reservationId);

        reservation.MarkAsFulfilled();
        reservation.SetUpdatedAt(utcNow);
    }
}
