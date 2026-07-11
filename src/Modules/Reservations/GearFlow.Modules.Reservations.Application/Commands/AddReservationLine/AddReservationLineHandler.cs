using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Shared.Abstractions.Common;
using GearFlow.Modules.Reservations.Domain.ValueObjects;
using GearFlow.Modules.Reservations.Application.Interfaces;

namespace GearFlow.Modules.Reservations.Application.Commands.AddReservationLine;

public class AddReservationLineHandler : ICommandHandler<AddReservationLineCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICatalogOfferReader _catalogOfferReader;
    private readonly IAvailabilityAllocator _availabilityAllocator;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly IClock _clock;

    public AddReservationLineHandler(IReservationRepository reservationRepository, ICatalogOfferReader catalogOfferReader, IAvailabilityAllocator availabilityAllocator,
        IReservationAuthorizationService reservationAuthorizationService, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _catalogOfferReader = catalogOfferReader;
        _availabilityAllocator = availabilityAllocator;
        _reservationAuthorizationService = reservationAuthorizationService;
        _clock = clock;
    }

    public async Task HandleAsync(AddReservationLineCommand command, CancellationToken cancellationToken = default)
    {
        var now = _clock.Current();

        var reservation = await _reservationRepository.GetAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
            throw new ReservationNotFoundException(command.ReservationId);

        _reservationAuthorizationService.Authorize(reservation);

        if (!reservation.IsDraft)
            throw new DomainException("Only draft reservations can be modified.");

        if (reservation.IsDraftExpired(now))
            throw new DomainException("Reservation draft has expired.");

        var offer = await _catalogOfferReader.GetReservableOfferAsync(command.OfferVariantId, cancellationToken);
        if (offer is null)
            throw new OfferNotAvailableException(command.OfferVariantId);
        if (offer.ActiveItemIds == null || offer.ActiveItemIds.Count == 0)
            throw new NoAvailableItemForOfferException(command.OfferVariantId);
        
        var heldItemId = await _availabilityAllocator.TryAllocateItemAsync(offer.ActiveItemIds, offer.VariantId, reservation.Id, reservation.ReservedPeriod, cancellationToken);
        if (heldItemId is null)
            throw new NoAvailableItemForOfferException(command.OfferVariantId);

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

        // todo: Introduce explicit idempotency key for AddReservationLine.
        // ReservationLineId is currently command-provided mainly for testability.
        reservation.AddReservationLine(command.ReservationLineId, snapshot, now);
    }
}
