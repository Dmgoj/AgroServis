using AgroServis.DAL.Entities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AgroServis.Services
{
    public class EmailService : IEmailService, IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _logger.LogInformation("EmailService instance created");
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Inside SendEmailAsync");
            var client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
            var from = new EmailAddress(_configuration["SendGrid:FromEmail"], _configuration["SendGrid:FromName"]);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, StripHtml(htmlMessage), htmlMessage);
            await client.SendEmailAsync(msg);
        }

        public async Task SendAdminApprovalNotificationAsync(PendingRegistration registration)
        {
            var adminEmail = _configuration["AdminEmail"];
            var baseUrl = _configuration["App:PublicBaseUrl"]!.TrimEnd('/');
            var approveLink = $"{baseUrl}/Registration/Approve?token={registration.ApprovalToken}";
            var rejectLink = $"{baseUrl}/Registration/Reject?token={registration.ApprovalToken}";

            var subject = $"New Registration Request - {registration.FirstName} {registration.LastName}";
            var body = $@"
            <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #007bff;'>New Worker Registration Request</h2>

                        <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Name:</strong> {registration.FirstName} {registration.LastName}</p>
                            <p><strong>Email:</strong> {registration.Email}</p>
                            <p><strong>Phone:</strong> {registration.PhoneNumber ?? "N/A"}</p>
                            <p><strong>Position:</strong> {registration.Position ?? "N/A"}</p>
                            <p><strong>Requested:</strong> {registration.RequestedAt:yyyy-MM-dd HH:mm}</p>
                        </div>

                        <h3 style='margin-top: 30px;'>Quick Actions:</h3>

                        <table width='100%' cellpadding='10' cellspacing='0'>
                            <tr>
                                <td width='50%' style='text-align: center;'>
                                    <a href='{approveLink}' style='display: inline-block; padding: 12px 30px; background-color: #28a745; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                        ✓ Approve
                                    </a>
                                </td>
                                <td width='50%' style='text-align: center;'>
                                    <a href='{rejectLink}' style='display: inline-block; padding: 12px 30px; background-color: #dc3545; color: #ffffff; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                        ✗ Reject
                                    </a>
                                </td>
                            </tr>
                        </table>
                    </div>
                </body>
                </html>
            ";

            await SendAdminEmailAsync(adminEmail, subject, body);
        }

        public async Task SendApprovalConfirmationAsync(string email, string firstName)
        {
            var subject = "Your Registration Has Been Approved";
            var body = $@"
            <h2>Welcome, {firstName}!</h2>
            <p>Your worker registration has been approved.</p>
            <p>You can now log in at: https://yourdomain.com/login</p>
        ";

            await SendAdminEmailAsync(email, subject, body);
        }

        public async Task SendRejectionNotificationAsync(string email, string firstName)
        {
            var subject = "Registration Request Update";
            var body = $@"
            <h2>Hello {firstName},</h2>
            <p>Unfortunately, your registration request could not be approved at this time.</p>
            <p>Please contact the administrator for more information.</p>
        ";

            await SendAdminEmailAsync(email, subject, body);
        }

        private async Task SendAdminEmailAsync(string to, string subject, string body)
        {
            try
            {
                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);

                var fromEmail = _configuration["SendGrid:FromEmail"];
                var fromName = _configuration["SendGrid:FromName"];
                var from = new EmailAddress(fromEmail, fromName);
                var toEmail = new EmailAddress(to);
                var plainText = StripHtml(body);

                var msg = MailHelper.CreateSingleEmail(
                    from,
                    toEmail,
                    subject,
                    plainTextContent: plainText,
                    htmlContent: body
                );

                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
            response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Email sent successfully to {To} with status {StatusCode}",
                        to, response.StatusCode);
                }
                else
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError("SendGrid returned {StatusCode}: {Response}",
                        response.StatusCode, responseBody);
                    throw new Exception($"SendGrid failed with status {response.StatusCode}: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        private string StripHtml(string html)
        {
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty)
                .Replace("&nbsp;", " ")
                .Trim();
        }
    }
}