using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SPage.Application.Common.Interfaces;
using SPage.Infrastructure.Configuration;

namespace SPage.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IEmailConfigService _configService;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailSettings> options,
        IEmailConfigService configService,
        ILogger<SmtpEmailService> logger)
    {
        _settings = options.Value;
        _configService = configService;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var config = await _configService.GetSendConfigAsync(cancellationToken);
        var host = config?.Host ?? _settings.Host;
        var port = config?.Port ?? _settings.Port;
        var fromEmail = config?.FromEmail ?? _settings.FromEmail;
        var fromName = config?.FromName ?? _settings.FromName ?? _settings.FromEmail;
        var enableSsl = config?.EnableSsl ?? _settings.EnableSsl;
        var userName = config?.UserName ?? _settings.UserName;
        var password = config?.Password ?? _settings.Password;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning("Email send config is not configured. Skip sending email to {Email}", toEmail);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName ?? fromEmail),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = string.IsNullOrWhiteSpace(userName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(userName, password ?? "")
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}

