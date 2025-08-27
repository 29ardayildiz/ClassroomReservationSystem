using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Net.Mail;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Services
{
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");
                var client = new SmtpClient(smtpSettings["SmtpServer"])
                {
                    Port = int.Parse(smtpSettings["SmtpPort"]),
                    Credentials = new NetworkCredential(smtpSettings["SmtpUsername"], smtpSettings["SmtpPassword"]),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromAddress"]),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                throw;
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}";
        }

        public async Task SendReservationApprovalAsync(Reservation reservation, string instructorEmail)
        {
            var subject = "Reservation Approved";
            var body = $@"
        <h2>Your Reservation Has Been Approved</h2>
        <p>Your reservation request has been approved as follows:</p>
        <ul>
            <li><strong>Classroom:</strong> {reservation.Classroom.Name}</li>
            <li><strong>Day:</strong> {reservation.DayOfWeek}</li>
            <li><strong>Time:</strong> {FormatTime(reservation.StartTime)} - {FormatTime(reservation.EndTime)}</li>
            <li><strong>Term:</strong> {reservation.TermStartDate:dd/MM/yyyy} - {reservation.TermEndDate:dd/MM/yyyy}</li>
        </ul>";

            await SendEmailAsync(instructorEmail, subject, body);
        }

        public async Task SendReservationRejectionAsync(Reservation reservation, string instructorEmail, string reason)
        {
            var subject = "Reservation Rejected";
            var body = $@"
        <h2>Your Reservation Has Been Rejected</h2>
        <p>Your reservation request has been rejected with the following details:</p>
        <ul>
            <li><strong>Classroom:</strong> {reservation.Classroom.Name}</li>
            <li><strong>Day:</strong> {reservation.DayOfWeek}</li>
            <li><strong>Time:</strong> {FormatTime(reservation.StartTime)} - {FormatTime(reservation.EndTime)}</li>
            <li><strong>Term:</strong> {reservation.TermStartDate:dd/MM/yyyy} - {reservation.TermEndDate:dd/MM/yyyy}</li>
        </ul>
        <p><strong>Reason for Rejection:</strong> {reason}</p>";

            await SendEmailAsync(instructorEmail, subject, body);
        }

        public async Task SendHolidayConflictAsync(Reservation reservation, string email, string conflictingDates)
        {
            var message = $@"<h2>Reservation-Holiday Conflict</h2>
           <p>Your reservation request conflicts with the following official holidays:</p>
           <ul>
               <li><strong>Classroom:</strong> {reservation.ClassroomId}</li>
               <li><strong>Day:</strong> {reservation.DayOfWeek}</li>
               <li><strong>Time:</strong> {reservation.StartTime} - {reservation.EndTime}</li>
               <li><strong>Conflicting Dates:</strong> {conflictingDates}</li>
           </ul>
           <p>Please review your reservation.</p>";

            await SendEmailAsync(email, "Reservation-Holiday Conflict", message);
        }

        public async Task SendContactSubmissionToAdmin(string name, string email, string subject, string message)
        {
            var adminEmail = _configuration["AdminContactEmail"];
            var adminSubject = "[Contact Form] New Inquiry";
            var adminBody = $@"
    <h3>New Contact Submission</h3>
    <p><strong>Name:</strong> {name}</p>
    <p><strong>Email:</strong> {email}</p>
    <p><strong>Subject:</strong> {subject}</p>
    <p><strong>Message:</strong> {message}</p>";

            await SendEmailAsync(adminEmail, adminSubject, adminBody);
        }

    }
}