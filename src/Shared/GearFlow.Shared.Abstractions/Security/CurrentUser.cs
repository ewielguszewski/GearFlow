using GearFlow.Shared.Abstractions.Enums;

namespace GearFlow.Shared.Abstractions.Security;

public sealed record CurrentUser(Guid UserId, Guid? CustomerId, Role Role)
{
    public bool IsCustomer() => Role == Role.Customer;
    public bool IsEmployeeOrAdmin() => Role is Role.Employee or Role.Admin;
}
