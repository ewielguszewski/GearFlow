using GearFlow.Modules.Users.Core.Entities;

namespace GearFlow.Modules.Users.Core.Repositories;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken);
}
