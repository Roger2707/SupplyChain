using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Orders
{
    public class OrderCreateDto
    {
        [Required]
        public int BasketId { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
