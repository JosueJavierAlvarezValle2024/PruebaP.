using System.ComponentModel.DataAnnotations;

namespace Prueba3._0.ViewModels;

public class DocumentoFormViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public string Version { get; set; } = "1.0";
    public string? Categoria { get; set; }
    public string? Descripcion { get; set; }
    public string? Tags { get; set; }

    public IFormFile? Archivo { get; set; }

    public string EstadoInicial { get; set; } = "borrador";

    // Sólo para la vista Edit: datos del archivo actual
    public string? NombreArchivoActual { get; set; }
    public double TamanioActualKb { get; set; }
}
