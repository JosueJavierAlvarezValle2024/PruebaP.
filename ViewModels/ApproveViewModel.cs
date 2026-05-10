using Prueba3._0.Models;

namespace Prueba3._0.ViewModels;

public class ApproveViewModel
{
    public required Documento Documento { get; set; }
    public List<Auditoria> Historial { get; set; } = [];

    // Campos del formulario de decisión
    public string NuevoEstado { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
}
