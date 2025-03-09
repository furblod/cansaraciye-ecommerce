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
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
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
        public async Task<IActionResult> CreateProduct(Product product)
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
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                Console.WriteLine("ÜRÜN EKLENDİ: " + product.Name);
                return RedirectToAction("ProductList");
            }
            catch (Exception ex)
            {
                Console.WriteLine("VERİTABANI HATASI: " + ex.Message);
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
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
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            try
            {
                var existingProduct = _context.Products.Find(product.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.CategoryId = product.CategoryId;

                await _context.SaveChangesAsync();
                Console.WriteLine("ÜRÜN GÜNCELLENDİ: " + existingProduct.Name);

                return RedirectToAction("ProductList");
            }
            catch (Exception ex)
            {
                Console.WriteLine("VERİTABANI HATASI: " + ex.Message);
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
        }


        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ProductList");
        }

        public IActionResult Orders()
        {
            var orders = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToList();
            return View(orders);
        }

    }
}
