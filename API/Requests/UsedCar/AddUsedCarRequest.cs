using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace API.Requests.UsedCar
{
    public class AddUsedCarRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "At least one image is required")]
        public List<IFormFile> Images { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "Buyer phone number is required")]
        public string BuyerPhoneNumber { get; set; }
        
        public int? CreatedAt { get; set; }
    }
}
