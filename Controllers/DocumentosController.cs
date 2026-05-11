using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba3._0.Data;
using Prueba3._0.Helpers;
using Prueba3._0.Models;
using Prueba3._0.Services;
using Prueba3._0.ViewModels;

namespace Prueba3._0.Controllers;

[Authorize]
public class DocumentosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly BusquedaApiService _busqueda;

    private static readonly string[] _extensionesPermitidas = [".pdf", ".docx", ".doc", ".xlsx"];

    public DocumentosController(ApplicationDbContext context,
        UserManager<Usuario> userManager,
        IWebHostEnvironment env,
        BusquedaApiService busqueda)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
        _busqueda = busqueda;
    }

    // GET /Documentos
    [HttpGet]
    [Authorize(Roles = "Admin,Revisor")]
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
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new DocumentoFormViewModel());

    // POST /Documentos/Create
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentoFormViewModel model)
    {
        if (model.Archivo is { Length: > 0 })
        {
            var ext = Path.GetExtension(model.Archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(ext))
            {
                ModelState.AddModelError("Archivo", "Formato no permitido. Solo se aceptan PDF, DOCX, DOC y XLSX.");
            }
        }

        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User)!;
        var now = DateTime.UtcNow;

        var doc = new Documento
        {
            Titulo = model.Titulo,
            Version = model.Version,
            Estado = "Borrador",
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
            Detalle = $"Documento '{doc.Titulo}' creado como Borrador"
        });
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Documento '{doc.Titulo}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Documentos/Details/5
    [HttpGet]
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
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        if (doc.Estado is "Aprobado" or "Obsoleto")
        {
            TempData["Error"] = $"No se puede editar un documento en estado '{EstadoHelper.ToLabel(doc.Estado)}'.";
            return RedirectToAction(nameof(Details), new { id });
        }

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
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentoFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (model.Archivo is { Length: > 0 })
        {
            var ext = Path.GetExtension(model.Archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(ext))
                ModelState.AddModelError("Archivo", "Formato no permitido. Solo se aceptan PDF, DOCX, DOC y XLSX.");
        }

        if (!ModelState.IsValid) return View(model);

        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var versionAnterior = doc.Version;

        if (model.Archivo is { Length: > 0 })
        {
            _context.HistorialVersiones.Add(new HistorialVersion
            {
                DocumentoId = doc.Id,
                VersionAnterior = versionAnterior,
                VersionNueva = model.Version,
                FechaCambio = DateTime.UtcNow,
                UsuarioId = userId,
                Notas = $"Reemplazo de archivo en v{model.Version}"
            });
            GuardarArchivo(model.Archivo, doc);
        }

        doc.Titulo = model.Titulo;
        doc.Version = model.Version;
        doc.Categoria = model.Categoria;
        doc.Descripcion = model.Descripcion;
        doc.Tags = model.Tags;
        doc.Estado = EstadoHelper.Normalizar(model.EstadoInicial);
        doc.FechaModificacion = DateTime.UtcNow;

        _context.Update(doc);

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

    // POST /Documentos/Delete/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        if (doc.Estado != "Borrador")
        {
            TempData["Error"] = "Solo se pueden eliminar documentos en estado Borrador.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User)!;

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Eliminó",
            Fecha = DateTime.UtcNow,
            Detalle = $"Documento '{doc.Titulo}' eliminado"
        });
        await _context.SaveChangesAsync();

        _context.Documentos.Remove(doc);
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Documento '{doc.Titulo}' eliminado.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Documentos/Approve/5
    [HttpGet]
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
            "Aprobado" => "Aprobó",
            "Borrador"  => "Rechazó",
            "Obsoleto"  => "Archivó",
            _           => "Editó"
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

        if (doc.Estado == "Aprobado")
            await _busqueda.IndexarDocumento(doc);

        return RedirectToAction(nameof(Details), new { id = doc.Id });
    }

    // POST /Documentos/EnviarARevision/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarARevision(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        if (doc.Estado != "Borrador")
        {
            TempData["Error"] = "Solo se pueden enviar a revisión documentos en estado Borrador.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User)!;
        doc.Estado = "EnRevision";
        doc.FechaModificacion = DateTime.UtcNow;

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Envió a revisión",
            Fecha = doc.FechaModificacion
        });

        _context.Update(doc);
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Documento '{doc.Titulo}' enviado a revisión.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET /Documentos/Aprobar/5
    [HttpGet]
    [Authorize(Roles = "Admin,Revisor")]
    public async Task<IActionResult> Aprobar(int id)
    {
        var doc = await _context.Documentos
            .Include(d => d.Usuario)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        if (doc.Estado != "EnRevision")
        {
            TempData["Error"] = "Solo se pueden aprobar documentos en estado En Revisión.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var historial = await _context.Auditorias
            .Include(a => a.Usuario)
            .Where(a => a.DocumentoId == id)
            .OrderBy(a => a.Fecha)
            .ToListAsync();

        return View("Approve", new ApproveViewModel { Documento = doc, Historial = historial });
    }

    // POST /Documentos/Aprobar/5
    [HttpPost]
    [Authorize(Roles = "Admin,Revisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id, string comentario)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        doc.Estado = "Aprobado";
        doc.FechaModificacion = DateTime.UtcNow;

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Aprobó",
            Fecha = doc.FechaModificacion,
            Detalle = comentario
        });

        _context.Update(doc);
        await _context.SaveChangesAsync();

        await _busqueda.IndexarDocumento(doc);

        TempData["Exito"] = $"Documento '{doc.Titulo}' aprobado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Documentos/Rechazar/5
    [HttpPost]
    [Authorize(Roles = "Admin,Revisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        doc.Estado = "Borrador";
        doc.FechaModificacion = DateTime.UtcNow;

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Rechazó",
            Fecha = doc.FechaModificacion,
            Detalle = motivoRechazo
        });

        _context.Update(doc);
        await _context.SaveChangesAsync();

        TempData["Error"] = $"Documento '{doc.Titulo}' rechazado y devuelto a Borrador.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Documentos/MarcarObsoleto/5
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarObsoleto(int id)
    {
        var doc = await _context.Documentos.FindAsync(id);
        if (doc == null) return NotFound();

        if (doc.Estado != "Aprobado")
        {
            TempData["Error"] = "Solo se pueden marcar como obsoletos documentos en estado Aprobado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = _userManager.GetUserId(User)!;
        doc.Estado = "Obsoleto";
        doc.FechaModificacion = DateTime.UtcNow;

        _context.Auditorias.Add(new Auditoria
        {
            DocumentoId = doc.Id,
            UsuarioId = userId,
            Accion = "Marcó como obsoleto",
            Fecha = doc.FechaModificacion
        });

        _context.Update(doc);
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Documento '{doc.Titulo}' marcado como obsoleto.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET /Documentos/Download/5
    [HttpGet]
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

        return PhysicalFile(doc.RutaArchivo, "application/octet-stream", doc.NombreArchivoOriginal ?? "documento");
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
