using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Infrastructure.Logging;
using GearFlow.Shared.Infrastructure.Logging.Decorators;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace GearFlow.Shared.Infrastructure.Tests.Unit.Logging;

public class LoggingCommandHandlerDecorator_Tests
{
    [Fact]
    public async Task successful_command_should_emit_one_safe_completion_event()
    {
        var command = new TestCommand("must-not-be-logged");
        var innerHandler = new RecordingCommandHandler(_ => Task.CompletedTask);
        var logger = new TestLogger<LoggingCommandHandlerDecorator<TestCommand>>();
        var decorator = new LoggingCommandHandlerDecorator<TestCommand>(innerHandler, logger);

        await decorator.HandleAsync(command);

        innerHandler.ReceivedCommand.ShouldBeSameAs(command);

        var entry = logger.Entries.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Information);
        entry.Properties["CommandType"].ShouldBe(nameof(TestCommand));
        entry.Properties["Outcome"].ShouldBe("Succeeded");
        entry.Properties.ContainsKey("ElapsedMilliseconds").ShouldBeTrue();
        entry.Properties.ContainsKey("Command").ShouldBeFalse();
        entry.Message.ShouldNotContain(command.Secret);
    }

    [Fact]
    public async Task failed_command_should_emit_failure_event_and_rethrow_original_exception()
    {
        var expectedException = new InvalidOperationException("Failure");
        var innerHandler = new RecordingCommandHandler(_ => Task.FromException(expectedException));
        var logger = new TestLogger<LoggingCommandHandlerDecorator<TestCommand>>();
        var decorator = new LoggingCommandHandlerDecorator<TestCommand>(innerHandler, logger);

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => decorator.HandleAsync(new TestCommand("secret")));

        exception.ShouldBeSameAs(expectedException);

        var entry = logger.Entries.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Warning);
        entry.Exception.ShouldBeNull();
        entry.Properties["Outcome"].ShouldBe("Failed");
        entry.Properties["ExceptionType"].ShouldBe(nameof(InvalidOperationException));
        entry.Properties.ContainsKey("Command").ShouldBeFalse();
    }

    [Fact]
    public async Task cancelled_command_should_emit_debug_event_and_rethrow_cancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var innerHandler = new RecordingCommandHandler(
            token => Task.FromCanceled(token));
        var logger = new TestLogger<LoggingCommandHandlerDecorator<TestCommand>>();
        var decorator = new LoggingCommandHandlerDecorator<TestCommand>(innerHandler, logger);

        await Should.ThrowAsync<OperationCanceledException>(
            () => decorator.HandleAsync(new TestCommand("secret"), cancellationTokenSource.Token));

        var entry = logger.Entries.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Debug);
        entry.Properties["Outcome"].ShouldBe("Cancelled");
        entry.Properties.ContainsKey("Command").ShouldBeFalse();
    }

    [Fact]
    public async Task logging_decorator_should_complete_after_unit_of_work_save()
    {
        var sequence = new List<string>();
        var logger = new TestLogger<LoggingCommandHandlerDecorator<TestCommand>>(
            () => sequence.Add("Log"));
        var services = new ServiceCollection();

        services.AddScoped<ICommandHandler<TestCommand>>(
            _ => new RecordingCommandHandler(_ =>
            {
                sequence.Add("Handler");
                return Task.CompletedTask;
            }));
        services.AddScoped<IUnitOfWork>(_ => new RecordingUnitOfWork(sequence));
        services.AddSingleton<ILogger<LoggingCommandHandlerDecorator<TestCommand>>>(logger);
        services.AddUoWHandlersDecorators();
        services.AddLoggingDecorators();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
        await handler.HandleAsync(new TestCommand("secret"));

        sequence.ShouldBe(["Handler", "SaveChanges", "Log"]);
    }

    private sealed record TestCommand(string Secret) : ICommand;

    private sealed class RecordingCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly Func<CancellationToken, Task> _action;

        public RecordingCommandHandler(Func<CancellationToken, Task> action)
        {
            _action = action;
        }

        public TestCommand? ReceivedCommand { get; private set; }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            ReceivedCommand = command;
            return _action(cancellationToken);
        }
    }

    private sealed class RecordingUnitOfWork : IUnitOfWork
    {
        private readonly IList<string> _sequence;

        public RecordingUnitOfWork(IList<string> sequence)
        {
            _sequence = sequence;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _sequence.Add("SaveChanges");
            return Task.CompletedTask;
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> action,
            CancellationToken cancellationToken = default)
        {
            await action();
            _sequence.Add("Commit");
        }
    }
}
