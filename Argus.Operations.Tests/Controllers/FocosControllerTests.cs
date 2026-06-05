using Argus.Operations.API.Controllers;
using Argus.Operations.Application.Integration;
using Argus.Operations.Tests.Integration;
using Microsoft.AspNetCore.Mvc;

namespace Argus.Operations.Tests.Controllers;

public class FocosControllerTests
{
    private static (FocosController controller, FakeFocoCalorJavaClient fake) BuildController()
    {
        var fake = new FakeFocoCalorJavaClient();
        var controller = new FocosController(fake);
        return (controller, fake);
    }

    private static FocoCalorDto FocoFake(long id) => new(
        Id: id,
        Latitude: -15.5,
        Longitude: -47.5,
        Frp: 23.4,
        TemperaturaEstimada: 320.5,
        Confianca: "ALTA",
        Satelite: "AQUA",
        Sensor: "MODIS",
        OrigemDado: "NASA FIRMS",
        DataHora: new DateTime(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc),
        Status: "ATIVO",
        PayloadJson: null,
        RegiaoId: 1
    );

    [Fact]
    public async Task GetAll_RetornaListaDoJava()
    {
        var (controller, fake) = BuildController();
        fake.FocosParaListar = new List<FocoCalorDto> { FocoFake(1), FocoFake(2), FocoFake(3) };

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<IEnumerable<FocoCalorDto>>(ok.Value);
        Assert.Equal(3, lista.Count());
    }

    [Fact]
    public async Task GetAll_QuandoJavaRetornaVazio_RetornaListaVazia()
    {
        var (controller, _) = BuildController();
        // FocosParaListar já é uma lista vazia por default

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<IEnumerable<FocoCalorDto>>(ok.Value);
        Assert.Empty(lista);
    }
}
