using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cansaraciye_ecommerce.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
