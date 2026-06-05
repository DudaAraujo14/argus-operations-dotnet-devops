using System.Net.Http.Json;
using Argus.Operations.Application.Integration;

namespace Argus.Operations.Infrastructure.Integration;

// Client HTTP tipado pra API Java (endpoint /api/focos). A BaseAddress e o
// timeout são configurados na registração do HttpClient no Program.cs
// (vindos de JavaApi:BaseUrl, mesmos do AlertaJavaClient).
public class FocoCalorJavaClient : IFocoCalorJavaClient
{
    private readonly HttpClient _http;

    public FocoCalorJavaClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<FocoCalorDto>> ListarAsync(CancellationToken ct = default)
    {
        var focos = await _http.GetFromJsonAsync<List<FocoCalorDto>>("/api/focos", ct);
        return focos ?? [];
    }
}
