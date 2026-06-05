namespace Argus.Operations.Application.Integration;

public interface IFocoCalorJavaClient
{
    // Lista todos os focos de calor monitorados (proxy pra API Java/FIRMS).
    // Sem filtros: o cliente (mobile) recebe a lista crua e decide o que mostrar.
    // Lança HttpRequestException se Java estiver fora ou TaskCanceledException
    // se exceder o timeout — ambos viram 503/504 pelo GlobalExceptionHandler.
    Task<IReadOnlyList<FocoCalorDto>> ListarAsync(CancellationToken ct = default);
}
