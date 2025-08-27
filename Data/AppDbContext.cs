using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClassroomReservationSystem.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AcademicTerm> AcademicTerms { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
    
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}