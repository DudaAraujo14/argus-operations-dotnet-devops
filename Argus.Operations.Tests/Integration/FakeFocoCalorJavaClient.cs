using Argus.Operations.Application.Integration;

namespace Argus.Operations.Tests.Integration;

public class FakeFocoCalorJavaClient : IFocoCalorJavaClient
{
    public List<FocoCalorDto> FocosParaListar { get; set; } = new();

    public Task<IReadOnlyList<FocoCalorDto>> ListarAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<FocoCalorDto>>(FocosParaListar);
    }
}
