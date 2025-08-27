#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Services;

namespace ClassroomReservationSystem.Areas.Identity.Pages.Account
{
    public class ContactModel : PageModel
    {
        private readonly IEmailSender _emailSender;

        public ContactModel(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Subject { get; set; }

            [Required]
            public string Message { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _emailSender.SendContactSubmissionToAdmin(
                Input.Name, 
                Input.Email, 
                Input.Subject, 
                Input.Message
            );

            TempData["SuccessMessage"] = "Your message has been sent successfully!";
            return RedirectToPage();
        }
    }
}