using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities.UsedCar
{
    public class UsedCar
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [Required]
        public string Images { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string City { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string BuyerPhoneNumber { get; set; }
        
        public int CreatedAtYear { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
