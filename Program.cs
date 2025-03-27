using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Models;
using cansaraciye_ecommerce.Services;

var builder = WebApplication.CreateBuilder(args);

//  Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Identity Ayarları
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // E-posta doğrulama gereksiz
    options.User.RequireUniqueEmail = true; // Her e-posta tekil olmalı
})
    .AddRoles<IdentityRole>() // Roller aktif edildi
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ShoppingCartService>();

var app = builder.Build();

// Middleware Tanımları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity için gerekli

// Admin ve Roller İçin DbInitializer Çalıştırma
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Admin ve roller oluşturuluyor...");

        // **DbInitializer asenkron çalıştığı için bekletiyoruz**
        var task = DbInitializer.SeedRolesAndAdmin(services);
        task.Wait(); // Asenkron işlemi beklet

        logger.LogInformation("Admin ve roller başarıyla oluşturuldu.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Admin ve roller oluşturulurken hata oluştu!");
    }
}

// Kullanıcı Profilleri İçin Otomatik Veri Ekleme
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var user in userManager.Users)
    {
        if (!dbContext.UserProfiles.Any(p => p.UserId == user.Id))
        {
            dbContext.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FirstName = "",
                LastName = "",
                Address = "",
                PhoneNumber = user.PhoneNumber ?? "" // Null kontrolü
            });
        }
    }

    dbContext.SaveChanges();
}

// 📌 Uygulamayı Başlat
app.Run();
