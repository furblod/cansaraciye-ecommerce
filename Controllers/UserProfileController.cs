using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using System.Threading.Tasks;
using System.Linq;

namespace cansaraciye_ecommerce.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserProfileController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UserProfile model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
            {
                return NotFound("Profil bulunamadı.");
            }

            profile.FirstName = model.FirstName;
            profile.LastName = model.LastName;
            profile.Address = model.Address;
            profile.PhoneNumber = model.PhoneNumber;

            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Bilgileriniz güncellendi!";
            return RedirectToAction("Index");
        }
    }
}
