using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace cansaraciye_ecommerce.Models
{
    public class WholesaleRequest
    {
        public int Id { get; set; }

        [BindNever]
        public string UserId { get; set; }

        [BindNever]
        [ValidateNever]
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [Display(Name = "İşletme Adı")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Ürün Bilgisi veya Özel İstek")]
        public string RequestDetails { get; set; }
        public int? SelectedProductId { get; set; } // dropdown seçimi (isteğe bağlı)


        [Display(Name = "Tahmini Adet")]
        public int? EstimatedQuantity { get; set; }

        [Display(Name = "İletişim Tercihi")]
        public string PreferredContactMethod { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
