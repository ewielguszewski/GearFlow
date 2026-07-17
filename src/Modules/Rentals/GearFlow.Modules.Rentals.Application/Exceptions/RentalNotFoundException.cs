using GearFlow.Shared.Abstractions.Common;

namespace GearFlow.Modules.Rentals.Application.Exceptions;

public sealed class RentalNotFoundException(Guid rentalId)
    : NotFoundException($"Rental '{rentalId}' was not found.");
