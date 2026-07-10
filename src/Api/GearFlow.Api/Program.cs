using GearFlow.Api.Extensions;
using GearFlow.Modules.Availability.Infrastructure;
using GearFlow.Modules.Catalog.Infrastructure;
using GearFlow.Modules.Users.Core;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Infrastructure;
using GearFlow.Shared.Infrastructure;
using GearFlow.Modules.Users.Core.UserContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure([
    typeof(CreateDraftReservationCommand).Assembly
]);
builder.Services.AddUsersCore(builder.Configuration);
builder.Services.AddCatalogModule();
builder.Services.AddAvailabilityModule();
builder.Services.AddReservationsModule();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

var app = builder.Build();

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

app.Run();

public partial class Program;
