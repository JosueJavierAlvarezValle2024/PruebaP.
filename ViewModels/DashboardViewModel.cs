using Prueba3._0.Models;

namespace Prueba3._0.ViewModels;

public class DashboardViewModel
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string RolUsuario { get; set; } = string.Empty;
    public string FechaHoy { get; set; } = string.Empty;

    // KPIs
    public int TotalDocumentos { get; set; }
    public int EnRevision { get; set; }
    public int Aprobados { get; set; }
    public int Obsoletos { get; set; }
    public int Borradores { get; set; }

    // Tabs
    public List<Auditoria> ActividadReciente { get; set; } = [];
    public List<Documento> DocumentosRecientes { get; set; } = [];
    public List<Documento> PendientesRevision { get; set; } = [];
    public List<Documento> DocumentosVigentes { get; set; } = [];
}
