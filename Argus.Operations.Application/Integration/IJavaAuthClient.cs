namespace Argus.Operations.Application.Integration;

// Client de autenticação contra a API Java. Separado dos outros clients
// (IAlertaJavaClient, IFocoCalorJavaClient) porque essa chamada NÃO pode
// passar pelo DelegatingHandler que injeta o Bearer token — senão entra em
// recursão (precisaria de token pra pedir token).
public interface IJavaAuthClient
{
    Task<JavaLoginResponse> LoginAsync(JavaLoginRequest request, CancellationToken ct = default);
}
