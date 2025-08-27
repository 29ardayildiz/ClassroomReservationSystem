using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Pages.Admin.AcademicTerms
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<AcademicTerm> AcademicTerms { get; set; } = new List<AcademicTerm>();

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public AcademicTerm NewAcademicTerm { get; set; }

        [BindProperty]
        public AcademicTerm EditAcademicTerm { get; set; }

        public async Task OnGetAsync(string searchTerm = null)
        {
            var query = _context.AcademicTerms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => x.Name.Contains(searchTerm));
            }

            AcademicTerms = await query
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (NewAcademicTerm == null ||
                string.IsNullOrEmpty(NewAcademicTerm.Name) ||
                NewAcademicTerm.StartDate == default ||
                NewAcademicTerm.EndDate == default)
            {
                StatusMessage = "Lütfen tüm alanları doldurunuz.";
                await OnGetAsync();
                return Page();
            }

            if (NewAcademicTerm.EndDate <= NewAcademicTerm.StartDate)
            {
                ModelState.AddModelError("NewAcademicTerm.EndDate", "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
                await OnGetAsync();
                return Page();
            }

            try
            {
                _context.AcademicTerms.Add(NewAcademicTerm);
                await _context.SaveChangesAsync();
                StatusMessage = "Yeni akademik dönem başarıyla oluşturuldu.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kaydetme işlemi sırasında bir hata oluştu.");
                await OnGetAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            var academicTerm = await _context.AcademicTerms
                .FirstOrDefaultAsync(x => x.Id == EditAcademicTerm.Id);

            if (academicTerm == null)
            {
                return NotFound();
            }

            if (EditAcademicTerm.IsActive)
            {
                var existingActiveTerm = await _context.AcademicTerms
                    .Where(x => x.IsActive && x.Id != EditAcademicTerm.Id)
                    .FirstOrDefaultAsync();

                if (existingActiveTerm != null)
                {
                    StatusMessage = "Zaten aktif bir akademik dönem var. Lütfen önce onu pasif hale getirin.";
                    await OnGetAsync();
                    return Page();
                }
            }

            academicTerm.Name = EditAcademicTerm.Name;
            academicTerm.StartDate = EditAcademicTerm.StartDate;
            academicTerm.EndDate = EditAcademicTerm.EndDate;
            academicTerm.IsActive = EditAcademicTerm.IsActive;

            _context.Entry(academicTerm).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            StatusMessage = "Akademik dönem başarıyla güncellendi.";
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var academicTerm = await _context.AcademicTerms.FindAsync(id);
            if (academicTerm == null)
            {
                return NotFound();
            }

            _context.AcademicTerms.Remove(academicTerm);
            await _context.SaveChangesAsync();

            StatusMessage = "Akademik dönem silindi.";
            return RedirectToPage();
        }
    }
}