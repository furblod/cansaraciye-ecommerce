using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using cansaraciye_ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace cansaraciye_ecommerce.Controllers
{
    [Authorize] // Kullanıcının giriş yapmasını zorunlu kılar
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
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Kullanıcı kimliği
            if (userId == null) return RedirectToAction("Login", "Account"); // Giriş yapmamışsa yönlendir

            await _shoppingCartService.AddToCartAsync(productId, userId);
            return RedirectToAction("Index", "Home");
        }

        // Kullanıcının sepetini görüntüleme
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = await _shoppingCartService.GetCartItemsAsync(userId);
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
        public IActionResult Checkout()
        {
            return View();
        }

        // Sipariş Tamamlama İşlemi
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewData.ModelState.AddModelError("", "HATA: ModelState geçersiz!");
                return View("Checkout", order);
            }

            // Kullanıcı kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Content("HATA: Kullanıcı giriş yapmamış!");
            }

            // UserId'yi ata
            order.UserId = userId;
            order.OrderDate = DateTime.Now;

            // Veritabanına siparişi ekle
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Önce sipariş kaydedilmeli ki ID oluşsun

            // Kullanıcının sepetindeki ürünleri al
            var cartItems = await _shoppingCartService.GetCartItemsAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return Content("HATA: Sepetiniz boş, sipariş oluşturulamadı!");
            }

            // Sepetteki her ürün için OrderItem oluştur
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id, // Yeni oluşturulan siparişin ID'si
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * item.Product.Price // Toplam fiyat hesaplandı
                };
                _context.OrderItems.Add(orderItem);
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            // Sepeti temizle
            await _shoppingCartService.ClearCartAsync(userId);

            // Başarı sayfasına yönlendir
            return RedirectToAction("OrderSuccess", "ShoppingCart");
        }

        // Sipariş Başarılı Sayfası
        public IActionResult OrderSuccess()
        {
            return View();
        }
    }
}
