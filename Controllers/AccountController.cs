using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prueba3._0.Models;
using Prueba3._0.ViewModels;

namespace Prueba3._0.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IConfiguration configuration)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Home");
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
            return RedirectToAction("Dashboard", "Home");

        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new Usuario
        {
            UserName      = model.Email,
            Email         = model.Email,
            Nombre        = model.Nombre,
            Rol           = model.Rol,
            FechaCreacion = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Rol);
            TempData["Exito"] = $"Usuario {model.Nombre} creado correctamente.";
            return RedirectToAction("Index", "Usuarios");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
