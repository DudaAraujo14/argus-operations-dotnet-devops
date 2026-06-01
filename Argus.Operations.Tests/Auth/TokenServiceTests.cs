using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
using Argus.Operations.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Argus.Operations.Tests.Auth;

public class TokenServiceTests
{
    private static TokenService BuildSut()
    {
        var settings = new JwtSettings
        {
            Issuer = "ArgusOperationsAPI-Test",
            Audience = "ArgusOperationsClients-Test",
            Key = "chave-de-teste-com-pelo-menos-32-caracteres-pra-HmacSha256",
            ExpirationMinutes = 60
        };
        return new TokenService(Options.Create(settings));
    }

    private static Usuario BuildUsuario(PerfilUsuario perfil = PerfilUsuario.Brigadista) => new()
    {
        Id = 42,
        Nome = "Fulano de Tal",
        Email = "fulano@argus.com",
        SenhaHash = "hash-irrelevante-pro-token",
        Perfil = perfil,
        Ativo = true
    };

    [Fact]
    public void GenerateToken_ComUsuarioValido_RetornaJwtCom3Partes()
    {
        // Arrange
        var sut = BuildSut();
        var usuario = BuildUsuario();

        // Act
        var token = sut.GenerateToken(usuario);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));
        // JWT tem 3 partes separadas por ponto: header.payload.signature
        Assert.Equal(3, token.Split('.').Length);
    }

    [Fact]
    public void GenerateToken_IncluiClaimsBasicasDoUsuario()
    {
        // Arrange
        var sut = BuildSut();
        var usuario = BuildUsuario();

        // Act
        var token = sut.GenerateToken(usuario);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(usuario.Id.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(usuario.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(usuario.Nome, jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void GenerateToken_IncluiClaimDeRoleComPerfilDoUsuario()
    {
        // Arrange
        var sut = BuildSut();
        var usuario = BuildUsuario(PerfilUsuario.Coordenador);

        // Act
        var token = sut.GenerateToken(usuario);

        // Assert
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);
        Assert.Equal("Coordenador", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ChamadoDuasVezes_GeraJtisDiferentes()
    {
        // Arrange
        var sut = BuildSut();
        var usuario = BuildUsuario();

        // Act
        var token1 = sut.GenerateToken(usuario);
        var token2 = sut.GenerateToken(usuario);

        // Assert
        var jti1 = new JwtSecurityTokenHandler().ReadJwtToken(token1)
            .Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = new JwtSecurityTokenHandler().ReadJwtToken(token2)
            .Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        // JTI é único por token (anti-replay), nunca pode repetir
        Assert.NotEqual(jti1, jti2);
    }
}
