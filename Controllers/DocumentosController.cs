using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Data;
using Prueba3._0.Helpers;
using Prueba3._0.Models;
using Prueba3._0.ViewModels;

namespace Prueba3._0.Controllers;

[Authorize]
public class DocumentosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IWebHostEnvironment _env;

    public DocumentosController(ApplicationDbContext context,
        UserManager<Usuario> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // GET /Documentos
    public async Task<IActionResult> Index()
    {
        var documentos = await _context.Documentos
            .Include(d => d.Usuario)
            .OrderByDescending(d => d.FechaModificacion)
            .ToListAsync();

        ViewBag.Total = documentos.Count;
        return View(documentos);
    }

    // GET /Documentos/Create
    public IActionResult Create() => View(new DocumentoFormViewModel());

    // POST /Documentos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentoFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        var doc = new Documento
        {
            Titulo = model.Titulo,
            Version = model.Version,
            Estado = EstadoHelper.Normalizar(model.EstadoInicial),
            FechaCreacion = now,
            FechaModificacion = now,
            Categoria = model.Categoria,
            Descripcion = model.Descripcion,
            Tags = model.Tags,
            UsuarioId = userId
        };

        if (model.Archivo is { Length: > 0 })
            GuardarArchivo(model.Archivo, doc);

        _context.Documentos.Add(doc);
        await _context.SaveChangesAsync();

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Creó",
            Fecha = now,
            Detalle = $"Documento creado en estado {EstadoHelper.ToLabel(doc.Estado)}"
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = doc.Id });
    }

    // GET /Documentos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var doc = await _context.Documentos
            .Include(d => d.Usuario)
            .Include(d => d.HistorialVersiones.OrderByDescending(h => h.FechaCambio))
                .ThenInclude(h => h.Usuario)
            .Include(d => d.Auditorias.OrderBy(a => a.Fecha))
                .ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null) return NotFound();
        return View(doc);
    }

    // GET /Documentos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var model = new DocumentoFormViewModel
        {
            Id = doc.Id,
            Titulo = doc.Titulo,
            Version = doc.Version,
            Categoria = doc.Categoria,
            Descripcion = doc.Descripcion,
            Tags = doc.Tags,
            EstadoInicial = doc.Estado.ToLower().Replace("enrevision", "revision"),
            NombreArchivoActual = doc.NombreArchivoOriginal,
            TamanioActualKb = doc.TamanioKb
        };
        return View(model);
    }

    // POST /Documentos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentoFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var versionAnterior = doc.Version;

        doc.Titulo = model.Titulo;
        doc.Version = model.Version;
        doc.Categoria = model.Categoria;
        doc.Descripcion = model.Descripcion;
        doc.Tags = model.Tags;
        doc.Estado = EstadoHelper.Normalizar(model.EstadoInicial);
        doc.FechaModificacion = DateTime.UtcNow;

        if (model.Archivo is { Length: > 0 })
            GuardarArchivo(model.Archivo, doc);

        _context.Update(doc);

        _context.HistorialVersiones.Add(new HistorialVersion
        {
            DocumentoId = doc.Id,
            VersionAnterior = versionAnterior,
            VersionNueva = doc.Version,
            FechaCambio = doc.FechaModificacion,
            UsuarioId = userId,
            Notas = model.Descripcion
        });

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Editó",
            Fecha = doc.FechaModificacion,
            Detalle = $"v{versionAnterior} → v{doc.Version}"
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = doc.Id });
    }

    // GET /Documentos/Approve/5
    public async Task<IActionResult> Approve(int id)
    {
        var doc = await _context.Documentos
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        var historial = await _context.Auditorias
            .Include(a => a.Usuario)
            .Where(a => a.DocumentoId == id)
            .OrderBy(a => a.Fecha)
            .ToListAsync();

        return View(new ApproveViewModel { Documento = doc, Historial = historial });
    }

    // POST /Documentos/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, ApproveViewModel model)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var estadoAnterior = doc.Estado;
        doc.Estado = EstadoHelper.Normalizar(model.NuevoEstado);
        doc.FechaModificacion = DateTime.UtcNow;

        var accion = doc.Estado switch
        {
            "Aprobado"   => "Aprobó",
            "Borrador"   => "Rechazó",
            "Obsoleto"   => "Archivó",
            _            => "Editó"
        };

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = accion,
            Fecha = doc.FechaModificacion,
            Detalle = $"{EstadoHelper.ToLabel(estadoAnterior)} → {EstadoHelper.ToLabel(doc.Estado)}. {model.Comentarios}"
        });

        _context.Update(doc);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = doc.Id });
    }

    // GET /Documentos/Download/5
    public async Task<IActionResult> Download(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc?.RutaArchivo == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Descargó",
            Fecha = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.RutaArchivo);
        return File(bytes, "application/octet-stream", doc.NombreArchivoOriginal ?? "documento");
    }

    // ─── helpers ────────────────────────────────────────────────────────────

    private void GuardarArchivo(IFormFile archivo, Documento doc)
    {
        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploads);

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var ruta = Path.Combine(uploads, fileName);

        using var stream = new FileStream(ruta, FileMode.Create);
        archivo.CopyTo(stream);

        doc.RutaArchivo = ruta;
        doc.NombreArchivoOriginal = archivo.FileName;
        doc.Extension = ext.TrimStart('.');
        doc.TamanioKb = Math.Round(archivo.Length / 1024.0, 1);
    }
}
