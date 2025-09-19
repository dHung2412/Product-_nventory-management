// Infrastructure/Services/EmailNotificationService.cs
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public interface INotificationService
    {
        Task SendLowStockAlertAsync(string productName, string warehouseName, int currentQuantity, string recipientEmail);
        Task SendOverStockAlertAsync(string productName, string warehouseName, int currentQuantity, string recipientEmail);
        Task SendWelcomeEmailAsync(string userName, string userEmail);
        Task SendPasswordResetEmailAsync(string userName, string userEmail, string resetToken);
        Task SendStockTransactionNotificationAsync(string transactionType, string productName, int quantity, string recipientEmail);
    }

    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "localhost";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@warehouse.com";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Warehouse Management System";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        }

        public async Task SendLowStockAlertAsync(string productName, string warehouseName, int currentQuantity, string recipientEmail)
        {
            var subject = $"Low Stock Alert - {productName}";
            var body = $@"
                <html>
                <body>
                    <h2>Low Stock Alert</h2>
                    <p>Dear Manager,</p>
                    <p>We would like to inform you that the stock level for the following product is running low:</p>
                    <ul>
                        <li><strong>Product:</strong> {productName}</li>
                        <li><strong>Warehouse:</strong> {warehouseName}</li>
                        <li><strong>Current Quantity:</strong> {currentQuantity}</li>
                    </ul>
                    <p>Please consider restocking this item to avoid stock-out situations.</p>
                    <p>Best regards,<br/>Warehouse Management System</p>
                </body>
                </html>";

            await SendEmailAsync(recipientEmail, subject, body, true);
        }

        public async Task SendOverStockAlertAsync(string productName, string warehouseName, int currentQuantity, string recipientEmail)
        {
            var subject = $"Over Stock Alert - {productName}";
            var body = $@"
                <html>
                <body>
                    <h2>Over Stock Alert</h2>
                    <p>Dear Manager,</p>
                    <p>We would like to inform you that the stock level for the following product is over the recommended threshold:</p>
                    <ul>
                        <li><strong>Product:</strong> {productName}</li>
                        <li><strong>Warehouse:</strong> {warehouseName}</li>
                        <li><strong>Current Quantity:</strong> {currentQuantity}</li>
                    </ul>
                    <p>Please consider reviewing your inventory management strategy for this item.</p>
                    <p>Best regards,<br/>Warehouse Management System</p>
                </body>
                </html>";

            await SendEmailAsync(recipientEmail, subject, body, true);
        }

        public async Task SendWelcomeEmailAsync(string userName, string userEmail)
        {
            var subject = "Welcome to Warehouse Management System";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Warehouse Management System</h2>
                    <p>Dear {userName},</p>
                    <p>Welcome to our Warehouse Management System! Your account has been successfully created.</p>
                    <p>You can now log in to the system using your credentials and start managing warehouse operations.</p>
                    <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
                    <p>Best regards,<br/>Warehouse Management Team</p>
                </body>
                </html>";

            await SendEmailAsync(userEmail, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string userName, string userEmail, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Dear {userName},</p>
                    <p>We received a request to reset your password for the Warehouse Management System.</p>
                    <p>Your password reset token is: <strong>{resetToken}</strong></p>
                    <p>Please use this token to reset your password. This token will expire in 24 hours.</p>
                    <p>If you did not request this password reset, please ignore this email.</p>
                    <p>Best regards,<br/>Warehouse Management Team</p>
                </body>
                </html>";

            await SendEmailAsync(userEmail, subject, body, true);
        }

        public async Task SendStockTransactionNotificationAsync(string transactionType, string productName, int quantity, string recipientEmail)
        {
            var subject = $"Stock Transaction Notification - {transactionType}";
            var body = $@"
                <html>
                <body>
                    <h2>Stock Transaction Notification</h2>
                    <p>Dear Manager,</p>
                    <p>A stock transaction has been completed:</p>
                    <ul>
                        <li><strong>Transaction Type:</strong> {transactionType}</li>
                        <li><strong>Product:</strong> {productName}</li>
                        <li><strong>Quantity:</strong> {quantity}</li>
                        <li><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                    </ul>
                    <p>Please review the transaction details in the system.</p>
                    <p>Best regards,<br/>Warehouse Management System</p>
                </body>
                </html>";

            await SendEmailAsync(recipientEmail, subject, body, true);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = _enableSsl;

                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await smtpClient.SendMailAsync(message);
                
                _logger.LogInformation("Email sent successfully to {ToEmail} with subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} with subject: {Subject}", toEmail, subject);
                throw;
            }
        }
    }
}