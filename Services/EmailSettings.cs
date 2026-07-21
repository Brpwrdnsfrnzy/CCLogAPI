namespace CCLogAPI.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.office365.com";
    public int SmtpPort { get; set; } = 587;
    public string FromAddress { get; set; } = string.Empty;
    public string FromPassword { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "CCLog System";
}
