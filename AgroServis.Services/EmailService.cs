using AgroServis.DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AgroServis.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAdminApprovalNotificationAsync(PendingRegistration registration)
        {
            var adminEmail = _configuration["AdminEmail"];
            var approvalLink = $"https://localhost:7120/Admin/ApproveRegistration/{registration.Id}";

            var subject = $"New Registration Request - {registration.FirstName} {registration.LastName}";
            var body = $@"
        <html>
        <body>
            <h2>New Worker Registration Request</h2>
            <p><strong>Name:</strong> {registration.FirstName} {registration.LastName}</p>
            <p><strong>Email:</strong> {registration.Email}</p>
            <p><strong>Phone:</strong> {registration.PhoneNumber ?? "N/A"}</p>
            <p><strong>Position:</strong> {registration.Position ?? "N/A"}</p>
            <p><strong>Requested:</strong> {registration.RequestedAt:yyyy-MM-dd HH:mm}</p>
            <p><a href='{approvalLink}'>Click here to Review and Approve/Reject</a></p>
        </body>
        </html>
    ";

            await SendEmailAsync(adminEmail, subject, body);
        }

        public async Task SendApprovalConfirmationAsync(string email, string firstName)
        {
            var subject = "Your Registration Has Been Approved";
            var body = $@"
            <h2>Welcome, {firstName}!</h2>
            <p>Your worker registration has been approved.</p>
            <p>You can now log in at: https://yourdomain.com/login</p>
        ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendRejectionNotificationAsync(string email, string firstName)
        {
            var subject = "Registration Request Update";
            var body = $@"
            <h2>Hello {firstName},</h2>
            <p>Unfortunately, your registration request could not be approved at this time.</p>
            <p>Please contact the administrator for more information.</p>
        ";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
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