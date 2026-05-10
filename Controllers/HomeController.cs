using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Data;
using Prueba3._0.Models;
using Prueba3._0.ViewModels;

namespace Prueba3._0.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Dashboard));
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var usuario = await _userManager.GetUserAsync(User);

        var cultura = new CultureInfo("es-ES");
        var fechaHoy = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", cultura);
        fechaHoy = char.ToUpper(fechaHoy[0]) + fechaHoy[1..];

        var totalDocs    = await _context.Documentos.CountAsync();
        var enRevision   = await _context.Documentos.CountAsync(d => d.Estado == "EnRevision");
        var aprobados    = await _context.Documentos.CountAsync(d => d.Estado == "Aprobado");
        var obsoletos    = await _context.Documentos.CountAsync(d => d.Estado == "Obsoleto");
        var borradores   = await _context.Documentos.CountAsync(d => d.Estado == "Borrador");

        var actividad = await _context.Auditorias
            .Include(a => a.Usuario)
            .Include(a => a.Documento)
            .OrderByDescending(a => a.Fecha)
            .Take(10)
            .ToListAsync();

        var pendientes = await _context.Documentos
            .Include(d => d.Usuario)
            .Where(d => d.Estado == "EnRevision")
            .OrderByDescending(d => d.FechaModificacion)
            .Take(10)
            .ToListAsync();

        var vigentes = await _context.Documentos
            .Include(d => d.Usuario)
            .Where(d => d.Estado == "Aprobado")
            .OrderByDescending(d => d.FechaModificacion)
            .Take(10)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            NombreUsuario    = usuario?.Nombre ?? User.Identity?.Name ?? "Usuario",
            RolUsuario       = usuario?.Rol ?? string.Empty,
            FechaHoy         = fechaHoy,
            TotalDocumentos  = totalDocs,
            EnRevision       = enRevision,
            Aprobados        = aprobados,
            Obsoletos        = obsoletos,
            Borradores       = borradores,
            ActividadReciente = actividad,
            PendientesRevision = pendientes,
            DocumentosVigentes = vigentes
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
