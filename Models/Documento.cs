using System.ComponentModel.DataAnnotations;

namespace Prueba3._0.Models;

public class Documento
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public required string Titulo { get; set; }

    public required string Version { get; set; }

    // Borrador | EnRevision | Aprobado | Obsoleto
    public required string Estado { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }

    public string? RutaArchivo { get; set; }
    public string? NombreArchivoOriginal { get; set; }
    public string? Extension { get; set; }
    public double TamanioKb { get; set; }

    public string? Categoria { get; set; }
    public string? Descripcion { get; set; }
    public string? Tags { get; set; }

    public required string UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<HistorialVersion> HistorialVersiones { get; set; } = [];
    public ICollection<Auditoria> Auditorias { get; set; } = [];
}
