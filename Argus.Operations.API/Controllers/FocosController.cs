using Argus.Operations.Application.Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Argus.Operations.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FocosController : ControllerBase
{
    private readonly IFocoCalorJavaClient _javaClient;

    public FocosController(IFocoCalorJavaClient javaClient)
    {
        _javaClient = javaClient;
    }

    // GET /api/focos → lista focos de calor (proxy pra API Java/FIRMS).
    // Pass-through: devolve o que o Java retorna, sem filtragem/transformação.
    // Mobile usa pra renderizar mapa com pontos de calor ativos.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FocoCalorDto>>> GetAll(CancellationToken ct)
    {
        var focos = await _javaClient.ListarAsync(ct);
        return Ok(focos);
    }
}
