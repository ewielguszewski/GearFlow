using GearFlow.Api.Extensions;
using GearFlow.Modules.Availability.Infrastructure;
using GearFlow.Modules.Catalog.Infrastructure;
using GearFlow.Modules.Users.Core;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Infrastructure;
using GearFlow.Modules.Rentals.Application.Commands.StartRentalFromReservation;
using GearFlow.Modules.Rentals.Infrastructure;
using GearFlow.Shared.Infrastructure;
using GearFlow.Shared.Abstractions.Security;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting GearFlow API");

    var builder = WebApplication.CreateBuilder(args);

    builder.ConfigureSerilog();

    builder.Services.AddInfrastructure([
        typeof(CreateDraftReservationCommand).Assembly,
        typeof(StartRentalFromReservationCommand).Assembly
    ]);
    builder.Services.AddUsersCore(builder.Configuration);
    builder.Services.AddCatalogModule();
    builder.Services.AddAvailabilityModule();
    builder.Services.AddReservationsModule();
    builder.Services.AddRentalsModule();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContext, UserContext>();

    var app = builder.Build();

    app.UseSerilog();
    app.UseInfrastructure();

    if (app.Environment.IsDevelopment())
    {
        await app.InitializeDevelopmentDatabaseAsync();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "GearFlow API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
