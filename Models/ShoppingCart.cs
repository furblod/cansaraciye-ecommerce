using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cansaraciye_ecommerce.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }

        public string? UserId { get; set; } // Kullanıcının kimliği (Anonim kullanıcılar için boş olabilir)
    }
}
