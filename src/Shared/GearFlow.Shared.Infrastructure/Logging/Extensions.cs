using GearFlow.Shared.Abstractions.Commands;
using GearFlow.Shared.Infrastructure.Logging.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace GearFlow.Shared.Infrastructure.Logging;

internal static class Extensions
{
    public static IServiceCollection AddLoggingDecorators(this IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));

        return services;
    }
}
