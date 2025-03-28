using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace cansaraciye_ecommerce.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // IdentityUser ile ilişkilendirme

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public string City { get; set; }           // şehit
        public string? District { get; set; }       // İlçe
        public string? Neighborhood { get; set; }
        // Mahalle
        public string? Street { get; set; }
        // Sokak
        public string? BuildingNo { get; set; }     // Bina No
        public string? ApartmentNo { get; set; }    // Daire No
    }
}
