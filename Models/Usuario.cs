using Microsoft.AspNetCore.Identity;

namespace Prueba3._0.Models;

public class Usuario : IdentityUser
{
    public const string RolAdmin = "Admin";
    public const string RolRevisor = "Revisor";
    public const string RolOperario = "Operario";

    public required string Nombre { get; set; }
    public required string Rol { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
