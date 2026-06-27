using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Shared.Infrastructure.Postgres.Decorators;

[Decorator]
public sealed class UnitOfWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerDecorator(
        ICommandHandler<TCommand> commandHandler,
        IUnitOfWork unitOfWork)
    {
        _commandHandler = commandHandler;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        if (command is ICrossModuleCommand)
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                () => _commandHandler.HandleAsync(command, cancellationToken),
                cancellationToken);

            return;
        }

        await _commandHandler.HandleAsync(command, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
