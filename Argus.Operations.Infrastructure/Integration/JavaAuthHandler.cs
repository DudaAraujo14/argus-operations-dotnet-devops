using System.Net;
using System.Net.Http.Headers;

namespace Argus.Operations.Infrastructure.Integration;

// DelegatingHandler que injeta Authorization: Bearer <token> em toda chamada
// pra API Java. Acoplado aos HttpClients tipados (AlertaJavaClient,
// FocoCalorJavaClient) via AddHttpMessageHandler no Program.cs.
//
// Se a request voltar 401 (token revogado/inválido antes do exp do JWT),
// invalida o cache e refaz a tentativa UMA vez. Sem retry infinito —
// se o segundo 401 vier, deixa propagar pro GlobalExceptionHandler.
public class JavaAuthHandler : DelegatingHandler
{
    private readonly JavaTokenProvider _tokenProvider;

    public JavaAuthHandler(JavaTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Token cacheado pode estar revogado mesmo dentro do exp.
            // Invalida + tenta de novo com token fresco.
            response.Dispose();
            _tokenProvider.Invalidate();

            var freshToken = await _tokenProvider.GetTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", freshToken);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
