using CCLogAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CCLogAPI.Services;

public class DailyReportJob
{
    private readonly CCLogContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailyReportJob> _logger;
    private readonly IConfiguration _configuration;

    public DailyReportJob(CCLogContext context, IEmailService emailService, ILogger<DailyReportJob> logger, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendDailyReportAsync()
    {
        _logger.LogInformation("DailyReportJob: starting");

        var openLogs = await _context.Logs
            .Where(l => l.SignOffDate == null)
            .OrderBy(l => l.DateLogged)
            .ToListAsync();

        var subject = $"CCLog Daily Report – {DateTime.Today:dd MMM yyyy} ({openLogs.Count} open log{(openLogs.Count == 1 ? "" : "s")})";
        var body = BuildHtmlBody(openLogs);

        // Get recipient and CC addresses from configuration
        var recipient = _configuration["DailyReport:Recipient"] ?? "richard.lockwood@allegion.com";
        var ccAddress = _configuration["DailyReport:CcAddress"];

        await _emailService.SendAsync(recipient, subject, body, ccAddress);

        _logger.LogInformation("DailyReportJob: sent report with {Count} open logs to {Recipient}{Cc}", 
            openLogs.Count, recipient, string.IsNullOrWhiteSpace(ccAddress) ? "" : $" (cc: {ccAddress})");
    }

    private static string BuildHtmlBody(List<Log> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; font-size: 13px; color: #333; }");
        sb.AppendLine("h2 { color: #2c3e50; }");
        sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("th { background-color: #2c3e50; color: white; padding: 8px 12px; text-align: left; }");
        sb.AppendLine("td { padding: 7px 12px; border-bottom: 1px solid #ddd; }");
        sb.AppendLine("tr:nth-child(even) td { background-color: #f5f5f5; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h2>CCLog Daily Report – {DateTime.Today:dd MMMM yyyy}</h2>");

        if (logs.Count == 0)
        {
            sb.AppendLine("<p>There are no open logs at this time.</p>");
        }
        else
        {
            sb.AppendLine($"<p>{logs.Count} open log{(logs.Count == 1 ? "" : "s")} as of {DateTime.Now:HH:mm}:</p>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>ID</th><th>Date Logged</th><th>Customer Account</th><th>Summary</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var log in logs)
            {
                var summary = log.LogText?.Length > 100
                    ? log.LogText[..100] + "…"
                    : log.LogText ?? string.Empty;

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{log.ID}</td>");
                sb.AppendLine($"<td>{log.DateLogged:dd/MM/yyyy}</td>");
                sb.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(log.CustomerAccount)}</td>");
                sb.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(summary)}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
