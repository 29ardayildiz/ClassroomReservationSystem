using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using System.ComponentModel.DataAnnotations;
using ClassroomReservationSystem.Services;

namespace ClassroomReservationSystem.Pages.Instructor
{
    [Authorize(Roles = "Instructor")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly HolidayService _holidayService;
        private readonly IEmailSender _emailSender;

        public DashboardModel(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            HolidayService holidayService,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _holidayService = holidayService;
            _emailSender = emailSender;
        }

        public List<Reservation> MyReservations { get; set; }
        public List<Classroom> AvailableClassrooms { get; set; }
        [TempData]
        public string Message { get; set; }

        [BindProperty]
        public ReservationInputModel Input { get; set; }

        public class ReservationInputModel
        {
            [Required(ErrorMessage = "Sınıf seçimi zorunludur")]
            public int ClassroomId { get; set; }

            [Required(ErrorMessage = "Gün seçimi zorunludur")]
            public DayOfWeek DayOfWeek { get; set; }

            [Required(ErrorMessage = "Başlangıç saati zorunludur")]
            public TimeSpan StartTime { get; set; }

            [Required(ErrorMessage = "Bitiş saati zorunludur")]
            public TimeSpan EndTime { get; set; }

            [Required(ErrorMessage = "Etkinlik adı zorunludur")]
            [StringLength(100)]
            public string Activity { get; set; }
        }

        public PaginatedList<Reservation> PaginatedReservations { get; set; }
        public int PageSize { get; set; } = 10;
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Reservations
                .Include(r => r.Classroom)
                .Where(r => r.InstructorId == userId &&
                          r.Status != "ModificationPending" &&
                          r.Status != "CancellationRequested" &&
                          r.Status != "Cancelled")
                .OrderByDescending(r => r.TermStartDate);

            PaginatedReservations = await PaginatedList<Reservation>.CreateAsync(
                query, CurrentPage, PageSize);

            MyReservations = PaginatedReservations.Items;
            AvailableClassrooms = await _context.Classrooms.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var term = await _context.AcademicTerms
                .FirstOrDefaultAsync(t => t.StartDate <= DateTime.Today && t.EndDate >= DateTime.Today);

            if (term == null)
            {
                Message = "Aktif bir akademik dönem bulunamadı!";
                return RedirectToPage();
            }

            var holidays = await _holidayService.GetHolidaysInRange(term.StartDate, term.EndDate);

            var conflictingDates = new List<DateTime>();
            var currentDate = term.StartDate;

            while (currentDate <= term.EndDate)
            {
                if (currentDate.DayOfWeek == Input.DayOfWeek && 
                    holidays.Any(h => h.Date == currentDate.Date)) 
                {
                    conflictingDates.Add(currentDate);
                }
                currentDate = currentDate.AddDays(1);
            }

            if (conflictingDates.Any())
            {
                var formattedDates = string.Join(", ",
                    conflictingDates.Select(d => d.ToString("dd/MM/yyyy")));

                var newReservation = new Reservation
                {
                    InstructorId = userId,
                    ClassroomId = Input.ClassroomId,
                    DayOfWeek = Input.DayOfWeek,
                    StartTime = Input.StartTime,
                    EndTime = Input.EndTime,
                    Activity = Input.Activity
                };

                await _emailSender.SendHolidayConflictAsync(
                    newReservation,
                    user.Email,
                    formattedDates);

                Message = $"Dikkat: Rezervasyon yapmak istediğiniz günlerden {formattedDates} tarihleri resmi tatile denk gelmektedir.";
                
            }

            var reservation = new Reservation
            {
                InstructorId = userId,
                ClassroomId = Input.ClassroomId,
                DayOfWeek = Input.DayOfWeek,
                StartTime = Input.StartTime,
                EndTime = Input.EndTime,
                TermStartDate = term.StartDate,
                TermEndDate = term.EndDate,
                Status = "Pending",
                Activity = Input.Activity,
                CreatedDate = DateTime.Now
            };

            if (reservation.HasConflict(_context, excludeId: null))
            {
                Message = "Bu zaman diliminde çakışan bir rezervasyon bulunmaktadır!";
                return RedirectToPage();
            }

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            Message = "Rezervasyon başarıyla oluşturuldu!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRequestModificationAsync(
            int id,
            DayOfWeek newDayOfWeek,
            TimeSpan newStartTime,
            TimeSpan newEndTime)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.InstructorId == userId);

            if (reservation == null)
            {
                Message = "Rezervasyon bulunamadı!";
                return RedirectToPage();
            }

            var modifiedReservation = new Reservation
            {
                InstructorId = reservation.InstructorId,
                ClassroomId = reservation.ClassroomId,
                DayOfWeek = newDayOfWeek,
                StartTime = newStartTime,
                EndTime = newEndTime,
                TermStartDate = reservation.TermStartDate,
                TermEndDate = reservation.TermEndDate,
                Status = "ModificationPending",
                RelatedReservationId = reservation.Id,
                CreatedDate = DateTime.Now
            };

            if (modifiedReservation.HasConflict(_context, reservation.Id))
            {
                Message = "Bu zaman diliminde çakışan bir rezervasyon bulunmaktadır!";
                return RedirectToPage();
            }

            reservation.Status = "ModificationRequested";
            _context.Reservations.Add(modifiedReservation);
            await _context.SaveChangesAsync();

            Message = "Değişiklik isteğiniz başarıyla gönderildi!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRequestCancellationAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.InstructorId == userId);

            if (reservation == null)
            {
                Message = "Rezervasyon bulunamadı!";
                return RedirectToPage();
            }

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            Message = "Rezervasyon başarıyla iptal edildi!";
            return RedirectToPage();
        }
        public async Task<JsonResult> OnGetMyCalendarEvents()
        {
            var userId = _userManager.GetUserId(User);

            var reservations = await _context.Reservations
            .Include(r => r.Classroom)
            .Where(r =>
                r.Status == "Approved" ||
                (r.Status == "Pending" || r.Status == "ModificationPending") && r.InstructorId == userId)
            .ToListAsync();


            var holidays = await _holidayService.GetHolidaysInRange(
                       DateTime.Today.AddMonths(-1),
                       DateTime.Today.AddMonths(2));

            var reservationEvents = reservations.SelectMany(r =>
                GenerateWeeklyEvents(r, r.TermStartDate, r.TermEndDate, holidays))
                .ToList();

            var holidayEvents = holidays.Select(h => new
            {
                title = "Resmi Tatil",
                start = h.ToString("yyyy-MM-dd"),
                allDay = true,
                color = "#ff0000",
            });
            Console.WriteLine($"Holidays: {string.Join(", ", holidays.Select(h => h.ToString("yyyy-MM-dd")))}");
            var allEvents = reservationEvents.Concat(holidayEvents);

            return new JsonResult(allEvents);
        }

        private IEnumerable<object> GenerateWeeklyEvents(
            Reservation reservation,
            DateTime startDate,
            DateTime endDate,
            List<DateTime> holidays)
        {
            var events = new List<object>();

            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == reservation.DayOfWeek)
                {
                    var isHoliday = holidays.Contains(currentDate.Date);
                    var hasConflict = reservation.HasConflict(_context, reservation.Id);

                    events.Add(new
                    {
                        title = $"{reservation.Activity} - {reservation.Classroom.Name} " +
                               $"({reservation.StartTime:hh\\:mm}-{reservation.EndTime:hh\\:mm})",
                        start = currentDate.Date.Add(reservation.StartTime),
                        end = currentDate.Date.Add(reservation.EndTime),
                        color = GetEventColor(reservation.Status, hasConflict, isHoliday)
                    });
                }
                currentDate = currentDate.AddDays(1);
            }
            return events;
        }

        private string GetEventColor(string status, bool hasConflict, bool isHoliday)
        {
            if (isHoliday) return "#ff0000";
            if (hasConflict) return "#ff8c00";

            return status switch
            {
                "Approved" => "#28a745",
                "Pending" => "#ffc107",
                "ModificationPending" => "#17a2b8",
                _ => "#dc3545"
            };
        }
        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Approved" => "#28a745",
                "Pending" => "#ffc107",
                "ModificationPending" => "#17a2b8",
                _ => "#dc3545"
            };
        }
    }

    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageIndex { get; }
        public int TotalPages { get; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip(
                (pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}