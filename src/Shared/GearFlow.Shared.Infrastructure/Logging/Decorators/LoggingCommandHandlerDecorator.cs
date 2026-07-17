using GearFlow.Shared.Abstractions.Commands;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GearFlow.Shared.Infrastructure.Logging.Decorators;

[Decorator]
internal sealed class LoggingCommandHandlerDecorator<TCommand> : ICommandHandler<TCommand> where TCommand : class, ICommand
{
    private readonly ICommandHandler<TCommand> _commandHandler;
    private readonly ILogger<LoggingCommandHandlerDecorator<TCommand>> _logger;

    public LoggingCommandHandlerDecorator(ICommandHandler<TCommand> commandHandler, ILogger<LoggingCommandHandlerDecorator<TCommand>> logger)
    {
        _commandHandler = commandHandler;
        _logger = logger;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _commandHandler.HandleAsync(command, cancellationToken);

            _logger.LogInformation(
                "Command {CommandType} completed with outcome {Outcome} in {ElapsedMilliseconds} ms",
                commandType,
                "Succeeded",
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Command {CommandType} completed with outcome {Outcome} in {ElapsedMilliseconds} ms",
                commandType,
                "Cancelled",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                "Command {CommandType} completed with outcome {Outcome} in {ElapsedMilliseconds} ms due to {ExceptionType}",
                commandType,
                "Failed",
                stopwatch.ElapsedMilliseconds,
                exception.GetType().Name);

            throw;
        }
    }
}
