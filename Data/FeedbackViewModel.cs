using System.ComponentModel.DataAnnotations;

namespace ClassroomReservationSystem.Data
{
    public class FeedbackViewModel
    {
        [Required(ErrorMessage = "Etkinlik seçimi zorunludur.")]
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Puan zorunludur.")]
        [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Yorum zorunludur.")]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir.")]
        public string Comment { get; set; } = null!;
    }
}
