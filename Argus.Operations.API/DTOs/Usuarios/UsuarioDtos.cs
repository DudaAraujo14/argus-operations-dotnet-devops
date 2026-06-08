using System.ComponentModel.DataAnnotations;
using Argus.Operations.Domain.Enums;

namespace Argus.Operations.API.DTOs.Usuarios;

// Payload pra criar usuário pelo /api/usuarios (Admin). Recebe senha PURA —
// o controller faz o hash via IPasswordHasher antes de gravar. Nunca aceitar
// hash vindo do cliente.
public record CreateUsuarioRequest(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MaxLength(20)] string Telefone,
    [MaxLength(100)] string? NomeEmergencia,
    [MaxLength(20)] string? TelefoneEmergencia,
    [MaxLength(30)] string? RelacaoEmergencia,
    [Required, MinLength(6)] string Senha,
    [Required] PerfilUsuario Perfil,
    // Opcional — vincula o Usuario a um Brigadista (entidade operacional).
    // Faz sentido só pra perfil Brigadista; pra Admin/Coordenador deixa null.
    long? BrigadistaId
);

// Payload de atualização. Não inclui senha (troca de senha tem fluxo próprio,
// fora do escopo dessa entrega) nem Id (vem da rota).
public record UpdateUsuarioRequest(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MaxLength(20)] string Telefone,
    [MaxLength(100)] string? NomeEmergencia,
    [MaxLength(20)] string? TelefoneEmergencia,
    [MaxLength(30)] string? RelacaoEmergencia,
    [Required] PerfilUsuario Perfil,
    [Required] bool Ativo,
    // Permite (re)vincular o Usuario a um Brigadista via PUT — esse é o caminho
    // pro coordenador linkar um usuário cadastrado a um membro de brigada.
    long? BrigadistaId
);

// Resposta pública do recurso — NUNCA inclui SenhaHash. Carrega Ativo,
// DataCriacao e UltimoLogin porque é a visão administrativa do usuário.
public record UsuarioDetalheResponse(
    long Id,
    string Nome,
    string Email,
    string Telefone,
    string? NomeEmergencia,
    string? TelefoneEmergencia,
    string? RelacaoEmergencia,
    PerfilUsuario Perfil,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? UltimoLogin,
    long? BrigadistaId
);
