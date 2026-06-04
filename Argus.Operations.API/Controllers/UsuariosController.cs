using Argus.Operations.API.Auth;
using Argus.Operations.API.DTOs.Usuarios;
using Argus.Operations.Application.Auth;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Argus.Operations.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsuariosController : ControllerBase
{
    private readonly ArgusDbContext _context;
    private readonly IPasswordHasher _hasher;

    public UsuariosController(ArgusDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    // GET /api/usuarios → lista todos os usuários (sem SenhaHash no retorno)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDetalheResponse>>> GetAll()
    {
        var usuarios = await _context.Usuarios.ToListAsync();
        return Ok(usuarios.Select(ToResponse));
    }

    // GET /api/usuarios/7 → busca um usuário pelo Id
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDetalheResponse>> GetById(long id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return NotFound();

        return Ok(ToResponse(usuario));
    }

    // POST /api/usuarios → cria um usuário novo. Recebe senha pura, hasheia
    // aqui via IPasswordHasher — cliente nunca manda hash, e a resposta nunca
    // o devolve. Email duplicado → 409 (também coberto pela constraint UNIQUE
    // no banco via GlobalExceptionHandler, mas validamos antes pra mensagem clara).
    [HttpPost]
    public async Task<ActionResult<UsuarioDetalheResponse>> Create(CreateUsuarioRequest request)
    {
        var emailJaExiste = await _context.Usuarios.AnyAsync(u => u.Email == request.Email);
        if (emailJaExiste)
            return Conflict("Já existe um usuário com este email.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Telefone = request.Telefone,
            NomeEmergencia = request.NomeEmergencia,
            TelefoneEmergencia = request.TelefoneEmergencia,
            RelacaoEmergencia = request.RelacaoEmergencia,
            SenhaHash = _hasher.Hash(request.Senha),
            Perfil = request.Perfil,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = usuario.Id },
            ToResponse(usuario)
        );
    }

    // PUT /api/usuarios/7 → atualiza um usuário existente. Não troca senha
    // (fluxo dedicado fica pra entrega futura) e não permite mexer em SenhaHash,
    // DataCriacao ou UltimoLogin — campos internos sob controle do servidor.
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, UpdateUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound();

        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        usuario.Telefone = request.Telefone;
        usuario.NomeEmergencia = request.NomeEmergencia;
        usuario.TelefoneEmergencia = request.TelefoneEmergencia;
        usuario.RelacaoEmergencia = request.RelacaoEmergencia;
        usuario.Perfil = request.Perfil;
        usuario.Ativo = request.Ativo;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/usuarios/7 → remove um usuário
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound();

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static UsuarioDetalheResponse ToResponse(Usuario u) => new(
        u.Id,
        u.Nome,
        u.Email,
        u.Telefone,
        u.NomeEmergencia,
        u.TelefoneEmergencia,
        u.RelacaoEmergencia,
        u.Perfil,
        u.Ativo,
        u.DataCriacao,
        u.UltimoLogin
    );
}
