using System.Threading.Tasks;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendReservationApprovalAsync(Reservation reservation, string instructorEmail);
        Task SendReservationRejectionAsync(Reservation reservation, string instructorEmail, string reason);
        Task SendHolidayConflictAsync(Reservation reservation, string email, string conflictingDates);
        Task SendContactSubmissionToAdmin(string name, string email, string subject, string message);
        
    }
}