using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ClassroomReservationSystem.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;

        public DashboardModel(AppDbContext context)
        {
            _context = context;
        }

        public List<AcademicTerm> ActiveAcademicTerms { get; set; }

        
        public int TotalClassrooms { get; set; }
        public int TotalReservations { get; set; }
        public int ApprovedReservationsCount { get; set; }
        public int PendingReservationsCount { get; set; }
        public int RejectedReservationsCount { get; set; }
        public Dictionary<string, int> WeeklyReservations { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MonthlyReservations { get; set; } = new Dictionary<string, int>();

        public async Task OnGetAsync()
        {
            ActiveAcademicTerms = await _context.AcademicTerms
                .Where(t => t.IsActive && t.EndDate >= DateTime.Today)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            TotalClassrooms = await _context.Classrooms.CountAsync();
            TotalReservations = await _context.Reservations.CountAsync();

            ApprovedReservationsCount = await _context.Reservations
                .CountAsync(r => r.Status == "Approved");
            PendingReservationsCount = await _context.Reservations
                .CountAsync(r => r.Status == "Pending");
            RejectedReservationsCount = await _context.Reservations
                .CountAsync(r => r.Status == "Rejected");

            var startDateWeekly = DateTime.Today.AddDays(-6);
            var weeklyData = await _context.Reservations
                .Where(r => r.CreatedDate >= startDateWeekly)
                .GroupBy(r => r.CreatedDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.Today.AddDays(-i);
                var formattedDate = date.ToString("dd/MM");
                var count = weeklyData.FirstOrDefault(d => d.Date == date.Date)?.Count ?? 0;
                WeeklyReservations[formattedDate] = count;
            }
        }

        public double GetApprovedPercentage() 
            => TotalReservations == 0 ? 0 : Math.Round((ApprovedReservationsCount / (double)TotalReservations) * 100, 1);

        public double GetPendingPercentage() 
            => TotalReservations == 0 ? 0 : Math.Round((PendingReservationsCount / (double)TotalReservations) * 100, 1);

        public double GetRejectedPercentage() 
            => TotalReservations == 0 ? 0 : Math.Round((RejectedReservationsCount / (double)TotalReservations) * 100, 1);
    }
}