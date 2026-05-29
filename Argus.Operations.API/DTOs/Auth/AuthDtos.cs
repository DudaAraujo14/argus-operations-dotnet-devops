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
    PerfilUsuario Perfil
);
