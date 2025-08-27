using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

namespace ClassroomReservationSystem.Pages.Admin.Classrooms
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private const int PageSize = 10;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public class ClassroomViewModel
        {
            public Classroom Classroom { get; set; } = null!;
            public double AverageRating { get; set; }
        }

        public List<ClassroomViewModel> ClassroomViewModels { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; }
        


        public async Task OnGetAsync()
        {
            var query = _context.Classrooms.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                query = query.Where(c => c.Name.Contains(Filter));
            }

            var classroomList = await query
                .OrderBy(c => c.Name)
                .ToListAsync();

            var allViewModels = new List<ClassroomViewModel>();

            foreach (var classroom in classroomList)
            {
                var feedbacks = await _context.Feedbacks
                    .Include(f => f.Reservation)
                    .Where(f => f.Reservation.ClassroomId == classroom.Id)
                    .ToListAsync();

                var avgRating = feedbacks.Count > 0
                    ? feedbacks.Average(f => (double?)f.Rating) ?? 0
                    : 0;

                allViewModels.Add(new ClassroomViewModel
                {
                    Classroom = classroom,
                    AverageRating = avgRating
                });
            }

            // Sıralama: default name, ama "rating_desc" ya da "rating_asc" ise ona göre sıralanır
            if (SortOrder == "rating_desc")
            {
                allViewModels = allViewModels.OrderByDescending(vm => vm.AverageRating).ToList();
            }
            else if (SortOrder == "rating_asc")
            {
                allViewModels = allViewModels.OrderBy(vm => vm.AverageRating).ToList();
            }

            var totalCount = allViewModels.Count;
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            PageNumber = Math.Max(1, Math.Min(PageNumber, TotalPages));

            ClassroomViewModels = allViewModels
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnGetClassroomDetailsAsync(int id)
        {
            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
            {
                return NotFound();
            }

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Reservation)
                .ThenInclude(r => r.Classroom)
                .Where(f => f.Reservation.ClassroomId == id)
                .OrderByDescending(f => f.CreatedDate)
                .Select(f => new
                {
                    rating = f.Rating,
                    comment = f.Comment,
                    createdDate = f.CreatedDate
                })
                .ToListAsync();

            return new JsonResult(new
            {
                classroom = new { name = classroom.Name },
                feedbacks
            });
        }

        public async Task<IActionResult> OnPostCreateAsync(string name, int capacity)
        {
            if (string.IsNullOrWhiteSpace(name) || capacity < 1)
            {
                return BadRequest();
            }

            // Check for duplicate name
            var existingClassroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

            if (existingClassroom != null)
            {
                ModelState.AddModelError("", "Bu isimde bir sınıf zaten mevcut.");
                await OnGetAsync();
                return Page();
            }

            var classroom = new Classroom
            {
                Name = name,
                Capacity = capacity
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
