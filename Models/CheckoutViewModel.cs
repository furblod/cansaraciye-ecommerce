using System.ComponentModel.DataAnnotations;

namespace cansaraciye_ecommerce.Models
{
    public class CheckoutViewModel
    {
        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Şehir")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Toplam Tutar")]
        public decimal TotalAmount { get; set; }
    }
}
