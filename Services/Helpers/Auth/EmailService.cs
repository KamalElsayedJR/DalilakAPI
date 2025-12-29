using Core.DTOs.Auth;
using Core.Interfaces.Auth;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Services.Helpers.Auth
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
        }

        public async Task<bool> SendPasswordResetOtpAsync(string email, string otpCode)
        {
            try
            {
                var templateData = new PasswordResetOtpData
                {
                    UserName = email.Split('@')[0],
                    OtpCode = otpCode,
                    CompanyName = _configuration["AppSettings:CompanyName"] ?? "Dalilak",
                    ExpirationMinutes = 5
                };

                var htmlContent = EmailTemplateService.GetPasswordResetOtpTemplate(templateData);
                var plainTextContent = $"Password Reset Request\n\nYour password reset code is: {otpCode}\n\nThis code will expire in 5 minutes.";

                return await SendEmailAsync(email, "Reset Your Password - OTP Code", htmlContent, plainTextContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset OTP to {email}");
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationAsync(string email, string otpCode)
        {
            try
            {
                var templateData = new EmailVerificationData
                {
                    UserName = email.Split('@')[0],
                    OtpCode = otpCode,
                    CompanyName = _configuration["AppSettings:CompanyName"] ?? "CarFinder",
                    ExpirationMinutes = 10
                };

                var htmlContent = EmailTemplateService.GetEmailVerificationTemplate(templateData);
                var plainTextContent = $"Email Verification\n\nYour verification code is: {otpCode}\n\nThis code will expire in 10 minutes.";

                return await SendEmailAsync(email, "Verify Your Email Address", htmlContent, plainTextContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email verification to {email}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var templateData = new WelcomeEmailData
                {
                    UserName = firstName,
                    CompanyName = _configuration["AppSettings:CompanyName"] ?? "CarFinder",
                    LoginUrl = $"{_configuration["AppSettings:FrontendUrl"]}/login"
                };

                var htmlContent = EmailTemplateService.GetWelcomeEmailTemplate(templateData);
                var plainTextContent = $"Welcome to {templateData.CompanyName}!\n\nHello {firstName},\n\nWelcome to {templateData.CompanyName}! Your account has been successfully created.\n\nVisit {templateData.LoginUrl} to start exploring.";

                return await SendEmailAsync(email, $"Welcome to {templateData.CompanyName}!", htmlContent, plainTextContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null)
        {
            try
            {
                if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
                {
                    _logger.LogWarning("Email sending is disabled. SMTP settings not configured.");
                    _logger.LogInformation($"Email would be sent to {toEmail} with subject '{subject}'");
                    return true;
                }

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                email.To.Add(new MailboxAddress("", toEmail));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlContent,
                    TextBody = plainTextContent ?? ConvertHtmlToPlainText(htmlContent)
                };

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, _emailSettings.EnableSsl);
                
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername))
                {
                    await smtp.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                }
                
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail} with subject '{subject}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail} with subject '{subject}'");
                return false;
            }
        }

        private static string ConvertHtmlToPlainText(string html)
        {
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty)
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Trim();
        }
    }
}