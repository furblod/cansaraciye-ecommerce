using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using cansaraciye_ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace cansaraciye_ecommerce.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ShoppingCartService shoppingCartService, ApplicationDbContext context)
        {
            _shoppingCartService = shoppingCartService;
            _context = context;
        }

        // Sepete ürün ekleme
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            // Sepette aynı ürün var mı kontrol et
            var existingCartItem = _context.ShoppingCartItems
                .FirstOrDefault(ci => ci.ProductId == productId && ci.UserId == userId);

            if (existingCartItem != null)
            {
                // Eğer ürün zaten sepette varsa sadece miktarı artır
                existingCartItem.Quantity += quantity;
            }
            else
            {
                // Yeni bir ürün ekle
                var newCartItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId
                };

                _context.ShoppingCartItems.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // Kullanıcının sepetini görüntüleme
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = await _shoppingCartService.GetCartItems(userId);
            return View(cartItems);
        }

        // Sepetten ürün kaldırma
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _shoppingCartService.RemoveFromCartAsync(cartItemId);
            return RedirectToAction("Index");
        }

        // Sepeti tamamen temizleme
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _shoppingCartService.ClearCartAsync(userId);
            }
            return RedirectToAction("Index");
        }

        // Ödeme Sayfası
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var cartItems = await _shoppingCartService.GetCartItems(userId);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Sepetiniz boş!";
                return RedirectToAction("Index");
            }

            // **Toplam Tutarı Hesaplayalım**
            decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);

            // **Hata Ayıklama İçin Konsola Yazalım**
            Console.WriteLine($"🛒 Sepette {cartItems.Count} ürün var.");
            Console.WriteLine($"💰 Hesaplanan toplam tutar: {totalAmount} TL");

            var model = new CheckoutViewModel
            {
                FullName = userProfile != null ? $"{userProfile.FirstName} {userProfile.LastName}" : "Ad Soyad Girilmedi",
                Address = userProfile?.Address ?? "Adres Girilmedi",
                City = "İstanbul",
                PhoneNumber = userProfile?.PhoneNumber ?? "0000000000",
                TotalAmount = totalAmount  // **Toplam Tutar Artık Boş Gelmeyecek**
            };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //  Sepetteki ürünleri al
            var cartItems = _context.ShoppingCartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Sepetiniz boş! Sipariş oluşturamazsınız.");
                return View(model);
            }

            //  Yeni sipariş oluştur
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                Address = model.Address,
                City = model.City,
                PhoneNumber = model.PhoneNumber,
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>() // Sipariş ürünleri için boş liste başlat
            };

            decimal totalPrice = 0;

            // Sepetteki ürünleri siparişe ekle
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Product.Price //  Toplam fiyat hesaplanıyor
                };

                totalPrice += orderItem.TotalPrice;
                order.OrderItems.Add(orderItem);
            }

            //  Siparişi veritabanına kaydet
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Sepeti temizle
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderSuccess");
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutProcess(CheckoutViewModel model, string cardNumber, string expireMonth, string expireYear, string cvc)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // **🚀 1️⃣ Stok kontrolünü sipariş başlamadan yapalım**
            var cartItems = await _shoppingCartService.GetCartItems(userId);

            foreach (var item in cartItems)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null || product.Stock < item.Quantity)
                {
                    Console.WriteLine($"❌ HATA: {product?.Name} stok yetersiz! Maksimum {product?.Stock} adet alınabilir.");
                    TempData["Error"] = $"{product?.Name} için yeterli stok yok! Maksimum {product?.Stock} adet alabilirsiniz.";
                    return RedirectToAction("Index"); // **Sipariş sürecini durduruyoruz!**
                }
            }

            // **🚀 2️⃣ Ödeme işlemini başlat**
            var payment = await _shoppingCartService.ProcessPaymentAsync(userId, model.TotalAmount, cardNumber, expireMonth, expireYear, cvc);

            if (payment == null)
            {
                TempData["Error"] = "Ödeme başarısız oldu!";
                return RedirectToAction("Checkout");
            }

            // **🚀 3️⃣ Sipariş oluştur (Eğer stok uygunsa)**
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                Address = model.Address,
                City = model.City,
                PhoneNumber = model.PhoneNumber,
                OrderDate = DateTime.Now,
                Status = "Onay Bekleniyor",
               // TotalPrice = model.TotalAmount
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // **🚀 4️⃣ Sipariş ürünlerini ekleyelim**
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Product.Price
                };
                _context.OrderItems.Add(orderItem);

                // **🚀 5️⃣ Stokları güncelle**
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    _context.Products.Update(product);
                    Console.WriteLine($"✅ {product.Name} stoktan düşüldü! Kalan stok: {product.Stock}");
                }
            }

            await _context.SaveChangesAsync();
            await _shoppingCartService.ClearCartAsync(userId);

            Console.WriteLine("✅ Sipariş tamamlandı ve stoklar güncellendi!");

            return RedirectToAction("OrderSuccess");
        }

        // Sipariş Tamamlama İşlemi
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            Console.WriteLine("🚀 PlaceOrder metodu çalıştı");

            var cartItems = await _shoppingCartService.GetCartItems(userId);
            if (cartItems.Count == 0)
            {
                Console.WriteLine("❌ Sepet boş!");
                TempData["Error"] = "Sepetiniz boş, sipariş oluşturulamadı!";
                return RedirectToAction("Index", "ShoppingCart");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                    {
                        Console.WriteLine("❌ Ürün bulunamadı!");
                        TempData["Error"] = "Üzgünüz, bazı ürünler bulunamadı!";
                        return RedirectToAction("Index", "ShoppingCart");
                    }

                    if (item.Quantity > product.Stock)
                    {
                        Console.WriteLine($"❌ Stok yetersiz! {product.Name} stok: {product.Stock}");
                        TempData["Error"] = $"Üzgünüz, {product.Name} stokta sadece {product.Stock} adet mevcut!";
                        return RedirectToAction("Index", "ShoppingCart");
                    }

                    product.Stock -= item.Quantity; // 🔹 Stok düş
                    Console.WriteLine($"✅ {product.Name} stok güncellendi, yeni stok: {product.Stock}");
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                    _context.Entry(product).State = EntityState.Detached;

                }

                await _context.SaveChangesAsync(); // 🔹 Stok güncellenmeli
                Console.WriteLine("📌 Stok güncellemeleri kaydedildi!");

                order.UserId = userId;
                order.OrderDate = DateTime.Now;
                order.Status = "Onay Bekleniyor";

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // 🔹 Sipariş ID oluşsun
                Console.WriteLine($"📌 Sipariş oluşturuldu, ID: {order.Id}");

                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = item.Quantity * item.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync(); // 🔹 Sipariş ürünlerini kaydet
                Console.WriteLine("📌 Sipariş ürünleri kaydedildi!");

                await transaction.CommitAsync(); // 🔹 Transaction tamamla
                Console.WriteLine("🎉 Sipariş işlemi tamamlandı!");

                await _shoppingCartService.ClearCartAsync(userId); // 🔹 Sepeti temizle

                return RedirectToAction("OrderSuccess");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // 🔹 Hata olursa işlemi geri al
                Console.WriteLine($"❌ Sipariş oluşturulurken hata oluştu: {ex.Message}");
                TempData["Error"] = "Sipariş oluşturulurken hata oluştu!";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        // Sipariş Başarılı Sayfası
        public IActionResult OrderSuccess()
        {
            return View();
        }

        // Sepetteki ürün miktarını artır
        public async Task<IActionResult> IncreaseQuantity(int cartItemId)
        {
            await _shoppingCartService.IncreaseQuantityAsync(cartItemId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DecreaseQuantity(int cartItemId)
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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
