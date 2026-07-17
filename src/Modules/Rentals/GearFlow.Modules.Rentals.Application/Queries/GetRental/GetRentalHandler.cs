using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Modules.Rentals.Application.Queries.Filters;
using GearFlow.Modules.Rentals.Application.Queries.Mappers;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Rentals.Application.Queries.GetRental;

public sealed class GetRentalHandler : IQueryHandler<GetRental, RentalDto?>
{
    private readonly IRentalReader _rentalReader;

    public GetRentalHandler(IRentalReader rentalReader)
    {
        _rentalReader = rentalReader;
    }

    public async Task<RentalDto?> HandleAsync(GetRental query, CancellationToken cancellationToken = default)
    {
        var rentals = await _rentalReader.GetAsync(new RentalReadFilter
        {
            RentalId = query.RentalId
        }, cancellationToken);

        return rentals.SingleOrDefault()?.ToRentalDto();
    }
}
