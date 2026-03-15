namespace SPage.Infrastructure.Configuration;

public sealed class EmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "SPage";

    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

