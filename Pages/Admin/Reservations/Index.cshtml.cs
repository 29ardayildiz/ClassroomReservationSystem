using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Services;
using Microsoft.AspNetCore.Identity;

namespace ClassroomReservationSystem.Pages.Admin.Reservations
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        public AppDbContext Context => _context;

        private readonly HolidayService _holidayService;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        private const int PageSize = 10;
        public Dictionary<int, bool> HolidayConflicts { get; set; } = new();
        private Dictionary<int, List<DateTime>> _holidayConflictDates = new();

        public IndexModel(
            AppDbContext context,
            HolidayService holidayService,
            IEmailSender emailSender,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _holidayService = holidayService;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IList<Reservation> Reservations { get; set; }

       public async Task OnGetAsync(int? pageNumber)
{
    if (pageNumber.HasValue && pageNumber.Value > 0)
        PageNumber = pageNumber.Value;

    var query = Context.Reservations
    .OrderBy(r => r.Status == "Pending" ? 0 :
                      r.Status == "ModificationRequested" ? 1 :
                      r.Status == "CancellationRequested" ? 2 :
                      r.Status == "Approved" ? 3 :
                      r.Status == "Rejected" ? 4 : 5).ThenBy(r => r.TermStartDate)
        .Include(r => r.Classroom)
        .Where(r => r.Status != "Cancelled")
        .AsQueryable();

    var count = await query.CountAsync();
    TotalPages = (int)Math.Ceiling(count / (double)PageSize);

    Reservations = await query
        .Skip((PageNumber - 1) * PageSize)
        .Take(PageSize)
        .ToListAsync();

    HolidayConflicts = new Dictionary<int, bool>();
    _holidayConflictDates = new Dictionary<int, List<DateTime>>();

    foreach (var reservation in Reservations)
    {
        var holidays = await _holidayService.GetHolidaysInRange(
            reservation.TermStartDate,
            reservation.TermEndDate);

        var conflictingDates = holidays
            .Where(h => h.DayOfWeek == reservation.DayOfWeek)
            .ToList();

        HolidayConflicts[reservation.Id] = conflictingDates.Any();
        if (conflictingDates.Any())
        {
            _holidayConflictDates[reservation.Id] = conflictingDates;
        }
    }
}
        public List<DateTime> GetHolidayConflictDates(int reservationId)
        {
            return _holidayConflictDates.TryGetValue(reservationId, out var dates) 
                ? dates 
                : new List<DateTime>();
        }

        public async Task<IActionResult> OnPostApprove(int id)
        {
            var reservation = await Context.Reservations
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["Error"] = "Rezervasyon bulunamadı.";
                return RedirectToPage();
            }

            var instructor = await _userManager.FindByIdAsync(reservation.InstructorId);
            if (instructor == null)
            {
                TempData["Error"] = "Instructor bulunamadı.";
                return RedirectToPage();
            }

            // Resmi tatilleri kontrol et
            var holidays = await _holidayService.GetHolidaysInRange(
                reservation.TermStartDate, 
                reservation.TermEndDate);

            var conflictingDates = holidays
                .Where(h => h.DayOfWeek == reservation.DayOfWeek)
                .ToList();

            

            reservation.Status = "Approved";
            await Context.SaveChangesAsync();

            await _emailSender.SendReservationApprovalAsync(
                reservation,
                instructor.Email); // Use instructor from UserManager

            TempData["Success"] = "Rezervasyon başarıyla onaylandı.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReject(int id)
        {
            var reservation = await Context.Reservations
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                TempData["Error"] = "Rezervasyon bulunamadı.";
                return RedirectToPage();
            }

            var instructor = await _userManager.FindByIdAsync(reservation.InstructorId);
            if (instructor == null)
            {
                TempData["Error"] = "Instructor bulunamadı.";
                return RedirectToPage();
            }

            reservation.Status = "Rejected";
            await Context.SaveChangesAsync();

            await _emailSender.SendReservationRejectionAsync(
                reservation,
                instructor.Email, // Use instructor from UserManager
                "Rezervasyon talebiniz yönetici tarafından reddedildi.");

            TempData["Success"] = "Rezervasyon reddedildi ve instructor bilgilendirildi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveModificationAsync(int id)
        {
            var reservation = await Context.Reservations
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Status == "ModificationRequested" && r.Id == id);

            if (reservation != null)
            {
                var modifiedReservation = await Context.Reservations
                    .FirstOrDefaultAsync(r => r.Status == "ModificationPending" && 
                                            r.InstructorId == reservation.InstructorId);

                if (modifiedReservation != null)
                {
                    reservation.Status = "Cancelled";
                    modifiedReservation.Status = "Approved";
                    await Context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectModificationAsync(int id)
        {
            var reservation = await Context.Reservations
                .Include(r => r.Classroom)
                .FirstOrDefaultAsync(r => r.Status == "ModificationRequested" && r.Id == id);

            if (reservation != null)
            {
                var modifiedReservation = await Context.Reservations
                    .FirstOrDefaultAsync(r => r.Status == "ModificationPending" && 
                                            r.InstructorId == reservation.InstructorId);

                if (modifiedReservation != null)
                {
                    reservation.Status = "Approved";
                    modifiedReservation.Status = "Rejected";
                    await Context.SaveChangesAsync();
                }
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveCancellationAsync(int id)
        {
            var reservation = await Context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Status = "Cancelled";
                await Context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectCancellationAsync(int id)
        {
            var reservation = await Context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Status = "Approved";
                await Context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}