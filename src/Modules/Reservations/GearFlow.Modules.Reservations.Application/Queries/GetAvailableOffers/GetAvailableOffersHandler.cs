using GearFlow.Modules.Availability.Contracts;
using GearFlow.Modules.Catalog.Contracts;
using GearFlow.Modules.Reservations.Application.Exceptions;
using GearFlow.Modules.Reservations.Application.Interfaces;
using GearFlow.Modules.Reservations.Domain.Repositories;
using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.Time;

namespace GearFlow.Modules.Reservations.Application.Queries.GetAvailableOffers;

public sealed class GetAvailableOffersHandler : IQueryHandler<GetAvailableOffers, IEnumerable<AvailableOfferResult>>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuthorizationService _reservationAuthorizationService;
    private readonly ICatalogOfferReader _catalogOfferReader;
    private readonly IAvailabilityReader _availabilityReader;
    private readonly IClock _clock;

    public GetAvailableOffersHandler(IReservationRepository reservationRepository, IReservationAuthorizationService reservationAuthorizationService, 
        ICatalogOfferReader catalogOfferReader, IAvailabilityReader availabilityReader, IClock clock)
    {
        _reservationRepository = reservationRepository;
        _reservationAuthorizationService = reservationAuthorizationService;
        _catalogOfferReader = catalogOfferReader;
        _availabilityReader = availabilityReader;
        _clock = clock;
    }

    public async Task<IEnumerable<AvailableOfferResult>> HandleAsync(GetAvailableOffers query, CancellationToken cancellationToken = default)
    {
        var now = _clock.Current();

        var customerId = _reservationAuthorizationService.ResolveCustomerId(query.TargetCustomerId);

        var reservation = await _reservationRepository.GetDraftByCustomerIdAsync(customerId, cancellationToken);
        if (reservation == null)
            throw new ReservationNotFoundException(null);

        _reservationAuthorizationService.Authorize(reservation);

        reservation.EnsureDraftNotExpired(now);

        var criteria = new OfferSearchCriteria
        {
            Type = query.Type,
            Brand = query.Brand,
            Model = query.Model,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            Size = query.Size,
        };

        var candidates = await _catalogOfferReader.SearchOfferCandidatesAsync(criteria, cancellationToken);
        if (candidates.Count == 0)
            return [];

        var availabilityCandidates = candidates
            .Select(c => new VariantAvailabilityCandidate(
                c.VariantId,
                c.ActiveItemIds))
            .ToArray();

        var availableCounts = await _availabilityReader.GetAvailableCountsByVariantAsync(availabilityCandidates, reservation.ReservedPeriod, cancellationToken);

        return candidates
            .Select(c =>
            {
                var availableCount = availableCounts.GetValueOrDefault(c.VariantId);

                return new AvailableOfferResult(
                    c.VariantId,
                    c.Brand,
                    c.Model,
                    c.Type,
                    c.PricePerDay,
                    c.Size,
                    availableCount);
            })
            .Where(x => x.AvailableCount > 0)
            .ToArray();
    }
}

// temporary contract: Catalog provides active item ids for allocation. Availability still owns item selection and draft hold creation.
// ADR-0003