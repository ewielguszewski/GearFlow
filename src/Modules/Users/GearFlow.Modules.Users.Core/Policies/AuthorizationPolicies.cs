namespace GearFlow.Modules.Users.Core.Policies;

public static class AuthorizationPolicies
{
    public const string Admin = nameof(Admin);
    public const string EmployeeOrAdmin = nameof(EmployeeOrAdmin);
    public const string Customer = nameof(Customer);
}