using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cansaraciye_ecommerce.Data;
using cansaraciye_ecommerce.Services;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı Bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Ayarları
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Email doğrulamasını kaldırıyoruz.
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>() // Roller eklendi
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Role ve Admin Kullanıcı oluşturuluyor...");
        
        await DbInitializer.SeedRolesAndAdmin(services);
        logger.LogInformation("Role ve Admin Kullanıcı oluşturuldu.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Role ve Admin Kullanıcı oluşturulurken hata oluştu!");
    }
}


app.Run();

