using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using cansaraciye_ecommerce.Models;
using cansaraciye_ecommerce.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace cansaraciye_ecommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task<IActionResult> Index(string searchTerm, int? categoryId, string sortOrder)
    {
        var products = _context.Products.Include(p => p.Category).AsQueryable();

        // 🔎 **Arama Çubuğu İşlemi**
        if (!string.IsNullOrEmpty(searchTerm))
        {
            products = products.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
        }

        // 🏷 **Kategoriye Göre Filtreleme**
        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId);
        }

        // 💰 **Fiyata Göre Sıralama**
        switch (sortOrder)
        {
            case "price_asc":
                products = products.OrderBy(p => p.Price);
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.Price);
                break;
            default:
                break;
        }

        var categories = await _context.Categories.ToListAsync();
        ViewBag.Categories = categories;

        return View(await products.ToListAsync());
    }

    public async Task<IActionResult> ProductDetails(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
