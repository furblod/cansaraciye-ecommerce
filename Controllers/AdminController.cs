using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace cansaraciye_ecommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CategoryList()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("HATA: ModelState geçerli değil!");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine("ModelState Hatası: " + error.ErrorMessage);
                    }
                }
                return View(category);
            }

            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                Console.WriteLine("KATEGORİ EKLENDİ: " + category.Name);
                return RedirectToAction("CategoryList");
            }
            catch (Exception ex)
            {
                Console.WriteLine("VERİTABANI HATASI: " + ex.Message);
                return View(category);
            }
        }


        [HttpGet]
        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("CategoryList");
            }
            return View(category);
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.ProductImages)
                .Include(c => c.Products)
                    .ThenInclude(p => p.OrderItems)
                .Include(c => c.Products)
                    .ThenInclude(p => p.ShoppingCartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                foreach (var product in category.Products.ToList())
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                    _context.OrderItems.RemoveRange(product.OrderItems);
                    _context.ShoppingCartItems.RemoveRange(product.ShoppingCartItems);
                    _context.Products.Remove(product);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CategoryList");
        }

        public IActionResult ProductList()
        {
            var products = _context.Products.Include(x => x.Category).ToList();
            return View(products);
        }
        [HttpGet]
        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile MainImage, List<IFormFile> ExtraImages, IFormFile ProductVideo)
        {
            if (ModelState.IsValid)
            {
                if (product.Stock < 0)
                {
                    ModelState.AddModelError("Stock", "Stok miktarı negatif olamaz.");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                // 1️⃣ Ana görseli yükle
                if (MainImage != null && MainImage.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await MainImage.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/" + fileName;
                }

                // 2️⃣ Video dosyasını yükle
                if (ProductVideo != null && ProductVideo.Length > 0)
                {
                    var videoFileName = Guid.NewGuid() + Path.GetExtension(ProductVideo.FileName);
                    var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", videoFileName);

                    using (var videoStream = new FileStream(videoPath, FileMode.Create))
                    {
                        await ProductVideo.CopyToAsync(videoStream);
                    }

                    product.VideoUrl = "/videos/" + videoFileName;
                }

                // 3️⃣ Ürünü kaydet
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 4️⃣ Ekstra görselleri yükle
                if (ExtraImages != null && ExtraImages.Any())
                {
                    foreach (var image in ExtraImages)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var productImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/images/" + fileName
                        };

                        _context.ProductImages.Add(productImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }


        public IActionResult EditProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }


        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, IFormFile MainImage, List<IFormFile> ExtraImages, IFormFile ProductVideo, string Name, string Description, decimal Price, int Stock, int CategoryId)
        {
            var existingProduct = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
                return NotFound();

            // Temel verileri güncelle
            existingProduct.Name = Name;
            existingProduct.Description = Description;
            existingProduct.Price = Price;
            existingProduct.Stock = Stock;
            existingProduct.CategoryId = CategoryId;

            // Ana görsel güncelle
            if (MainImage != null && MainImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(MainImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await MainImage.CopyToAsync(stream);
                existingProduct.ImageUrl = "/images/" + fileName;
            }

            // Yeni video dosyası güncelle
            if (ProductVideo != null && ProductVideo.Length > 0)
            {
                var videoFileName = Guid.NewGuid() + Path.GetExtension(ProductVideo.FileName);
                var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", videoFileName);
                using var stream = new FileStream(videoPath, FileMode.Create);
                await ProductVideo.CopyToAsync(stream);
                existingProduct.VideoUrl = "/videos/" + videoFileName;
            }

            // Yeni ek görseller
            if (ExtraImages != null && ExtraImages.Any())
            {
                foreach (var image in ExtraImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await image.CopyToAsync(stream);

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = existingProduct.Id,
                        ImageUrl = "/images/" + fileName
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ProductList");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProductVideo(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || string.IsNullOrEmpty(product.VideoUrl))
                return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.VideoUrl.TrimStart('/'));
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            product.VideoUrl = null;
            await _context.SaveChangesAsync();

            return RedirectToAction("EditProduct", new { id = productId });
        }


        [HttpGet]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var image = await _context.ProductImages.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (image == null) return NotFound();

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            var productId = image.ProductId;

            var imageToDelete = new ProductImage { Id = id };
            _context.ProductImages.Attach(imageToDelete);
            _context.ProductImages.Remove(imageToDelete);
            await _context.SaveChangesAsync();

            return RedirectToAction("EditProduct", new { id = productId });
        }


        [HttpGet]
        public async Task<IActionResult> DeleteMainImage(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                product.ImageUrl = null;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditProduct", new { id = productId });
        }



        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.OrderItems)
                .Include(p => p.ShoppingCartItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Ürün görsellerini sil
                _context.ProductImages.RemoveRange(product.ProductImages);

                // Sipariş ürünlerini sil (Siparişin kendisini değil sadece ilişkili satırları)
                _context.OrderItems.RemoveRange(product.OrderItems);

                // Sepet ürünlerini sil
                _context.ShoppingCartItems.RemoveRange(product.ShoppingCartItems);

                // Ana ürünü sil
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ProductList");
        }

        public IActionResult Orders(string statusFilter, string sortOrder)
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable(); // Dinamik sorgu oluşturmak için AsQueryable() kullanıyoruz

            //  **Sipariş Durumuna Göre Filtreleme**
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Tümü")
            {
                orders = orders.Where(o => o.Status == statusFilter);
            }

            //  **Tarihe Göre Sıralama**
            switch (sortOrder)
            {
                case "newest":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "oldest":
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
                default:
                    orders = orders.OrderByDescending(o => o.OrderDate); // Varsayılan: Yeniden eskiye
                    break;
            }

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortOrder = sortOrder;

            return View(orders.ToList());
        }


        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                Console.WriteLine($"❌ HATA: Sipariş {id} bulunamadı!");
                return NotFound();
            }

            Console.WriteLine($"✅ Sipariş {id} detayları alındı! {order.OrderItems.Count} ürün bulundu.");

            return View(order);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            _context.OrderItems.RemoveRange(order.OrderItems); // Sipariş ürünlerini sil
            _context.Orders.Remove(order); // Siparişi sil
            _context.SaveChanges();

            return RedirectToAction("Orders");
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Sipariş iptal edildiyse stok geri eklenmeli
                if (status == "İptal Edildi" && order.Status != "İptal Edildi")
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity; // 🔹 Stok geri artır
                            _context.Products.Update(product);
                        }
                    }
                }

                order.Status = status;
                _context.Orders.Update(order);
                _context.SaveChanges();
                transaction.Commit();

                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = "Sipariş durumu güncellenirken hata oluştu!";
                return RedirectToAction("Orders");
            }
        }
        public IActionResult WholesaleRequests()
        {
            var requests = _context.WholesaleRequests
                .Include(r => r.User)
                .Include(r => r.SelectedProduct != null ? r.SelectedProduct : null)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(requests);
        }
        public IActionResult WholesaleRequestDetails(int id)
        {
            var request = _context.WholesaleRequests
                .Include(r => r.SelectedProduct)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == id);


            if (request == null)
                return NotFound();

            return View(request);
        }

        public IActionResult DeleteWholesaleRequest(int id)
        {
            var request = _context.WholesaleRequests.Find(id);
            if (request == null)
                return NotFound();

            _context.WholesaleRequests.Remove(request);
            _context.SaveChanges();

            return RedirectToAction("WholesaleRequests");
        }
    }
}
