using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Data;
using Prueba3._0.Models;

namespace Prueba3._0.Controllers;

[Authorize(Roles = "Operario")]
public class ConsultaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _config;

    public ConsultaController(ApplicationDbContext context, UserManager<Usuario> userManager, IConfiguration config)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.Documentos
            .Where(d => d.Estado == "Aprobado")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.Titulo.Contains(q));

        var docs = await query
            .OrderByDescending(d => d.FechaModificacion)
            .ToListAsync();

        ViewBag.Q = q;
        ViewBag.ModuloConsultaUrl = _config["ModuloConsulta:Url"];
        return View(docs);
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc?.RutaArchivo == null) return NotFound();
        if (doc.Estado != "Aprobado") return Forbid();

        var userId = _userManager.GetUserId(User)!;
        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Descargó",
            Fecha = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return PhysicalFile(doc.RutaArchivo, "application/octet-stream", doc.NombreArchivoOriginal ?? "documento");
    }
}
