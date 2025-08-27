using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomReservationSystem.Data
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string InstructorId { get; set; } =null!;

        [Required]
        public int ClassroomId { get; set; }

        [Required]
        [Display(Name = "Dönem Başlangıç")]
        public DateTime TermStartDate { get; set; }

        [Required]
        [Display(Name = "Dönem Bitiş")]
        public DateTime TermEndDate { get; set; }

        [Required]
        [Display(Name = "Haftanın Günü")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan EndTime { get; set; }

        // Yeni eklenen etkinlik/course bilgisi
        [Required]
        [StringLength(100)]
        [Display(Name = "Etkinlik/Ders")]
        public string Activity { get; set; } = string.Empty; // Örneğin: "Ceng241"

        [Required]
        [Display(Name = "Durum")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("ClassroomId")]
        public Classroom Classroom { get; set; } = null!;

        [Display(Name = "İlişkili Rezervasyon")]
        public int? RelatedReservationId { get; set; }
        

        public bool HasConflict(AppDbContext context, int? excludeId = null)
        {
            var query = context.Reservations
                .Where(r =>
                    r.ClassroomId == ClassroomId &&
                    r.DayOfWeek == DayOfWeek &&
                    r.TermStartDate <= TermEndDate &&
                    r.TermEndDate >= TermStartDate &&
                    r.StartTime < EndTime &&
                    r.EndTime > StartTime &&
                    r.Status != "Rejected" &&
                    r.Status != "CancellationRequested" && 
                    r.Status != "Cancelled" &&
                    r.Id != Id);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return query.Any();
        }

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        
    }


}