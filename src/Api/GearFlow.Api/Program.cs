using GearFlow.Modules.Availability.Infrastructure;
using GearFlow.Modules.Catalog.Infrastructure;
using GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;
using GearFlow.Modules.Reservations.Infrastructure;
using GearFlow.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure([
    typeof(CreateDraftReservationCommand).Assembly
]);
builder.Services.AddCatalogModule();
builder.Services.AddAvailabilityModule();
builder.Services.AddReservationsModule();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
