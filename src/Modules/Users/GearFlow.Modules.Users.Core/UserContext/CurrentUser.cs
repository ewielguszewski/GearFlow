using GearFlow.Modules.Users.Core.Enums;

namespace GearFlow.Modules.Users.Core.UserContext;

public sealed record CurrentUser(Guid UserId, Guid? CustomerId, Role Role)
{
    public bool IsCustomer() => Role == Role.Customer;
    public bool IsEmployeeOrAdmin() => Role is Role.Employee or Role.Admin;
}
