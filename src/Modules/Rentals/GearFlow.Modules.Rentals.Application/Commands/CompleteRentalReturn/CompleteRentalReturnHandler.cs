using GearFlow.Modules.Rentals.Application.Exceptions;
using GearFlow.Modules.Rentals.Domain.Repositories;
using GearFlow.Modules.Rentals.Domain.ValueObjects;
using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Abstractions.ValueObjects;

namespace GearFlow.Modules.Rentals.Application.Commands.CompleteRentalReturn;

public sealed class CompleteRentalReturnHandler : ICommandHandler<CompleteRentalReturnCommand>
{
    private readonly IRentalRepository _rentalRepository;
    private readonly IClock _clock;

    public CompleteRentalReturnHandler(IRentalRepository rentalRepository, IClock clock)
    {
        _rentalRepository = rentalRepository;
        _clock = clock;
    }

    public async Task HandleAsync(CompleteRentalReturnCommand command, CancellationToken cancellationToken = default)
    {
        var rental = await _rentalRepository.GetAsync(command.RentalId, cancellationToken)
                     ?? throw new RentalNotFoundException(command.RentalId);

        var returnedLines = command.Lines
            .Select(line => new RentalLineReturn(
                line.RentalLineId,
                line.Condition,
                line.Note,
                Money.Create(line.DamageFeeAmount, rental.Currency)))
            .ToArray();

        rental.CompleteReturn(
            returnedLines,
            Money.Create(command.LateFeeAmount, rental.Currency),
            _clock.Current());

        _rentalRepository.Update(rental);
    }
}
