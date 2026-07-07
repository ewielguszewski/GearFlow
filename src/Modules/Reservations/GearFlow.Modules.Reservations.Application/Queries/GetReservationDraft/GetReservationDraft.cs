using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Reservations.Application.Queries.GetReservationDraft;

public sealed record GetReservationDraft(Guid DraftId) : IQuery<ReservationDraftDto?>;