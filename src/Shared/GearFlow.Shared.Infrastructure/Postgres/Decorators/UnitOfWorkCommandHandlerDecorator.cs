using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Shared.Infrastructure.Postgres.Decorators;

public class UnitOfWorkCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand> where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkCommandHandlerDecorator(ICommandHandler<TCommand> commandHandler, IUnitOfWork unitOfWork)
    {
        _commandHandler = commandHandler;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteAsync(() => _commandHandler.HandleAsync(command, cancellationToken), cancellationToken);
    }
}
