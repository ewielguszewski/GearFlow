using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Catalog.Contracts;

namespace GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;

public class AddReservationLineHandler : ICommandHandler<AddReservationLineCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICatalogOfferReader _catalogOfferReader;
    private readonly IAvailabilityAllocator _availabilityAllocator;
    private readonly IClock _clock;

    public AddReservationLineHandler(IReservationRepository reservationRepository, ICatalogOfferReader catalogOfferReader, IAvailabilityAllocator availabilityAllocator, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _catalogOfferReader = catalogOfferReader;
        _availabilityAllocator = availabilityAllocator;
        _clock = clock;
    }

    public async Task HandleAsync(AddReservationLineCommand command, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
            throw new ReservationNotFoundException(command.ReservationId);

        var offer = await _catalogOfferReader.GetReservableOfferVariantAsync(command.OfferVariantId, cancellationToken);
        if (offer is null)
            throw new OfferVariantNotAvailableException(command.OfferVariantId);

        // todo: Wrap Catalog validation, Availability allocation, and Reservation update in one Unit of Work once persistence is introduced.
        var heldItemId = await _availabilityAllocator.AllocateItemAsync(offer.VariantId, reservation.ReservedPeriod, command.ReservationLineId, cancellationToken);
        if (heldItemId is null)
            throw new NoAvailableItemForOfferVariantException(command.OfferVariantId);

        var snapshot = new OfferSnapshot
        {
            ItemId = heldItemId.Value,
            VariantId = offer.VariantId,
            VariantName = offer.VariantName,
            Brand = offer.Brand,
            Model = offer.Model,
            PublicNote = offer.PublicNote,
            UnitPrice = offer.OverridenPrice ?? offer.BasePrice,
            PriceSource = offer.OverridenPrice == null? PriceSource.CatalogModel : PriceSource.CatalogVariantOverride,
            Size = offer.Size
        };

        reservation.AddReservationLine(command.ReservationLineId, snapshot, _clock.Current());

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
    }
}