namespace CCLogAPI.Services;

public class DailyReportScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyReportScheduler> _logger;

    // Time of day to send the report
    private static readonly TimeOnly SendTime = new(11, 30);

    public DateTime? NextRunTime { get; private set; }
    public DateTime? LastRunTime { get; private set; }
    public string? LastRunResult { get; private set; }

    public DailyReportScheduler(IServiceScopeFactory scopeFactory, ILogger<DailyReportScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyReportScheduler started. Will send daily at {SendTime}", SendTime);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextRun();
            NextRunTime = DateTime.Now.Add(delay);
            _logger.LogInformation("DailyReportScheduler: next run in {Delay:hh\\:mm\\:ss} at {RunTime:dd MMM yyyy HH:mm}",
                delay, NextRunTime);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await RunReportAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DailyReportScheduler: unhandled exception during report send");
            }
        }

        _logger.LogInformation("DailyReportScheduler stopped");
    }

    private async Task RunReportAsync()
    {
        LastRunTime = DateTime.Now;
        LastRunResult = "Running";
        _logger.LogInformation("DailyReportScheduler: running jobs at {Time}", LastRunTime);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var reportJob = scope.ServiceProvider.GetRequiredService<DailyReportJob>();
            await reportJob.SendDailyReportAsync();

            var reminderJob = scope.ServiceProvider.GetRequiredService<ReminderCheckJob>();
            await reminderJob.RunAsync();

            var escalationJob = scope.ServiceProvider.GetRequiredService<EscalationCheckJob>();
            await escalationJob.RunAsync();

            LastRunResult = "Success";
        }
        catch (Exception ex)
        {
            LastRunResult = $"Failed: {ex.Message}";
            throw;
        }
    }

    private static TimeSpan TimeUntilNextRun()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.Add(SendTime.ToTimeSpan());

        // If today's send time has already passed, schedule for tomorrow
        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }
}
