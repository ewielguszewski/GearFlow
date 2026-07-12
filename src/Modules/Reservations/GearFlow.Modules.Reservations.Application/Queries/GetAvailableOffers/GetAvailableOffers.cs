using GearFlow.Shared.Abstractions.Queries;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Reservations.Application.Queries.GetAvailableOffers;

public sealed record GetAvailableOffers(
    Guid? TargetCustomerId,
    string? Type,
    string? Brand,
    string? Model,
    Money? MinPrice,
    Money? MaxPrice,
    string? Size,

    int Page,
    int PageSize
    ) : IQuery<IEnumerable<AvailableOfferResult>>;