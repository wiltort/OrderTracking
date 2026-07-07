using System.ComponentModel.DataAnnotations;

namespace OrderTracking.Application.DTOs
{
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("Created|Sent|Delivered|Cancelled", 
            ErrorMessage = "Status must be one of: Created, Sent, Delivered, Cancelled")]
        public required string Status { get; set; }
    }
}