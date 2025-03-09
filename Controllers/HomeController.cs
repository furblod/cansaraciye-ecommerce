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
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync(); // Kategorileri çek
        var products = await _context.Products.Include(p => p.Category).ToListAsync(); // Ürünleri çek

        ViewData["Categories"] = categories; // Kategorileri View'a gönder
        return View(products); // Ürünleri View'a gönder
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
