namespace Prueba3._0.Helpers;

public static class EstadoHelper
{
    public static string ToBadgeClass(string estado) => estado switch
    {
        "Borrador"   => "badge-borrador",
        "EnRevision" => "badge-en-revision",
        "Aprobado"   => "badge-aprobado",
        "Obsoleto"   => "badge-obsoleto",
        _            => "badge-borrador"
    };

    public static string ToLabel(string estado) => estado switch
    {
        "EnRevision" => "En Revisión",
        _            => estado
    };

    public static string Normalizar(string raw) => raw.ToLower() switch
    {
        "borrador"   => "Borrador",
        "revision"
        or "enrevision" => "EnRevision",
        "aprobado"   => "Aprobado",
        "obsoleto"   => "Obsoleto",
        _            => "Borrador"
    };

    public static string Iniciales(string nombre)
    {
        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(partes.Take(2).Select(p => char.ToUpper(p[0])));
    }
}
