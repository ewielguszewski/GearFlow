using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.CreateDraftReservation;

public sealed record CreateDraftReservationCommand(Guid Id, Guid CustomerId, DateTime From, DateTime To, string Currency) : ICrossModuleCommand;
