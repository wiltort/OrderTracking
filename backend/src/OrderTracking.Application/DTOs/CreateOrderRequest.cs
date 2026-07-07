using System.ComponentModel.DataAnnotations;

namespace OrderTracking.Application.DTOs
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 500 characters")]
        public required string Description { get; set; }
    }
}