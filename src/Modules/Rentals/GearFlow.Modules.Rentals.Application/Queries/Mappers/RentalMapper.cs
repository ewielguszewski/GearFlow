using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Modules.Rentals.Domain.Entities;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Application.Queries.Mappers;

public static class RentalMapper
{
    public static RentalDto ToRentalDto(this Rental rental)
    {
        var damageFeeTotal = rental.RentalLines
            .Select(x => x.DamageFee)
            .Aggregate(Money.ZeroFromCurrency(rental.Currency), (total, fee) => total.Add(fee));

        return new RentalDto(
            rental.Id,
            rental.ReservationId,
            rental.CustomerId,
            rental.Status.ToString(),
            rental.RentalPeriod.Start,
            rental.RentalPeriod.End,
            rental.PickedUpAt,
            rental.ReturnedAt,
            rental.Currency.ToString(),
            rental.TotalPrice.Amount,
            rental.LateFee.Amount,
            damageFeeTotal.Amount,
            rental.RentalLines.Select(line => new RentalLineDto(
                line.Id,
                line.ReservationLineId,
                line.Item.ItemId,
                line.Item.VariantId,
                line.Item.Brand,
                line.Item.Model,
                line.Item.Size,
                line.Item.VariantName,
                line.LineTotalPrice.Amount,
                line.PickupCondition.ToString(),
                line.ReturnCondition?.ToString(),
                line.DamageFee.Amount)).ToArray());
    }

    public static IEnumerable<RentalDto> ToRentalDtos(this IEnumerable<Rental> rentals)
        => rentals.Select(x => x.ToRentalDto());
}
