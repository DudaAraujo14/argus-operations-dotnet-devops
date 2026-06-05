using Argus.Operations.API.Controllers;
using Argus.Operations.API.DTOs.Ocorrencias;
using Argus.Operations.Application.Integration;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Infrastructure.Data;
using Argus.Operations.Tests.Integration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Argus.Operations.Tests.Controllers;

public class AlertasControllerTests
{
    private static (AlertasController controller, ArgusDbContext db, FakeAlertaJavaClient fakeJava) BuildController()
    {
        var options = new DbContextOptionsBuilder<ArgusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new ArgusDbContext(options);
        var fakeJava = new FakeAlertaJavaClient();
        var controller = new AlertasController(fakeJava, db);
        return (controller, db, fakeJava);
    }

    private static AlertaDto AlertaFake(long id = 1, string nivel = "ALTO") => new(
        Id: id,
        Titulo: "Alerta no Pantanal",
        Descricao: "Foco com alta intensidade térmica",
        Nivel: nivel,
        Status: "ABERTO",
        ScoreRisco: 88.5,
        RecomendacaoOperacional: "Acionar brigada regional",
        DataGeracao: new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc),
        DataAtualizacao: null,
        FocoCalorId: 10
    );

    private static async Task<(long brigadaId, long brigadistaId)> SeedBrigadaEBrigadistaAsync(ArgusDbContext db)
    {
        var brigada = new Brigada { Nome = "Brigada Teste", BaseOperacional = "Cuiabá", Telefone = "65900000000", Ativa = true };
        db.Brigadas.Add(brigada);
        await db.SaveChangesAsync();

        var brigadista = new Brigadista
        {
            Nome = "Brigadista Teste",
            Matricula = "BR-001",
            Email = "br@argus.com",
            Telefone = "11900000000",
            Funcao = "Combatente",
            Ativo = true,
            DataAdmissao = DateTime.UtcNow,
            BrigadaId = brigada.Id
        };
        db.Brigadistas.Add(brigadista);
        await db.SaveChangesAsync();

        return (brigada.Id, brigadista.Id);
    }

    // ===================== GET /api/alertas =====================

    [Fact]
    public async Task GetAll_RetornaListaDoJava()
    {
        // Arrange
        var (controller, _, fake) = BuildController();
        fake.AlertasParaListar = new List<AlertaDto> { AlertaFake(1), AlertaFake(2) };

        // Act
        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var lista = Assert.IsAssignableFrom<IEnumerable<AlertaDto>>(ok.Value);
        Assert.Equal(2, lista.Count());
    }

    // ===================== GET /api/alertas/{id} =====================

    [Fact]
    public async Task GetById_QuandoExiste_RetornaOk()
    {
        var (controller, _, fake) = BuildController();
        fake.AlertasPorId[7] = AlertaFake(7);

        var result = await controller.GetById(7, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var alerta = Assert.IsType<AlertaDto>(ok.Value);
        Assert.Equal(7, alerta.Id);
    }

    [Fact]
    public async Task GetById_QuandoNaoExiste_RetornaNotFound()
    {
        var (controller, _, _) = BuildController();

        var result = await controller.GetById(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ===================== POST /api/alertas/{id}/criar-ocorrencia =====================

    [Fact]
    public async Task CriarOcorrencia_ComDadosValidos_CriaERetornaCreated()
    {
        // Arrange
        var (controller, db, fake) = BuildController();
        fake.AlertasPorId[42] = AlertaFake(42);
        var (brigadaId, brigadistaId) = await SeedBrigadaEBrigadistaAsync(db);
        var request = new CriarOcorrenciaDeAlertaRequest(
            BrigadaId: brigadaId,
            BrigadistaId: brigadistaId,
            Latitude: -16.5,
            Longitude: -56.5,
            Descricao: null
        );

        // Act
        var result = await controller.CriarOcorrencia(42, request, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var ocorrencia = Assert.IsType<Ocorrencia>(created.Value);
        Assert.Equal(42, ocorrencia.AlertaId);
        Assert.Equal(brigadaId, ocorrencia.BrigadaId);
        Assert.Equal(brigadistaId, ocorrencia.BrigadistaId);
        Assert.Equal(-16.5, ocorrencia.Latitude);
        // Como veio sem descrição, deve ter herdado do alerta (título + descrição + recomendação)
        Assert.Contains("Alerta no Pantanal", ocorrencia.Descricao);
        Assert.Contains("Acionar brigada regional", ocorrencia.Descricao);

        // Persistiu no banco
        Assert.Equal(1, await db.Ocorrencias.CountAsync());
    }

    [Fact]
    public async Task CriarOcorrencia_ComDescricaoCustomizada_NaoSobrescreve()
    {
        var (controller, db, fake) = BuildController();
        fake.AlertasPorId[42] = AlertaFake(42);
        var (brigadaId, brigadistaId) = await SeedBrigadaEBrigadistaAsync(db);
        var request = new CriarOcorrenciaDeAlertaRequest(
            brigadaId, brigadistaId, -16.5, -56.5,
            Descricao: "Descrição personalizada do coordenador"
        );

        var result = await controller.CriarOcorrencia(42, request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var ocorrencia = Assert.IsType<Ocorrencia>(created.Value);
        Assert.Equal("Descrição personalizada do coordenador", ocorrencia.Descricao);
    }

    [Fact]
    public async Task CriarOcorrencia_QuandoAlertaNaoExisteNoJava_RetornaNotFound()
    {
        var (controller, db, _) = BuildController();
        // Fake não tem o alerta 999 mapeado → devolve null
        var (brigadaId, brigadistaId) = await SeedBrigadaEBrigadistaAsync(db);
        var request = new CriarOcorrenciaDeAlertaRequest(brigadaId, brigadistaId, -16.5, -56.5, null);

        var result = await controller.CriarOcorrencia(999, request, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(0, await db.Ocorrencias.CountAsync());
    }

    [Fact]
    public async Task CriarOcorrencia_ComBrigadaInexistente_RetornaBadRequest()
    {
        var (controller, db, fake) = BuildController();
        fake.AlertasPorId[42] = AlertaFake(42);
        var (_, brigadistaId) = await SeedBrigadaEBrigadistaAsync(db);
        var request = new CriarOcorrenciaDeAlertaRequest(
            BrigadaId: 9999,  // não existe
            BrigadistaId: brigadistaId,
            Latitude: -16.5, Longitude: -56.5, Descricao: null
        );

        var result = await controller.CriarOcorrencia(42, request, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Brigada 9999", bad.Value?.ToString());
        Assert.Equal(0, await db.Ocorrencias.CountAsync());
    }

    [Fact]
    public async Task CriarOcorrencia_ComBrigadistaInexistente_RetornaBadRequest()
    {
        var (controller, db, fake) = BuildController();
        fake.AlertasPorId[42] = AlertaFake(42);
        var (brigadaId, _) = await SeedBrigadaEBrigadistaAsync(db);
        var request = new CriarOcorrenciaDeAlertaRequest(
            BrigadaId: brigadaId,
            BrigadistaId: 8888,  // não existe
            Latitude: -16.5, Longitude: -56.5, Descricao: null
        );

        var result = await controller.CriarOcorrencia(42, request, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Brigadista 8888", bad.Value?.ToString());
        Assert.Equal(0, await db.Ocorrencias.CountAsync());
    }
}
