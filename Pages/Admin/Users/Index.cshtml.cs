using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ClassroomReservationSystem.Pages.Admin.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManager<IdentityUser> UserManager => _userManager;

        public List<IdentityUser> Users { get; set; }
        public List<IdentityRole> Roles { get; set; }

        public string StatusMessage { get; set; }
        public bool IsError { get; set; }

        public IndexModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task OnGetAsync()
        {
            Users = _userManager.Users.ToList();
            Roles = _roleManager.Roles.ToList();
        }

        public async Task<IActionResult> OnPostAddToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(user, roleName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveFromRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.RemoveFromRoleAsync(user, roleName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateUserAsync(string email, string password, string confirmPassword, string initialRole)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                StatusMessage = "Tüm alanları doldurunuz.";
                IsError = true;

                Users = _userManager.Users.ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            if (password != confirmPassword)
            {
                StatusMessage = "Şifreler eşleşmiyor.";
                IsError = true;

                Users = _userManager.Users.ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            var newUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(initialRole))
                {
                    await _userManager.AddToRoleAsync(newUser, initialRole);
                }

                StatusMessage = $"{email} kullanıcısı başarıyla oluşturuldu.";
                IsError = false;
            }
            else
            {
                StatusMessage = "Kullanıcı oluşturulamadı: " + string.Join(", ", result.Errors.Select(e => e.Description));
                IsError = true;
            }

            Users = _userManager.Users.ToList();
            Roles = _roleManager.Roles.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string userId, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                StatusMessage = "Şifre boş olamaz.";
                IsError = true;

                Users = _userManager.Users.ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            if (newPassword != confirmPassword)
            {
                StatusMessage = "Şifreler eşleşmiyor.";
                IsError = true;

                Users = _userManager.Users.ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Kullanıcı bulunamadı.";
                IsError = true;

                Users = _userManager.Users.ToList();
                Roles = _roleManager.Roles.ToList();
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                StatusMessage = $"{user.UserName} kullanıcısının şifresi başarıyla değiştirildi.";
                IsError = false;
            }
            else
            {
                StatusMessage = "Şifre değiştirilemedi: " + string.Join(", ", result.Errors.Select(e => e.Description));
                IsError = true;
            }

            Users = _userManager.Users.ToList();
            Roles = _roleManager.Roles.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                StatusMessage = "Kullanıcı bulunamadı.";
                IsError = true;
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    StatusMessage = $"{user.UserName} kullanıcısı başarıyla silindi.";
                    IsError = false;
                }
                else
                {
                    StatusMessage = "Kullanıcı silinemedi: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    IsError = true;
                }
            }
            Users = _userManager.Users.ToList();
            Roles = _roleManager.Roles.ToList();
            return Page();
        }

    }
}