using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cansaraciye_ecommerce.Controllers
{
    [Authorize(Roles = "Admin")] // Sadece Admin erişebilir
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
