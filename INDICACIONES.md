Continuamos con NormaDoc. Este es el último paso del Módulo .NET.
Los operarios solo deben consultar documentos aprobados, no administrarlos.

TAREA: Crear una vista simple de consulta para el rol Operario dentro del 
módulo .NET, con un enlace al Módulo PHP para consulta pública completa.

════════════════════════════════════════
Controllers/ConsultaController.cs
════════════════════════════════════════

[GET] Index()
  → [Authorize(Roles = "Operario")]
  → Obtiene solo documentos donde Estado == "Aprobado"
  → Ordena por FechaModificacion descendente
  → Soporta búsqueda por parámetro q (string):
    Si q no es null: filtrar por Titulo.Contains(q)
  → Pasa los resultados a la vista

[GET] Download(int id)
  → [Authorize(Roles = "Operario")]
  → Igual que el Download del DocumentosController
  → Registra en Auditoria: Accion = "Descargó" con UsuarioId del operario

════════════════════════════════════════
Views/Consulta/Index.cshtml
════════════════════════════════════════
Crear esta vista nueva (diseño consistente con el _Layout.cshtml existente):

- Título: "Documentos Vigentes"
- Barra de búsqueda simple (form GET con campo q)
- Tabla con columnas: Título | Versión | Fecha Aprobación | Extensión | Acción
- Columna Acción: botón "Descargar" que llame a Download(id)
- Si no hay documentos aprobados: mensaje "No hay documentos vigentes disponibles"
- Enlace al módulo PHP: "Ver portal de consulta pública →" 
  (la URL viene de appsettings: "ModuloConsulta:Url")
- Diseño: usar las mismas clases CSS ya definidas en site.css (cards, tabla, badges)

En appsettings.json agregar:
"ModuloConsulta": {
  "Url": "http://localhost:8080"
}

RESTRICCIONES:
- El diseño debe ser consistente con el _Layout.cshtml existente
- NO permitir acceso a Admin ni Revisor a este controlador
- El enlace al módulo PHP solo es un <a href>, no requiere autenticación especial