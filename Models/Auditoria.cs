namespace Prueba3._0.Models;

public class Auditoria
{
    public int Id { get; set; }
    public int DocumentoId { get; set; }
    public required string UsuarioId { get; set; }

    // Creó | Editó | Aprobó | Rechazó | Descargó
    public required string Accion { get; set; }
    public DateTime Fecha { get; set; }
    public string? Detalle { get; set; }

    public Documento? Documento { get; set; }
    public Usuario? Usuario { get; set; }
}
