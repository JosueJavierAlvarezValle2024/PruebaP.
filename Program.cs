using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Data;
using Prueba3._0.Models;
using Prueba3._0.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50 MB
});

builder.Services.AddHttpClient<BusquedaApiService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

    foreach (var rol in new[] { Usuario.RolAdmin, Usuario.RolRevisor, Usuario.RolOperario })
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));

    if (!userManager.Users.Any(u => u.Rol == Usuario.RolAdmin))
    {
        var admin = new Usuario
        {
            UserName      = "admin@normadoc.com",
            Email         = "admin@normadoc.com",
            Nombre        = "Administrador",
            Rol           = Usuario.RolAdmin,
            FechaCreacion = DateTime.UtcNow
        };
        var seedResult = await userManager.CreateAsync(admin, "Admin123!");
        if (seedResult.Succeeded)
            await userManager.AddToRoleAsync(admin, Usuario.RolAdmin);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
