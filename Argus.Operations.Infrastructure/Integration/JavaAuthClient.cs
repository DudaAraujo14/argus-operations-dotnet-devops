using System.Net.Http.Json;
using System.Text.Json;
using Argus.Operations.Application.Integration;

namespace Argus.Operations.Infrastructure.Integration;

// Faz login na API Java em POST /api/auth/login e devolve o token.
// HttpClient próprio (NÃO compartilhado com os clients que usam DelegatingHandler)
// pra evitar recursão "preciso de token pra pedir token".
public class JavaAuthClient : IJavaAuthClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public JavaAuthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<JavaLoginResponse> LoginAsync(JavaLoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", request, JsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JavaLoginResponse>(JsonOpts, ct);
        return body ?? throw new InvalidOperationException("API Java retornou login vazio.");
    }
}
