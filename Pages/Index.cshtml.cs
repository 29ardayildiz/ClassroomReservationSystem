using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClassroomReservationSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {   
            if (User.Identity.IsAuthenticated)
            {   
                if (User.IsInRole("Admin"))
                {
                    return RedirectToPage("/Admin/Dashboard");
                }
                else if (User.IsInRole("Instructor"))
                {
                    return RedirectToPage("/Instructor/Dashboard");
                }
            }
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }
    }
}
