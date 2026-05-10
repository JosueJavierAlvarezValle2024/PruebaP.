namespace Prueba3._0.Models;

public class HistorialVersion
{
    public int Id { get; set; }
    public int DocumentoId { get; set; }

    public required string VersionAnterior { get; set; }
    public required string VersionNueva { get; set; }
    public DateTime FechaCambio { get; set; }

    public required string UsuarioId { get; set; }
    public string? Notas { get; set; }

    public Documento? Documento { get; set; }
    public Usuario? Usuario { get; set; }
}
