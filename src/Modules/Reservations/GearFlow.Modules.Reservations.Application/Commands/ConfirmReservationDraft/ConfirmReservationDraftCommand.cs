using GearFlow.Modules.Reservations.Domain.Entities;
using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.ConfirmReservationDraft;

public sealed record ConfirmReservationDraftCommand(Guid? TargetCustomerId, PaymentMethod PaymentMethod) : ICommand;
