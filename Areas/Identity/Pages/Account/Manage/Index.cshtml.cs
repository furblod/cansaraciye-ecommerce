using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using System.Linq;

namespace cansaraciye_ecommerce.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Display(Name = "Telefon Numarası")]
            [Phone]
            public string PhoneNumber { get; set; }

            [Display(Name = "Adres")]
            public string Address { get; set; }
            [Display(Name = "İl")]
            public string City { get; set; }

            [Display(Name = "İlçe")]
            public string District { get; set; }

            [Display(Name = "Mahalle")]
            public string Neighborhood { get; set; }

            [Display(Name = "Sokak")]
            public string Street { get; set; }

            [Display(Name = "Bina No")]
            public string BuildingNo { get; set; }

            [Display(Name = "Daire No")]
            public string ApartmentNo { get; set; }

        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userProfile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (userProfile == null)
            {
                userProfile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();
            }

            Input = new InputModel
            {
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                PhoneNumber = userProfile.PhoneNumber,
                Address = userProfile.Address,
                City = userProfile.City,
                District = userProfile.District,
                Neighborhood = userProfile.Neighborhood,
                Street = userProfile.Street,
                BuildingNo = userProfile.BuildingNo,
                ApartmentNo = userProfile.ApartmentNo
            };

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userProfile = _context.UserProfiles.FirstOrDefault(p => p.UserId == user.Id);
            if (userProfile == null)
            {
                return NotFound("Profil bulunamadı.");
            }

            userProfile.FirstName = Input.FirstName;
            userProfile.LastName = Input.LastName;
            userProfile.PhoneNumber = Input.PhoneNumber;
            userProfile.Address = Input.Address;
            userProfile.City = Input.City;
            userProfile.District = Input.District;
            userProfile.Neighborhood = Input.Neighborhood;
            userProfile.Street = Input.Street;
            userProfile.BuildingNo = Input.BuildingNo;
            userProfile.ApartmentNo = Input.ApartmentNo;


            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profiliniz başarıyla güncellendi!";
            return RedirectToPage();
        }
    }
}
