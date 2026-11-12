using CCLogAPI.Models;
using CCLogAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CCLogAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly DailyReportJob _reportJob;
    private readonly IEmailService _emailService;
    private readonly DailyReportScheduler _scheduler;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(DailyReportJob reportJob, IEmailService emailService,
        IEnumerable<IHostedService> hostedServices, ILogger<ReportsController> logger)
    {
        _reportJob = reportJob;
        _emailService = emailService;
        _scheduler = hostedServices.OfType<DailyReportScheduler>().First();
        _logger = logger;
    }

    // POST: Reports/sendtestemail
    [HttpPost("sendtestemail")]
    public async Task<IActionResult> SendTestEmail()
    {
        _logger.LogInformation("Manual test email triggered via API");
        try
        {
            await _reportJob.SendDailyReportAsync();
            return Ok(new { message = "Test email sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test email failed");
            return StatusCode(500, new { error = "Failed to send test email.", details = ex.Message });
        }
    }

    // POST: Reports/sendemail
    [HttpPost("sendemail")]
    public async Task<IActionResult> SendEmail(SendEmailRequest req)
    {
        _logger.LogInformation("SendEmail called: To={To}, Subject={Subject}", req.To, req.Subject);
        try
        {
            await _emailService.SendAsync(req.To, req.Subject, req.Body);
            return Ok(new { message = "Email sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendEmail failed: To={To}", req.To);
            return StatusCode(500, new { error = "Failed to send email.", details = ex.Message });
        }
    }

    // GET: Reports/schedulerstatus
    [HttpGet("schedulerstatus")]
    public IActionResult SchedulerStatus()
    {
        return Ok(new
        {
            serverTime     = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"),
            nextRunTime    = _scheduler.NextRunTime?.ToString("dd MMM yyyy HH:mm:ss") ?? "Not yet calculated",
            lastRunTime    = _scheduler.LastRunTime?.ToString("dd MMM yyyy HH:mm:ss") ?? "Has not run yet",
            lastRunResult  = _scheduler.LastRunResult ?? "N/A"
        });
    }

    // POST: Reports/testreminders
    [HttpPost("testreminders")]
    public async Task<IActionResult> TestReminders()
    {
        _logger.LogInformation("Manual reminder check triggered via API");
        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<ReminderCheckJob>();
            await job.RunAsync(overrideEmail: "richard.lockwood@allegion.com");
            return Ok(new { message = "Reminder check completed — all emails sent to richard.lockwood@allegion.com." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reminder check failed");
            return StatusCode(500, new { error = "Reminder check failed.", details = ex.Message });
        }
    }

    // POST: Reports/testescalations
    [HttpPost("testescalations")]
    public async Task<IActionResult> TestEscalations()
    {
        _logger.LogInformation("Manual escalation check triggered via API");
        try
        {
            using var scope = HttpContext.RequestServices.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<EscalationCheckJob>();
            await job.RunAsync(overrideEmail: "richard.lockwood@allegion.com");
            return Ok(new { message = "Escalation check completed — all emails sent to richard.lockwood@allegion.com." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Escalation check failed");
            return StatusCode(500, new { error = "Escalation check failed.", details = ex.Message });
        }
    }
}
