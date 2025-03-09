using System.ComponentModel.DataAnnotations.Schema;

namespace cansaraciye_ecommerce.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; } // Siparişle ilişkilendirildi

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; } // Ürünle ilişkilendirildi

        public int Quantity { get; set; } // Adet bilgisi
        public decimal TotalPrice { get; set; } // Toplam fiyat
    }
}
