# NormaDoc — Documentación del Frontend

Sistema de Gestión Documental para Normativas de Calidad.
Construido como frontend visual en ASP.NET Core MVC con datos ficticios (sin backend ni base de datos).

---

## Stack utilizado

- ASP.NET Core MVC (.NET)
- Bootstrap 5 (CDN)
- Bootstrap Icons (CDN)
- Google Fonts: Inter (texto) + Space Grotesk (títulos)
- CSS personalizado por módulo

---

## Estructura de archivos creados

```
Controllers/
├── AccountController.cs       Login, Register
├── DocumentosController.cs    Index, Create, Details, Edit, Approve
└── HomeController.cs          Index, Dashboard (agregado)

Views/
├── Shared/
│   ├── _Layout.cshtml         Layout principal (topbar + sidebar + footer)
│   └── _AuthLayout.cshtml     Layout para autenticación (sin sidebar)
├── Account/
│   ├── Login.cshtml
│   └── Register.cshtml
├── Home/
│   └── Dashboard.cshtml
└── Documentos/
    ├── Index.cshtml
    ├── Create.cshtml
    ├── Details.cshtml
    ├── Edit.cshtml
    └── Approve.cshtml

wwwroot/css/
├── site.css                   Estilos globales y variables CSS
├── dashboard.css              Estilos del dashboard
├── documentos.css             Estilos del CRUD de documentos
└── aprobacion.css             Estilos del flujo de aprobación
```

---

## Rutas disponibles

| Ruta | Descripción |
|------|-------------|
| `/Account/Login` | Página de inicio de sesión |
| `/Account/Register` | Página de registro de usuario |
| `/Home/Index` | Página de inicio (default) |
| `/Home/Dashboard` | Panel de control principal |
| `/Documentos/Index` | Lista de documentos con búsqueda y filtros |
| `/Documentos/Create` | Formulario para crear documento |
| `/Documentos/Details/1` | Detalle completo de un documento |
| `/Documentos/Edit/1` | Formulario para editar documento |
| `/Documentos/Approve/1` | Flujo de aprobación del documento |

---

## Layouts

**`_Layout.cshtml`** — usado por Dashboard y todas las vistas de Documentos.
- Topbar fija: logo NormaDoc, campana (badge 3), avatar JP / Juan Pérez / Admin
- Sidebar azul marino colapsable con 3 secciones: Navegación, Gestión, Sistema
- Footer: "NormaDoc © 2025 — Sistema de Gestión Documental para Normativas de Calidad"

**`_AuthLayout.cshtml`** — usado por Login y Register.
- Sin sidebar ni topbar
- Fondo degradado azul marino (#1e3a5f → #2d6a9f)
- Card blanca centrada máx. 420px con logo NormaDoc arriba

---

## Paleta de colores y badges de estado

| Estado | Color | Clase CSS |
|--------|-------|-----------|
| Borrador | Gris | `badge-borrador` |
| En Revisión | Amarillo | `badge-en-revision` |
| Aprobado | Verde | `badge-aprobado` |
| Obsoleto | Rojo | `badge-obsoleto` |

Variables CSS principales definidas en `site.css`:
- `--sidebar-bg: #1e3a5f`
- `--accent-color: #2d6cdf`
- `--content-bg: #f4f6f9`
- `--font-body: 'Inter'` / `--font-heading: 'Space Grotesk'`

---

## Convenciones de sintaxis Razor

- Comentarios: `@* texto *@` — nunca `<!-- -->` ni `{{-- --}}`
- Layout principal: `@{ Layout = "~/Views/Shared/_Layout.cshtml"; }`
- Layout auth: `@{ Layout = "~/Views/Shared/_AuthLayout.cshtml"; }`
- CSS por módulo en archivo `.css` separado, nunca en etiqueta `<style>` dentro del `.cshtml`
- Secciones usadas: `@section Styles { }` y `@section Scripts { }`
