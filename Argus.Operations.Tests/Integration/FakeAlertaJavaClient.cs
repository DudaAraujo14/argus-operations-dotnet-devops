using Argus.Operations.Application.Integration;

namespace Argus.Operations.Tests.Integration;

// Fake controlado pelo teste: configura a lista e o mapa de id→alerta antes
// do Act. Sem Moq pra não adicionar dep — pattern já usado no resto da suíte.
public class FakeAlertaJavaClient : IAlertaJavaClient
{
    public List<AlertaDto> AlertasParaListar { get; set; } = new();
    public Dictionary<long, AlertaDto> AlertasPorId { get; set; } = new();
    public Exception? ExcecaoAoBuscar { get; set; }

    public Task<AlertaDto?> BuscarPorIdAsync(long id, CancellationToken ct = default)
    {
        if (ExcecaoAoBuscar != null)
            throw ExcecaoAoBuscar;
        AlertasPorId.TryGetValue(id, out var alerta);
        return Task.FromResult<AlertaDto?>(alerta);
    }

    public Task<IReadOnlyList<AlertaDto>> ListarAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<AlertaDto>>(AlertasParaListar);
    }
}
