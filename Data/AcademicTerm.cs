using System.ComponentModel.DataAnnotations;

namespace ClassroomReservationSystem.Data
{
    public class AcademicTerm
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Dönem adı zorunludur.")]
        [Display(Name = "Dönem Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = false; 

        
    }
}