using System.Text;
using System.Text.Json;
using Prueba3._0.Models;

namespace Prueba3._0.Services;

public class BusquedaApiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public BusquedaApiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool> IndexarDocumento(Documento doc)
    {
        var payload = new
        {
            id_documento = doc.Id.ToString(),
            titulo = doc.Titulo,
            etiquetas = new[] { doc.Extension, "calidad", "normativa" },
            extension = doc.Extension,
            tamanio_kb = doc.TamanioKb,
        };

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var urlBase = _config["BusquedaApi:Url"] ?? "http://localhost:8000";
            var response = await _http.PostAsync($"{urlBase}/api/v1/indexar", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Si el módulo de búsqueda no está disponible, no falla todo el sistema
            return false;
        }
    }
}
