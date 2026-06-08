using System.IdentityModel.Tokens.Jwt;
using Argus.Operations.Application.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Argus.Operations.Infrastructure.Integration;

// Mantém em memória o JWT que a API Java emite e renova quando vai expirar.
// Singleton no DI — todas as chamadas pro Java compartilham o mesmo cache
// pra não fazer login a cada request.
//
// Thread-safe via SemaphoreSlim: se duas requests precisarem de token ao
// mesmo tempo e o cache estiver vazio/expirado, só uma faz o login; a outra
// espera e reusa o token recém-emitido.
public class JavaTokenProvider
{
    private readonly IJavaAuthClient _authClient;
    private readonly ILogger<JavaTokenProvider> _logger;
    private readonly string _email;
    private readonly string _senha;

    private string? _cachedToken;
    // Renova um pouco antes do exp pra não enviar token "quase expirado"
    // numa request lenta que possa estourar o exp no meio.
    private DateTime _renewAt = DateTime.MinValue;
    private static readonly TimeSpan RenewBuffer = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _lock = new(1, 1);

    public JavaTokenProvider(
        IJavaAuthClient authClient,
        IConfiguration configuration,
        ILogger<JavaTokenProvider> logger)
    {
        _authClient = authClient;
        _logger = logger;
        _email = configuration["JavaApi:AdminEmail"]
            ?? throw new InvalidOperationException(
                "JavaApi:AdminEmail não configurado — necessário pra autenticar contra a API Java.");
        _senha = configuration["JavaApi:AdminSenha"]
            ?? throw new InvalidOperationException(
                "JavaApi:AdminSenha não configurada — use user-secrets em dev ou Application Settings em prod.");
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        // Caminho rápido sem lock: token válido em cache.
        if (_cachedToken is not null && DateTime.UtcNow < _renewAt)
            return _cachedToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check depois do lock — outra thread pode ter renovado enquanto esperávamos.
            if (_cachedToken is not null && DateTime.UtcNow < _renewAt)
                return _cachedToken;

            _logger.LogInformation("Solicitando novo token na API Java (cache vazio ou prestes a expirar).");
            var response = await _authClient.LoginAsync(new JavaLoginRequest(_email, _senha), ct);

            _cachedToken = response.Token;
            _renewAt = ExtractExpiry(response.Token) - RenewBuffer;
            _logger.LogInformation("Token Java cacheado. Renovação agendada pra {RenewAt:o}.", _renewAt);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Invalida o cache pra forçar novo login. Chamado pelo DelegatingHandler
    // quando uma request retorna 401 (token revogado/inválido antes do exp).
    public void Invalidate()
    {
        _cachedToken = null;
        _renewAt = DateTime.MinValue;
    }

    // Lê o claim 'exp' (segundos desde epoch UTC) do JWT pra saber quando
    // renovar. Não valida a assinatura — só queremos o exp, e a validação
    // já aconteceu no Java emitir o token.
    private static DateTime ExtractExpiry(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.ValidTo; // já vem em UTC
    }
}
