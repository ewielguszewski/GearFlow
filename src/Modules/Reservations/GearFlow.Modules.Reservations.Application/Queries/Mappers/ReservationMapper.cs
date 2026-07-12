using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Modules.Reservations.Domain.Entities;

namespace GearFlow.Modules.Reservations.Application.Queries.Mappers;

public static class ReservationMapper
{
    public static ReservationDraftDto ToReservationDraftDto(this Reservation reservation, bool isExpired)
    {
        return new ReservationDraftDto
        (
            reservation.Id,
            reservation.CustomerId,
            reservation.Status.ToString(),
            reservation.ReservedPeriod.Start,
            reservation.ReservedPeriod.End,
            reservation.TtlExpiresAt,
            isExpired,
            reservation.Currency.ToString(),
            reservation.TotalPrice.Amount,
            reservation.ReservationLines.Select(ri => new ReservedItemDto
            (
                ri.Id,
                ri.Item.VariantId,
                ri.Item.Model,
                ri.Item.Brand,
                ri.Item.UnitPrice.Amount,
                ri.LineTotalPrice.Amount
            )).ToList()
        );
    }

    public static IEnumerable<ReservationDraftDto> ToReservationDraftDtos(this IEnumerable<Reservation> reservations, DateTime currentTime)
    {
        return reservations.Select(reservation => reservation.ToReservationDraftDto(reservation.IsDraftExpired(currentTime)));
    }

    public static UpcomingReservationDto ToUpcomingReservationDto(this Reservation reservation)
    {
        return new UpcomingReservationDto
        (
            reservation.Id,
            reservation.CustomerId,
            reservation.ReservedPeriod.Start,
            reservation.ReservedPeriod.End,
            reservation.Currency.ToString(),
            reservation.TotalPrice.Amount,
            reservation.ReservationLines.Select(ri => new ReservedItemDto
            (
                ri.Id,
                ri.Item.VariantId,
                ri.Item.Model,
                ri.Item.Brand,
                ri.Item.UnitPrice.Amount,
                ri.LineTotalPrice.Amount
            )).ToList()
        );
    }

    public static IEnumerable<UpcomingReservationDto> ToUpcomingReservationDtos(this IEnumerable<Reservation> reservations)
    {
        return reservations.Select(reservation => reservation.ToUpcomingReservationDto());
    }

    public static AdminReservationDto ToAdminReservationDto(this Reservation reservation)
    {
        return new AdminReservationDto
        (
            reservation.Id,
            reservation.CustomerId,
            reservation.Status.ToString(),
            reservation.CancReason?.ToString(),
            reservation.ReservedPeriod.Start,
            reservation.ReservedPeriod.End,
            reservation.Currency.ToString(),
            reservation.TotalPrice.Amount,
            reservation.ReservationLines.Select(ri => new ReservedItemDto
            (
                ri.Id,
                ri.Item.VariantId,
                ri.Item.Model,
                ri.Item.Brand,
                ri.Item.UnitPrice.Amount,
                ri.LineTotalPrice.Amount
            )).ToList()
        );
    }

    public static IEnumerable<AdminReservationDto> ToAdminReservationDtos(this IEnumerable<Reservation> reservations)
    {
        return reservations.Select(reservation => reservation.ToAdminReservationDto());
    }
}
