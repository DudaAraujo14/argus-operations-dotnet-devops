using System.ComponentModel.DataAnnotations;
using Argus.Operations.Domain.Enums;

namespace Argus.Operations.API.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha
);

public record RegisterRequest(
    [Required, MaxLength(150)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MaxLength(20)] string Telefone,
    // Trio de campos do contato de emergência — andam juntos no mobile,
    // todos opcionais aqui pra acomodar usuários administrativos.
    [MaxLength(100)] string? NomeEmergencia,
    [MaxLength(20)] string? TelefoneEmergencia,
    [MaxLength(30)] string? RelacaoEmergencia,
    [Required, MinLength(6)] string Senha,
    [Required] string CodigoConvite
);

public record AuthResponse(
    string Token,
    DateTime ExpiraEm,
    UsuarioResponse Usuario
);

public record UsuarioResponse(
    long Id,
    string Nome,
    string Email,
    string Telefone,
    string? NomeEmergencia,
    string? TelefoneEmergencia,
    string? RelacaoEmergencia,
    PerfilUsuario Perfil
);

// Payload do PUT /api/auth/me — usuário logado editando o próprio perfil.
// Cobre só campos "seguros" (não muda Perfil, Ativo, Email, SenhaHash).
public record AtualizarPerfilRequest(
    [Required, MaxLength(150)] string Nome,
    [Required, MaxLength(20)] string Telefone,
    [MaxLength(100)] string? NomeEmergencia,
    [MaxLength(20)] string? TelefoneEmergencia,
    [MaxLength(30)] string? RelacaoEmergencia
);
