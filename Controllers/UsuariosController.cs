using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Prueba3._0.Models;

namespace Prueba3._0.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController : Controller
{
    private readonly UserManager<Usuario> _userManager;

    public UsuariosController(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        var usuarios = _userManager.Users.OrderBy(u => u.Nombre).ToList();
        return View(usuarios);
    }
}
