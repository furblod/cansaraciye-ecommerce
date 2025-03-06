using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cansaraciye_ecommerce.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir.")]
        public string Name { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
