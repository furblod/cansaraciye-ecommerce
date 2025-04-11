using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
[Authorize]
public class WholesaleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public WholesaleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        ViewBag.Products = _context.Products.Include(p => p.Category).ToList();
        ViewBag.Categories = _context.Categories.ToList();
        return View(new WholesaleRequest());
    }

    [HttpPost]
    public async Task<IActionResult> SubmitRequest(WholesaleRequest model)
    {
        var user = await _userManager.GetUserAsync(User);
        model.UserId = user.Id;

        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            ViewBag.Products = _context.Products.Include(p => p.Category).ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View("Index", model);
        }


        _context.WholesaleRequests.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Talebiniz başarıyla iletildi. En kısa sürede sizinle iletişime geçilecektir.";
        return RedirectToAction("Index");
    }
    public IActionResult CustomRequest()
    {
        ViewBag.Products = _context.Products.ToList();
        return View();
    }

}
