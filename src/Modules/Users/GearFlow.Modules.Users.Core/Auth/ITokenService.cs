using GearFlow.Modules.Users.Core.Entities;

namespace GearFlow.Modules.Users.Core.Auth;

public interface ITokenService
{
    string GenerateAccessToken(UserAccount user);
}
