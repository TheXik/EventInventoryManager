using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WarehouseManager.Core;

namespace WarehouseManager.Infrastructure.Services;

public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<EmailSender> _logger;
    private readonly AuthMessageSenderOptions _options;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
    {
        _options = optionsAccessor.Value;
        _logger = logger;
    }

    private async Task Execute(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_options.SendGridKey))
        {
            throw new ArgumentException("The 'SendGridKey' is not configured in appsettings.json.");
        }

        var client = new SendGridClient(_options.SendGridKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("contact@appnestiq.com", "Warehouse Manager"),
            Subject = subject,
            HtmlContent = htmlMessage
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking to prevent SendGrid from wrapping links
        msg.SetClickTracking(false, false);

        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email to {ToEmail} queued successfully!", toEmail);
        }
        else
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("Failed to send email to {ToEmail}. Status Code: {StatusCode}, Response: {ResponseBody}", 
                toEmail, response.StatusCode, responseBody);
        }
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirm your email";
        var message = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
        return Execute(email, subject, message);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Reset your password";
        var message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        return Execute(email, subject, message);
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Reset your password";
        var message = $"Please reset your password using the following code: {resetCode}";
        return Execute(email, subject, message);
    }
}