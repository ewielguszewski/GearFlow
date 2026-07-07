using GearFlow.Shared.Abstractions.Commands;

namespace GearFlow.Modules.Reservations.Application.Commands.ConfirmReservationDraft;

public sealed record ConfirmReservationDraftCommand(Guid draftId, string PaymentMethod) : ICommand;
