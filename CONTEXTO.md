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
| 4 — Dashboard con datos reales | ✅ Completo | KPIs reales, DocumentosRecientes, routing por rol, stub Consulta |
| 5 — Módulo Consulta + correcciones | ✅ Completo | ConsultaController real, sidebar por rol, Login unificado, seguridad |

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
├── ConsultaController.cs      Index(q?) + Download(id) para Operario — solo docs Aprobados
├── DocumentosController.cs    Index, Create, Details, Edit, Approve, Download
└── HomeController.cs          Index, Dashboard (redirect Operario → Consulta), Privacy, Error

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
└── DashboardViewModel.cs      KPIs + ActividadReciente + DocumentosRecientes + PendientesRevision + DocumentosVigentes

Views/
├── Shared/
│   ├── _Layout.cshtml         Topbar + sidebar azul marino + footer
│   ├── _AuthLayout.cshtml     Card centrada, sin sidebar
│   └── _ViewImports.cshtml    Usings globales: ViewModels + Helpers + Taghelpers
├── Account/
│   ├── Login.cshtml           @model LoginViewModel, asp-for, validación
│   └── Register.cshtml        @model RegisterViewModel, asp-for, validación
├── Home/
│   └── Dashboard.cshtml       @model DashboardViewModel, KPIs reales, 3 pestañas + tabla DocumentosRecientes
├── Consulta/
│   └── Index.cshtml           Búsqueda + tabla docs Aprobados + enlace Módulo PHP
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
| `/Consulta` | GET | Lista docs Aprobados con búsqueda `[Authorize(Roles="Operario")]` |
| `/Consulta/Download/{id}` | GET | Descarga doc Aprobado + auditoría `[Authorize(Roles="Operario")]` |

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

---

## Fase 4 — Dashboard con datos reales (última sesión)

Se completó la conexión del Dashboard con SQL Server y se agregó routing por rol.

### Cambios realizados

**`ViewModels/DashboardViewModel.cs`**
- Agregada propiedad `List<Documento> DocumentosRecientes` (últimos 5 docs por `FechaCreacion` desc, todos los estados)

**`Controllers/HomeController.cs`**
- Al inicio de `Dashboard()`: si `usuario.Rol == "Operario"` → `RedirectToAction("Index", "Consulta")`
- Nueva query LINQ: últimos 5 documentos de cualquier estado ordenados por `FechaCreacion` desc, con `Include(d => d.Usuario)`
- Propiedad `DocumentosRecientes = recientes` asignada al ViewModel

**`Views/Home/Dashboard.cshtml`**
- Pestaña Admin: agregada tabla "Documentos Recientes" debajo de la tabla de actividad
- La tabla itera sobre `@Model.DocumentosRecientes` mostrando Título (enlace a Details), Versión, Estado (badge), Autor, Fecha de creación
- Si la lista está vacía muestra mensaje `"Sin documentos registrados."`

**`Controllers/ConsultaController.cs`** *(nuevo)*
- Stub con `[Authorize(Roles = "Operario")]`
- Acción `Index()` devuelve la vista placeholder

**`Views/Consulta/Index.cshtml`** *(nuevo)*
- Vista con `_Layout.cshtml`, mensaje "Módulo en desarrollo", botón para volver al inicio
- Pendiente de reemplazar en Prompt B6

### Comportamiento por rol tras esta fase

| Rol | Comportamiento en `/Home/Dashboard` |
|-----|--------------------------------------|
| Admin | Ve KPIs reales + actividad reciente + documentos recientes |
| Revisor | Ve KPIs reales + documentos pendientes de revisión |
| Operario | Redirigido automáticamente a `/Consulta/Index` |

---

## Fase 5 — Módulo Consulta + correcciones de seguridad y UX

### Módulo de Consulta para Operarios (Prompt B6)

**`Controllers/ConsultaController.cs`** *(reemplazado stub)*
- DI: `ApplicationDbContext`, `UserManager<Usuario>`, `IConfiguration`
- `Index(string? q)`: query solo Estado == "Aprobado", filtro `Titulo.Contains(q)` si q no es vacío, orden por `FechaModificacion` desc. Pasa `ViewBag.Q` y `ViewBag.ModuloConsultaUrl`
- `Download(int id)`: busca doc, verifica `Estado == "Aprobado"` (Forbid si no), registra auditoría "Descargó", retorna `PhysicalFile`
- Clase: `[Authorize(Roles = "Operario")]`

**`Views/Consulta/Index.cshtml`** *(reemplazado placeholder)*
- Barra de búsqueda: `<form method="get">` con input `name="q"` y botón "Limpiar" condicional
- Tabla: Título | Versión | Fecha Aprobación | Extensión | Acción (botón Descargar)
- Estado vacío: "No hay documentos vigentes disponibles."
- Enlace al módulo PHP: `<a href="@ViewBag.ModuloConsultaUrl">Ver portal de consulta pública →</a>` en `card-footer`

**`appsettings.json`**
- Agregada clave `"ModuloConsulta": { "Url": "http://localhost:8080" }`

### Correcciones de seguridad y UX

**`Views/Shared/_Layout.cshtml`**
- "Documentos" en sidebar: oculto a Operarios (`@if (User.IsInRole("Admin") || User.IsInRole("Revisor"))`)
- "Usuarios" en sidebar: oculto a todos salvo Admin (`@if (User.IsInRole("Admin"))`)
- Sección "Gestión" completa oculta a Operarios
- Badge de notificaciones hardcodeado `3` eliminado del topbar

**`Views/Account/Login.cshtml`**
- Eliminado link "¿No tienes cuenta? Regístrate" que causaba loop para usuarios sin rol Admin

**`Controllers/AccountController.cs`**
- Login POST simplificado: todos los roles redirigen a `Dashboard` tras autenticarse
- Eliminada la redirección diferenciada a ModuloPHP para Operarios (el Dashboard ya maneja el redirect a Consulta)

### Comportamiento final por rol

| Rol | Flujo tras login |
|-----|-----------------|
| Admin | Login → Dashboard (KPIs + actividad + docs recientes) |
| Revisor | Login → Dashboard (KPIs + pendientes de revisión) |
| Operario | Login → Dashboard → redirect automático a `/Consulta/Index` |

### Sidebar visible según rol

| Sección | Admin | Revisor | Operario |
|---------|-------|---------|----------|
| Dashboard | ✅ | ✅ | ✅ |
| Documentos | ✅ | ✅ | ❌ |
| Usuarios | ✅ | ❌ | ❌ |
| Reportes | ✅ | ✅ | ❌ |
| Configuración | ✅ | ✅ | ✅ |
| Cerrar sesión | ✅ | ✅ | ✅ |
