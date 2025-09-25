using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WarehouseManager.Core;

namespace WarehouseManager.Infrastructure.Services;


/// <summary>
/// This class is used to send emails to users using SendGrid 
/// </summary>
public class EmailSender : IEmailSender<ApplicationUser>
{
    private readonly ILogger<EmailSender> _logger;
    private readonly AuthMessageSenderOptions _options;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
    {
        _options = optionsAccessor.Value;
        _logger = logger;
    }
    
    /// <summary>
    /// Sends a confirmation link to the user to confirm their email address when registering
    /// </summary>
    /// <param name="user"> Name of the user  </param>
    /// <param name="email"> Email address of the user </param>
    /// <param name="confirmationLink"> Confirmation link to confirm the users email address </param>
    /// <returns></returns>
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirm your email";
        var message = $"Please confirm your account by <a href='{confirmationLink}'>clicking here.  Your trusted warehouse manager by Lukáš Hellesch :)))</a>.";
        return Execute(email, subject, message);
    }
    /// <summary>
    /// Sends a password reset link to the user to reset their password 
    /// </summary>
    /// <param name="user"> Name of the user </param>
    /// <param name="email"> Email address of the user </param>
    /// <param name="resetLink"> Reset link to reset the users password </param>
    /// <returns></returns>
    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Reset your password";
        var message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        return Execute(email, subject, message);
    }
    
    /// <summary>
    /// Sends a password reset code to the user to reset their password 
    /// </summary>
    /// <param name="user"> Name of the user  </param>
    /// <param name="email">Email address of the user </param>
    /// <param name="resetCode">Reset code to reset the users password </param>
    /// <returns></returns>
    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Reset your password";
        var message = $"Please reset your password using the following code: {resetCode}";
        return Execute(email, subject, message);
    }

    private async Task Execute(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_options.SendGridKey))
            throw new ArgumentException("The 'SendGridKey' is not configured in appsettings.json.");

        var client = new SendGridClient(_options.SendGridKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress("contact@appnestiq.com", "Warehouse Manager by Lukáš Hellesch :)))"),
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
}