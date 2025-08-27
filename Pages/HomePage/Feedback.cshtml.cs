
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using System.ComponentModel.DataAnnotations;

namespace ClassroomReservationSystem.Pages.Instructor
{
    public class FeedbackModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public FeedbackViewModel FeedbackVM { get; set; } = new();

        public List<Reservation> UserReservations { get; set; } = new();

        public FeedbackModel(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            
            UserReservations = await _context.Reservations
                .Include(r => r.Classroom)
                .Where(r => 
                    r.Status == "Approved")
                .OrderByDescending(r => r.TermEndDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            var isValidReservation = await _context.Reservations
                .AnyAsync(r => r.Id == FeedbackVM.ReservationId );

            if (!isValidReservation)
            {
                ModelState.AddModelError("", "Geçersiz etkinlik seçimi!");
                await OnGetAsync();
                return Page();
            }

            var newFeedback = new Feedback
            {
                ReservationId = FeedbackVM.ReservationId,
                Rating = FeedbackVM.Rating,
                Comment = FeedbackVM.Comment,
                CreatedDate = DateTime.UtcNow
            };

            try
            {
                _context.Feedbacks.Add(newFeedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Feedback has been recorded!";
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
                await OnGetAsync();
                return Page();
            }
        }
    }

    public class FeedbackViewModel
    {
        [Required(ErrorMessage = "Etkinlik seçimi zorunludur.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Puan zorunludur.")]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Yorum zorunludur.")]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir.")]
        public string Comment { get; set; } = null!;
    }
}