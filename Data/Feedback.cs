using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ClassroomReservationSystem.Data
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ReservationId { get; set; } 
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Comment { get; set; } = null!;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; } = null!; 
    }
}