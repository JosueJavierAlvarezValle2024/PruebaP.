# NormaDoc — Documentación del Proyecto

Sistema de Gestión Documental para Normativas de Calidad.
Construido en ASP.NET Core MVC 10.0 con Entity Framework Core, ASP.NET Core Identity y SQL Server LocalDB.

---

## Resumen de progreso

| Fase | Estado | Descripción |
|------|--------|-------------|
| 1 — Frontend visual | ✅ Completo | 16 vistas .cshtml + CSS personalizado, datos ficticios |
| 2 — Capa de datos | ✅ Completo | EF Core, Identity, DbContext, modelos, migraciones |
| 3 — Lógica de controladores | ✅ Completo | Controladores reales, ViewModels, helper, BD operativa |

---

## Stack utilizado

- **Framework:** ASP.NET Core MVC (.NET 10.0)
- **ORM:** Entity Framework Core 10.0.7 (SqlServer + Tools + Design)
- **Autenticación:** ASP.NET Core Identity (`IdentityUser → Usuario`, `IdentityRole`)
- **Base de datos:** SQL Server LocalDB `(localdb)\MSSQLLocalDB` → BD `NormaDocDB`
- **UI:** Bootstrap 5 (CDN), Bootstrap Icons (CDN), Google Fonts (Inter + Space Grotesk)
- **CSS:** Archivos personalizados por módulo

---

## Estructura completa de archivos

```
Controllers/
├── AccountController.cs       Login, Register, Logout (Identity real)
├── DocumentosController.cs    Index, Create, Details, Edit, Approve, Download
└── HomeController.cs          Index, Dashboard, Privacy, Error

Data/
└── ApplicationDbContext.cs    IdentityDbContext<Usuario>, DbSets, OnModelCreating

Helpers/
└── EstadoHelper.cs            ToBadgeClass, ToLabel, Normalizar, Iniciales

Models/
├── Usuario.cs                 Hereda IdentityUser + Nombre, Rol, FechaCreacion
├── Documento.cs               Entidad principal con nav props e ICollections
├── HistorialVersion.cs        Registro de cambios de versión
└── Auditoria.cs               Log de acciones (Creó/Editó/Aprobó/Rechazó/Descargó)

ViewModels/
├── LoginViewModel.cs
├── RegisterViewModel.cs
├── DocumentoFormViewModel.cs  Creación y edición (incluye IFormFile)
├── ApproveViewModel.cs        Flujo de aprobación
└── DashboardViewModel.cs      KPIs + listas para el panel de control

Views/
├── Shared/
│   ├── _Layout.cshtml         Topbar + sidebar azul marino + footer
│   ├── _AuthLayout.cshtml     Card centrada, sin sidebar
│   └── _ViewImports.cshtml    Usings globales: ViewModels + Helpers + Taghelpers
├── Account/
│   ├── Login.cshtml           @model LoginViewModel, asp-for, validación
│   └── Register.cshtml        @model RegisterViewModel, asp-for, validación
├── Home/
│   └── Dashboard.cshtml       @model DashboardViewModel, KPIs reales, 3 pestañas
└── Documentos/
    ├── Index.cshtml            @model IEnumerable<Documento>
    ├── Create.cshtml           @model DocumentoFormViewModel
    ├── Details.cshtml          @model Documento (nav props, historial, auditoría)
    ├── Edit.cshtml             @model DocumentoFormViewModel (archivo actual visible)
    └── Approve.cshtml          @model ApproveViewModel (flujo + historial real)

wwwroot/
├── css/
│   ├── site.css               Variables CSS globales + estilos base
│   ├── dashboard.css          KPI cards, tabs, progress bars
│   ├── documentos.css         CRUD, dropzone, tablas, badges
│   └── aprobacion.css         Flujo de aprobación paso a paso
└── uploads/                   Archivos subidos (guid + extensión original)
```

---

## Modelos y relaciones

```
AspNetUsers (Usuario)
    │── Id (PK, string)
    │── Nombre (required)
    │── Rol: Admin | Revisor | Operario
    └── FechaCreacion

Documentos
    │── Id (PK, int)
    │── Titulo (required, MaxLength 200) + Version → índice único
    │── Estado: Borrador | EnRevision | Aprobado | Obsoleto
    │── Categoria?, Descripcion?, Tags?
    │── RutaArchivo?, NombreArchivoOriginal?, Extension?, TamanioKb
    │── FechaCreacion (default getutcdate()), FechaModificacion
    │── UsuarioId (FK → AspNetUsers, Restrict on delete)
    ├── HistorialVersiones (ICollection)
    └── Auditorias (ICollection)

HistorialVersiones
    │── DocumentoId (FK → Documentos, Restrict)
    │── UsuarioId   (FK → AspNetUsers, Restrict)
    │── VersionAnterior, VersionNueva, FechaCambio, Notas?

Auditorias
    │── DocumentoId (FK → Documentos, Restrict)
    │── UsuarioId   (FK → AspNetUsers, Restrict)
    └── Accion, Fecha (default getutcdate()), Detalle?
```

> **Nota clave:** Todas las FK secundarias usan `DeleteBehavior.Restrict` para evitar el error 1785 de SQL Server (múltiples rutas en cascada).

---

## Rutas disponibles

| Ruta | Método | Descripción |
|------|--------|-------------|
| `/Account/Login` | GET / POST | Inicio de sesión (Identity) |
| `/Account/Register` | GET / POST | Registro de usuario |
| `/Account/Logout` | POST | Cierre de sesión |
| `/Home/Index` | GET | Redirige a Dashboard si autenticado |
| `/Home/Dashboard` | GET | Panel de control `[Authorize]` |
| `/Documentos` | GET | Lista de documentos `[Authorize]` |
| `/Documentos/Create` | GET / POST | Crear documento + subir archivo |
| `/Documentos/Details/{id}` | GET | Detalle completo con historial |
| `/Documentos/Edit/{id}` | GET / POST | Editar, genera nueva versión automática |
| `/Documentos/Approve/{id}` | GET / POST | Flujo de aprobación + auditoría |
| `/Documentos/Download/{id}` | GET | Descarga archivo, registra auditoría |

---

## Configuración

### Connection string (`appsettings.json`)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NormaDocDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### Identity (`Program.cs`)
```csharp
builder.Services.AddIdentity<Usuario, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### Middleware
```csharp
app.UseAuthentication();   // antes de
app.UseAuthorization();
```

---

## Convenciones del proyecto

- **Estado en BD:** `"Borrador"`, `"EnRevision"`, `"Aprobado"`, `"Obsoleto"` (PascalCase)
- **Estado en vistas:** lowercase (`"borrador"`, `"revision"`) → normalizado via `EstadoHelper.Normalizar()`
- **Strings requeridos en modelos:** `required string` (nullable habilitado)
- **Strings en ViewModels:** `= string.Empty` (sin `required`)
- **Archivos subidos:** `wwwroot/uploads/{guid}{extensión}`, máx. 10MB, solo PDF/DOCX/XLSX
- **Comentarios Razor:** `@* texto *@` — nunca `<!-- -->`
- **CSS:** Un archivo por módulo, nunca `<style>` inline en `.cshtml`

---

## Paleta y badges de estado

| Estado | Color | Clase CSS |
|--------|-------|-----------|
| Borrador | Gris | `badge-borrador` |
| En Revisión | Amarillo | `badge-en-revision` |
| Aprobado | Verde | `badge-aprobado` |
| Obsoleto | Rojo | `badge-obsoleto` |

Variables CSS principales (`site.css`):
- `--sidebar-bg: #1e3a5f`
- `--accent-color: #2d6cdf`
- `--content-bg: #f4f6f9`
- `--font-body: 'Inter'` / `--font-heading: 'Space Grotesk'`

---

## Migraciones

Comandos usados (CLI `dotnet ef`):
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

La BD `NormaDocDB` en LocalDB contiene:
- Tablas de Identity: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.
- Tablas del dominio: `Documentos`, `HistorialVersiones`, `Auditorias`
