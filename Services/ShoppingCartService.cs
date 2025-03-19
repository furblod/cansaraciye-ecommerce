using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Globalization;

namespace cansaraciye_ecommerce.Services
{
    public class ShoppingCartService
    {
        private readonly ApplicationDbContext _context;
        private readonly Options _options;
        public ShoppingCartService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _options = new Options
            {
                ApiKey = configuration["IyziCo:ApiKey"],
                SecretKey = configuration["IyziCo:SecretKey"],
                BaseUrl = configuration["IyziCo:BaseUrl"]
            };
        }

        // Sepete ürün ekleme
        public async Task AddToCartAsync(int productId, string userId)
        {
            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1
                };
                await _context.ShoppingCartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        // Kullanıcının sepetini getir
        public async Task<List<ShoppingCartItem>> GetCartItems(string userId)
        {
            return await _context.ShoppingCartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        // Sepetten ürün silme
        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        // Sepeti boşalt
        public async Task ClearCartAsync(string userId)
        {
            var cartItems = _context.ShoppingCartItems.Where(c => c.UserId == userId);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task IncreaseQuantityAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity += 1;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DecreaseQuantityAsync(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity -= 1;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(cartItem);
                }
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Payment> ProcessPaymentAsync(string userId, decimal totalAmount, string cardNumber, string expireMonth, string expireYear, string cvc)
        {
            try
            {
                Console.WriteLine("🟢 ProcessPaymentAsync METODU ÇALIŞTI");

                // **1️⃣ Kullanıcı bilgilerini al**
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (userProfile == null || user == null)
                {
                    Console.WriteLine("❌ HATA: Kullanıcı veya kullanıcı profili bulunamadı!");
                    return null;
                }

                // **2️⃣ Sepeti al**
                var cartItems = await _context.ShoppingCartItems
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    Console.WriteLine("❌ HATA: Sepet boş!");
                    return null;
                }

                // **3️⃣ Kart bilgilerini oluştur**
                var paymentCard = new PaymentCard
                {
                    CardHolderName = $"{userProfile.FirstName} {userProfile.LastName}",
                    CardNumber = cardNumber,
                    ExpireMonth = expireMonth,
                    ExpireYear = expireYear,
                    Cvc = cvc,
                    RegisterCard = 0
                };

                // **4️⃣ Kullanıcı bilgilerini hazırla**
                var buyer = new Buyer
                {
                    Id = userId,
                    Name = userProfile.FirstName,
                    Surname = userProfile.LastName,
                    GsmNumber = user.PhoneNumber ?? "+905555555555",
                    Email = user.Email,
                    IdentityNumber = "12345678901",
                    RegistrationAddress = userProfile.Address ?? "Adres bulunamadı",
                    Ip = "85.34.78.112",
                    City = "İstanbul",
                    Country = "Türkiye"
                };

                // **5️⃣ Sepetteki ürünleri IyziCo formatına dönüştür**
                var basketItems = cartItems.Select(item => new BasketItem
                {
                    Id = item.ProductId.ToString(),
                    Name = item.Product.Name ?? "Ürün Adı Yok",
                    Category1 = item.Product.Category?.Name ?? "Genel",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = (item.Quantity * item.Product.Price).ToString("F2", CultureInfo.InvariantCulture)
                }).ToList();

                // **🚀 6️⃣ Eksik olan Teslimat Adresini (ShippingAddress) ekleyelim**
                var shippingAddress = new Address
                {
                    ContactName = $"{userProfile.FirstName} {userProfile.LastName}",
                    City = "İstanbul", // Burayı userProfile'dan alabilirsiniz
                    Country = "Türkiye",
                    Description = userProfile.Address ?? "Adres bulunamadı"
                };

                // **🚀 7️⃣ Fatura Adresi de ekleyelim (Opsiyonel ama önerilir)**
                var billingAddress = new Address
                {
                    ContactName = $"{userProfile.FirstName} {userProfile.LastName}",
                    City = "İstanbul", // Burayı userProfile'dan alabilirsiniz
                    Country = "Türkiye",
                    Description = userProfile.Address ?? "Adres bulunamadı"
                };

                // **🚀 8️⃣ Ödeme isteğini oluştur**
                var request = new CreatePaymentRequest
                {
                    Locale = Locale.TR.ToString(),
                    ConversationId = Guid.NewGuid().ToString(),
                    Price = totalAmount.ToString("F2", CultureInfo.InvariantCulture),
                    PaidPrice = totalAmount.ToString("F2", CultureInfo.InvariantCulture),
                    Currency = Currency.TRY.ToString(),
                    Installment = 1,
                    PaymentCard = paymentCard,
                    Buyer = buyer,
                    ShippingAddress = shippingAddress, // **Teslimat adresini ekledik!**
                    BillingAddress = billingAddress,   // **Fatura adresini ekledik!**
                    BasketItems = basketItems
                };

                // **🚀 9️⃣ Logları ekleyelim**
                Console.WriteLine("📌 Gönderilen Ödeme Bilgileri:");
                Console.WriteLine($"   - Price: {request.Price}");
                Console.WriteLine($"   - PaidPrice: {request.PaidPrice}");
                Console.WriteLine($"   - Currency: {request.Currency}");
                Console.WriteLine($"   - Kart Numarası: {paymentCard.CardNumber}");
                Console.WriteLine($"   - Son Kullanma Tarihi: {paymentCard.ExpireMonth}/{paymentCard.ExpireYear}");
                Console.WriteLine($"   - CVC: {paymentCard.Cvc}");
                Console.WriteLine($"   - Sepette {basketItems.Count} ürün var.");
                Console.WriteLine($"   - **Shipping Address:** {shippingAddress.Description}");

                // **🚀 10️⃣ IyziCo ödeme isteğini gerçekleştir**
                var payment = await Task.Run(() => Payment.Create(request, _options));

                Console.WriteLine("🟢 Ödeme Yanıtı: " + payment.Status);
                Console.WriteLine("🟢 Hata Mesajı: " + payment.ErrorMessage);

                if (payment.Status != "success")
                {
                    Console.WriteLine($"❌ Ödeme başarısız! Hata Mesajı: {payment.ErrorMessage}");
                    return null;
                }

                Console.WriteLine("✅ ÖDEME BAŞARILI!");
                return payment;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Ödeme sırasında hata oluştu: " + ex.Message);
                return null;
            }
        }


    }
}

