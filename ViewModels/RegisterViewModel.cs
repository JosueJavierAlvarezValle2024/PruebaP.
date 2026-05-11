using System.ComponentModel.DataAnnotations;

namespace Prueba3._0.ViewModels;

public class RegisterViewModel
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;
}
