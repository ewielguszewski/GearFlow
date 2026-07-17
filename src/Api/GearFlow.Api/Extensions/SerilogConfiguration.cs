using Serilog;
using Serilog.Events;

namespace GearFlow.Api.Extensions;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "GearFlow.Api")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .WriteTo.Console(new Serilog.Formatting.Compact.RenderedCompactJsonFormatter());

            var seqServerUrl = context.Configuration["Seq:ServerUrl"];

            if (!string.IsNullOrWhiteSpace(seqServerUrl))
                configuration.WriteTo.Seq(seqServerUrl);
        });
    }

    public static void UseSerilog(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (context, _, exception) =>
            {
                var isSuccessfulHealthCheck = context.Request.Path.StartsWithSegments("/health")
                    && exception is null
                    && context.Response.StatusCode < StatusCodes.Status500InternalServerError;

                if (isSuccessfulHealthCheck)
                    return LogEventLevel.Debug;

                return exception is not null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            };
        });
    }
}
