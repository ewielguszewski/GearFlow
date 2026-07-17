using GearFlow.Modules.Rentals.Application.Queries.DTO;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Rentals.Application.Queries.GetRental;

public sealed record GetRental(Guid RentalId) : IQuery<RentalDto?>;
