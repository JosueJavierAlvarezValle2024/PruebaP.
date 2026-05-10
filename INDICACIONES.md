Estoy trabajando en NormaDoc, un Sistema de Gestión Documental en ASP.NET Core MVC 
10 con C# y SQL Server. Ya tengo el frontend completo (vistas .cshtml y CSS).

TAREA DE ESTE PROMPT: Configurar Entity Framework Core y crear los modelos 
que representan las tablas de SQL Server. Solo esto, sin tocar las vistas.

════════════════════════════════════════
PASO 1 — Paquetes NuGet necesarios
════════════════════════════════════════
Dime exactamente estos comandos para ejecutar en la Consola del Administrador de Paquetes:
- Microsoft.EntityFrameworkCore.SqlServer (versión compatible con .NET 10)
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.AspNetCore.Identity.EntityFrameworkCore

════════════════════════════════════════
PASO 2 — Modelos (carpeta Models/)
════════════════════════════════════════
Crea estos archivos de modelos C#:

Models/Usuario.cs
- Hereda de IdentityUser (para usar el sistema de login de .NET)
- Propiedades adicionales: Nombre (string), Rol (string), FechaCreacion (DateTime)
- Roles posibles como constantes: "Admin", "Revisor", "Operario"

Models/Documento.cs
- Id (int, PK)
- Titulo (string, requerido, max 200 chars)
- Version (string, ej: "1.0", "2.3")
- Estado (string): "Borrador" | "EnRevision" | "Aprobado" | "Obsoleto"
- FechaCreacion (DateTime)
- FechaModificacion (DateTime)
- RutaArchivo (string) — ruta física donde se guardó el archivo subido
- NombreArchivoOriginal (string)
- Extension (string) — "pdf", "docx", etc.
- TamanioKb (double)
- UsuarioId (string, FK a Usuario)
- Propiedad de navegación: Usuario

Models/HistorialVersion.cs
- Id (int, PK)
- DocumentoId (int, FK)
- VersionAnterior (string)
- VersionNueva (string)
- FechaCambio (DateTime)
- UsuarioId (string, FK) — quién hizo el cambio
- Propiedades de navegación: Documento, Usuario

Models/Auditoria.cs
- Id (int, PK)
- DocumentoId (int, FK)
- UsuarioId (string, FK)
- Accion (string) — "Creó", "Editó", "Aprobó", "Rechazó", "Descargó"
- Fecha (DateTime)
- Detalle (string) — descripción adicional
- Propiedades de navegación: Documento, Usuario

════════════════════════════════════════
PASO 3 — DbContext (Data/ApplicationDbContext.cs)
════════════════════════════════════════
- Hereda de IdentityDbContext<Usuario>
- DbSet para: Documentos, HistorialVersiones, Auditorias
- En OnModelCreating:
  * Llama base.OnModelCreating(builder)
  * Índice único en Documento: Titulo + Version
  * Valor por defecto de FechaCreacion: DateTime.UtcNow
  * Valor por defecto de Auditoria.Fecha: DateTime.UtcNow

════════════════════════════════════════
PASO 4 — appsettings.json
════════════════════════════════════════
Agrega la cadena de conexión:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=NormaDocDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
(Si usa Docker más adelante, la cambiaremos por variables de entorno)

════════════════════════════════════════
PASO 5 — Program.cs
════════════════════════════════════════
Agrega los servicios necesarios:
- builder.Services.AddDbContext<ApplicationDbContext>(...)
- builder.Services.AddIdentity<Usuario, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
  }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()
- builder.Services.AddControllersWithViews()
- app.UseAuthentication() ANTES de app.UseAuthorization()

════════════════════════════════════════
PASO 6 — Migración inicial
════════════════════════════════════════
Dame los comandos exactos para:
1. Crear la primera migración: Add-Migration InitialCreate
2. Aplicarla a la base de datos: Update-Database

RESTRICCIONES:
- NO modificar las vistas .cshtml existentes
- NO crear controladores todavía
- Solo modelos, DbContext y configuración