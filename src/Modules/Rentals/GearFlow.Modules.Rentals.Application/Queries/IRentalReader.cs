using GearFlow.Modules.Rentals.Application.Queries.Filters;
using GearFlow.Modules.Rentals.Domain.Entities;

namespace GearFlow.Modules.Rentals.Application.Queries;

public interface IRentalReader
{
    Task<IReadOnlyCollection<Rental>> GetAsync(RentalReadFilter filter, CancellationToken cancellationToken);
}
