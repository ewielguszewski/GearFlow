using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace GearFlow.Shared.Infrastructure.Postgres;

public sealed class EfPostgresUnitOfWork : IUnitOfWork
{
    private readonly NpgsqlConnection _connection;
    private readonly IReadOnlyCollection<DbContext> _dbContexts;

    public EfPostgresUnitOfWork(NpgsqlConnection connection, IEnumerable<DbContext> dbContexts)
    {
        _connection = connection;
        _dbContexts = dbContexts
            .Distinct()
            .ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangedContextsAsync(allowMultipleContexts: false, cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(cancellationToken);

        await using var transaction = await _connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        try
        {
            foreach (var dbContext in _dbContexts)
            {
                await dbContext.Database.UseTransactionAsync(transaction, cancellationToken);
            }

            await action();
            await SaveChangedContextsAsync(allowMultipleContexts: true, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            foreach (var dbContext in _dbContexts)
            {
                await dbContext.Database.UseTransactionAsync(null, cancellationToken);
            }
        }
    }

    private async Task SaveChangedContextsAsync(bool allowMultipleContexts, CancellationToken cancellationToken)
    {
        var changedContexts = _dbContexts
            .Where(dbContext => dbContext.ChangeTracker.HasChanges())
            .ToArray();

        if (!allowMultipleContexts && changedContexts.Length > 1)
        {
            throw new InvalidOperationException(
                "A command modified multiple module DbContexts without implementing ICrossModuleCommand.");
        }

        foreach (var dbContext in changedContexts)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
