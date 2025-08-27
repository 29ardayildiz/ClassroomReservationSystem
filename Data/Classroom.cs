using System.ComponentModel.DataAnnotations;

namespace ClassroomReservationSystem.Data
{
    public class Classroom
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}