using CCLogAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CCLogAPI.Services;

public class ReminderCheckJob
{
    private const int SignedOffStatusId = 8;

    private readonly CCLogContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderCheckJob> _logger;
    private readonly IConfiguration _configuration;

    public ReminderCheckJob(CCLogContext context, IEmailService emailService, ILogger<ReminderCheckJob> logger, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task RunAsync(string overrideEmail = null)
    {
        _logger.LogInformation("ReminderCheckJob: starting{Override}",
            overrideEmail != null ? $" (test mode – emails redirected to {overrideEmail})" : string.Empty);

        var now = DateTime.Now;

        var openLogs = await _context.Logs
            .Where(l => l.Status != SignedOffStatusId)
            .Include(l => l.StatusNavigation)
            .ToListAsync();

        var logsNeedingReminder = new List<Log>();
        var logsToReset = new List<Log>();

        foreach (var log in openLogs)
        {
            if (log.StatusNavigation == null)
                continue;

            var daysSinceUpdate = (now - log.DateLastUpdated).TotalDays;
            var remindDays = log.StatusNavigation.RemindDays;

            if (daysSinceUpdate > remindDays)
            {
                if (log.RemindedOnThisStatus != true)
                    logsNeedingReminder.Add(log);
            }
            else
            {
                if (log.RemindedOnThisStatus == true)
                    logsToReset.Add(log);
            }
        }

        // Load only the users we need in one query
        var userIds = logsNeedingReminder.Select(l => l.AssignedTo).Distinct().ToList();
        var users = userIds.Count > 0
            ? await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id)
            : [];

        // Send reminders
        foreach (var log in logsNeedingReminder)
        {
            if (!users.TryGetValue(log.AssignedTo, out var user) || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("ReminderCheckJob: AssignedTo user {UserId} not found or has no email for Log {LogId}",
                    log.AssignedTo, log.ID);
                continue;
            }

            try
            {
                var recipient = overrideEmail ?? user.Email;
                await _emailService.SendAsync(
                    recipient,
                    $"Reminder - Log {log.ID}",
                    BuildReminderEmail(log, user, _configuration));

                log.RemindedOnThisStatus = true;
                _logger.LogInformation("ReminderCheckJob: reminder sent for Log {LogId} to {Email}", log.ID, recipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReminderCheckJob: failed to send reminder for Log {LogId}", log.ID);
            }
        }

        // Reset flag on recently-updated logs
        foreach (var log in logsToReset)
            log.RemindedOnThisStatus = false;

        if (logsNeedingReminder.Count > 0 || logsToReset.Count > 0)
            await _context.SaveChangesAsync();

        _logger.LogInformation(
            "ReminderCheckJob: complete – {ReminderCount} reminder(s) sent, {ResetCount} log(s) reset",
            logsNeedingReminder.Count, logsToReset.Count);
    }

    private static string BuildReminderEmail(Log log, User user, IConfiguration configuration)
    {
        var daysSince = (int)(DateTime.Now - log.DateLastUpdated).TotalDays;
        var summary = log.LogText?.Length > 150
            ? log.LogText[..150] + "…"
            : log.LogText ?? string.Empty;

        var webAppBaseUrl = configuration["WebAppBaseUrl"] ?? "http://www.cclogs.net";
        var logUrl = $"{webAppBaseUrl}/viewlog/{log.ID}";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; font-size: 13px; color: #333; }");
        sb.AppendLine("h2 { color: #2c3e50; }");
        sb.AppendLine(".box { background:#f5f5f5; border-left:4px solid #2c3e50; padding:12px 16px; margin:16px 0; }");
        sb.AppendLine(".label { font-weight:bold; color:#555; display:inline-block; width:110px; }");
        sb.AppendLine(".btn { display:inline-block; padding:12px 24px; margin:16px 0; background:#2c3e50; color:#fff; text-decoration:none; border-radius:4px; font-weight:bold; }");
        sb.AppendLine(".btn:hover { background:#34495e; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h2>Reminder – Log {log.ID}</h2>");
        sb.AppendLine($"<p>Hi {System.Web.HttpUtility.HtmlEncode(user.Name)},</p>");
        sb.AppendLine($"<p>This is a friendly reminder that the following log has not been updated for " +
                      $"<strong>{daysSince} day{(daysSince == 1 ? "" : "s")}</strong> and may need your attention.</p>");
        sb.AppendLine("<div class=\"box\">");
        sb.AppendLine($"<div><span class=\"label\">Log ID:</span> {log.ID}</div>");
        sb.AppendLine($"<div><span class=\"label\">Customer:</span> {System.Web.HttpUtility.HtmlEncode(log.CustomerAccount)}</div>");
        sb.AppendLine($"<div><span class=\"label\">Date Logged:</span> {log.DateLogged:dd/MM/yyyy}</div>");
        sb.AppendLine($"<div><span class=\"label\">Last Updated:</span> {log.DateLastUpdated:dd/MM/yyyy}</div>");
        sb.AppendLine($"<div><span class=\"label\">Summary:</span> {System.Web.HttpUtility.HtmlEncode(summary)}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p><a href=\"{logUrl}\" class=\"btn\">View Log</a></p>");
        sb.AppendLine("<p>Please review and update this log at your earliest convenience.</p>");
        sb.AppendLine("<p>Kind regards,<br/>CCLog System</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}

