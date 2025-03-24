using Microsoft.AspNetCore.Mvc;

namespace cansaraciye_ecommerce.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
