using System.ComponentModel.DataAnnotations;

namespace cansaraciye_ecommerce.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
