using System.ComponentModel.DataAnnotations.Schema;

namespace cansaraciye_ecommerce.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public string? ImageUrl { get; set; } = "";

        // 🔁 İlişki: Hangi ürüne ait
        [ForeignKey("Product")]
        public int? ProductId { get; set; }

        public virtual Product? Product { get; set; }
    }
}
