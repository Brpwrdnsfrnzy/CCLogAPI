using CCLogAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CCLogAPI.Services;

public class EscalationCheckJob
{
    private const int SignedOffStatusId = 8;
    private const string EscalationToEmail = "cspiers@brisant-secure.com";
    private const string EscalationCcEmail = "rlockwood@brisant-secure.com";

    private readonly CCLogContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<EscalationCheckJob> _logger;
    private readonly IConfiguration _configuration;

    public EscalationCheckJob(CCLogContext context, IEmailService emailService, ILogger<EscalationCheckJob> logger, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task RunAsync(string overrideEmail = null)
    {
        _logger.LogInformation("EscalationCheckJob: starting{Override}",
            overrideEmail != null ? $" (test mode – emails redirected to {overrideEmail})" : string.Empty);

        var now = DateTime.Now;

        var openLogs = await _context.Logs
            .Where(l => l.Status != SignedOffStatusId)
            .Include(l => l.StatusNavigation)
            .ToListAsync();

        var logsNeedingEscalation = new List<Log>();
        var logsToReset = new List<Log>();

        foreach (var log in openLogs)
        {
            if (log.StatusNavigation == null)
                continue;

            var daysSinceUpdate = (now - log.DateLastUpdated).TotalDays;
            var escalateDays = log.StatusNavigation.EscalateDays;

            if (daysSinceUpdate > escalateDays)
            {
                if (log.EscalatedOnThisStatus != true)
                    logsNeedingEscalation.Add(log);
            }
            else
            {
                if (log.EscalatedOnThisStatus == true)
                    logsToReset.Add(log);
            }
        }

        // Load assigned users for logs needing escalation in one query
        var userIds = logsNeedingEscalation.Select(l => l.AssignedTo).Distinct().ToList();
        var users = userIds.Count > 0
            ? await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id)
            : [];

        // Send escalation emails
        foreach (var log in logsNeedingEscalation)
        {
            users.TryGetValue(log.AssignedTo, out var assignedUser);

            try
            {
                var toRecipient = overrideEmail ?? EscalationToEmail;
                var ccRecipient = overrideEmail != null ? null : EscalationCcEmail;

                await _emailService.SendAsync(
                    toRecipient,
                    $"Escalation Required - Log {log.ID}",
                    BuildEscalationEmail(log, assignedUser, _configuration),
                    ccRecipient);

                log.EscalatedOnThisStatus = true;
                _logger.LogInformation("EscalationCheckJob: escalation sent for Log {LogId} to {Email}", log.ID, toRecipient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EscalationCheckJob: failed to send escalation for Log {LogId}", log.ID);
            }
        }

        // Reset flag on recently-updated logs
        foreach (var log in logsToReset)
            log.EscalatedOnThisStatus = false;

        if (logsNeedingEscalation.Count > 0 || logsToReset.Count > 0)
            await _context.SaveChangesAsync();

        _logger.LogInformation(
            "EscalationCheckJob: complete – {EscalationCount} escalation(s) sent, {ResetCount} log(s) reset",
            logsNeedingEscalation.Count, logsToReset.Count);
    }

    private static string BuildEscalationEmail(Log log, User assignedUser, IConfiguration configuration)
    {
        var daysSince = (int)(DateTime.Now - log.DateLastUpdated).TotalDays;
        var summary = log.LogText?.Length > 150
            ? log.LogText[..150] + "…"
            : log.LogText ?? string.Empty;
        var assignedName = assignedUser?.Name ?? "Unknown";

        var webAppBaseUrl = configuration["WebAppBaseUrl"] ?? "http://www.cclogs.net";
        var logUrl = $"{webAppBaseUrl}/viewlog/{log.ID}";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; font-size: 13px; color: #333; }");
        sb.AppendLine("h2 { color: #c0392b; }");
        sb.AppendLine(".box { background:#fff5f5; border-left:4px solid #c0392b; padding:12px 16px; margin:16px 0; }");
        sb.AppendLine(".label { font-weight:bold; color:#555; display:inline-block; width:110px; }");
        sb.AppendLine(".btn { display:inline-block; padding:12px 24px; margin:16px 0; background:#c0392b; color:#fff; text-decoration:none; border-radius:4px; font-weight:bold; }");
        sb.AppendLine(".btn:hover { background:#e74c3c; }");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h2>Escalation Required – Log {log.ID}</h2>");
        sb.AppendLine("<p>The following log has not been updated within the required escalation period and requires your attention.</p>");
        sb.AppendLine("<div class=\"box\">");
        sb.AppendLine($"<div><span class=\"label\">Log ID:</span> {log.ID}</div>");
        sb.AppendLine($"<div><span class=\"label\">Customer:</span> {System.Web.HttpUtility.HtmlEncode(log.CustomerAccount)}</div>");
        sb.AppendLine($"<div><span class=\"label\">Assigned To:</span> {System.Web.HttpUtility.HtmlEncode(assignedName)}</div>");
        sb.AppendLine($"<div><span class=\"label\">Date Logged:</span> {log.DateLogged:dd/MM/yyyy}</div>");
        sb.AppendLine($"<div><span class=\"label\">Last Updated:</span> {log.DateLastUpdated:dd/MM/yyyy} ({daysSince} day{(daysSince == 1 ? "" : "s")} ago)</div>");
        sb.AppendLine($"<div><span class=\"label\">Summary:</span> {System.Web.HttpUtility.HtmlEncode(summary)}</div>");
        sb.AppendLine("</div>");
        sb.AppendLine($"<p><a href=\"{logUrl}\" class=\"btn\">View Log</a></p>");
        sb.AppendLine("<p>Please review this log and take the necessary action.</p>");
        sb.AppendLine("<p>Kind regards,<br/>CCLog System</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}

