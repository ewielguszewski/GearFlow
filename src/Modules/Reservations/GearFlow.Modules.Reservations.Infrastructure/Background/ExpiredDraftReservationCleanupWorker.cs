using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace GearFlow.Modules.Reservations.Infrastructure.Background;

internal sealed class ExpiredDraftReservationCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<ReservationExpiryCleanupOptions> _options;
    private readonly ILogger<ExpiredDraftReservationCleanupWorker> _logger;

    public ExpiredDraftReservationCleanupWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ReservationExpiryCleanupOptions> options,
        ILogger<ExpiredDraftReservationCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.Value;

        if (!options.Enabled)
        {
            _logger.LogInformation("Expired reservation draft cleanup worker is disabled.");
            return;
        }

        try
        {
            if (options.InitialDelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(options.InitialDelaySeconds), stoppingToken);

            await ProcessExpiredDraftsAsync(stoppingToken);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.IntervalSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessExpiredDraftsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessExpiredDraftsAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting expired reservation drafts cleanup");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IExpiredDraftReservationProcessor>();
            var processedCount = await processor.ProcessExpiredDraftsAsync(cancellationToken);

            stopwatch.Stop();

            if (processedCount > 0)
            {
                _logger.LogInformation(
                    "Expired reservation draft cleanup completed. Cancelled {ProcessedCount} drafts in {ElapsedMiliseconds} ms.",
                    processedCount, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(exception, "Expired reservation draft cleanup failed after {ElapsedMiliseconds}.", stopwatch.ElapsedMilliseconds);
        }
    }
}