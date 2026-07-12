using GearFlow.Modules.Reservations.Application.Queries.DTO;
using GearFlow.Shared.Abstractions.Queries;

namespace GearFlow.Modules.Reservations.Application.Queries.GetCurrentReservationDraft;

public sealed record GetCurrentReservationDraft(Guid? TargetCustomerId) : IQuery<ReservationDraftDto?>;
