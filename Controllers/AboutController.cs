using Microsoft.AspNetCore.Mvc;

namespace cansaraciye_ecommerce.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
