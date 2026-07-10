using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Modules.Users.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GearFlow.Modules.Users.Core.DAL.Repositories;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UsersDbContext _dbContext;

    public RefreshTokenRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(RefreshToken refreshToken)
        => _dbContext.RefreshTokens.Add(refreshToken);

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
        => _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var rt = await GetByTokenAsync(refreshToken, cancellationToken);
        rt?.Revoke();
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToArrayAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }
    }
}
