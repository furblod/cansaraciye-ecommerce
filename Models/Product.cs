using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cansaraciye_ecommerce.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok bilgisi gereklidir.")]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual List<ProductImage> ProductImages { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }

        public Product()
        {
            ProductImages = new List<ProductImage>();
            OrderItems = new List<OrderItem>();
            ShoppingCartItems = new List<ShoppingCartItem>();
        }
    }

}
