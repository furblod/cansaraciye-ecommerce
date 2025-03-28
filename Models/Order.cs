using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cansaraciye_ecommerce.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Adres gereklidir.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Şehir gereklidir.")]
        public string? City { get; set; }
        [Required]
        public string District { get; set; }        // İlçe

        [Required]
        public string Neighborhood { get; set; }    // Mahalle

        [Required]
        public string Street { get; set; }          // Sokak

        [Required]
        public string BuildingNo { get; set; }      // Bina No

        [Required]
        public string? ApartmentNo { get; set; }     // Daire No

        [Required(ErrorMessage = "Telefon gereklidir.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; }

        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "Onay Bekleniyor";
        public List<OrderItem>? OrderItems { get; set; }
    }
}
