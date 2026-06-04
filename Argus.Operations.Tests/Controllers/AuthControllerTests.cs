using Argus.Operations.API.Controllers;
using Argus.Operations.API.DTOs.Auth;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
using Argus.Operations.Infrastructure.Auth;
using Argus.Operations.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Argus.Operations.Tests.Controllers;

public class AuthControllerTests
{
    private const string CodigoConviteValido = "TEST-CONVITE-2026";

    private static (AuthController controller, ArgusDbContext db, BcryptPasswordHasher hasher) BuildController()
    {
        var options = new DbContextOptionsBuilder<ArgusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new ArgusDbContext(options);

        var jwtSettings = new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "chave-de-teste-com-pelo-menos-32-caracteres-pra-HmacSha256",
            ExpirationMinutes = 60
        };
        var tokenService = new TokenService(Options.Create(jwtSettings));
        var hasher = new BcryptPasswordHasher();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:CodigoConvite"] = CodigoConviteValido
            })
            .Build();

        var controller = new AuthController(db, tokenService, hasher, Options.Create(jwtSettings), config);
        return (controller, db, hasher);
    }

    private static async Task SeedUsuarioAsync(ArgusDbContext db, BcryptPasswordHasher hasher,
        string email, string senha, PerfilUsuario perfil = PerfilUsuario.Brigadista, bool ativo = true)
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = "Usuario Teste",
            Email = email,
            Telefone = "11900000000",
            SenhaHash = hasher.Hash(senha),
            Perfil = perfil,
            Ativo = ativo,
            DataCriacao = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    // ===================== LOGIN =====================

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaOkComToken()
    {
        // Arrange
        var (controller, db, hasher) = BuildController();
        await SeedUsuarioAsync(db, hasher, "alice@argus.com", "Senha@123");
        var request = new LoginRequest("alice@argus.com", "Senha@123");

        // Act
        var result = await controller.Login(request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(ok.Value);
        Assert.False(string.IsNullOrEmpty(response.Token));
        Assert.Equal("alice@argus.com", response.Usuario.Email);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_RetornaUnauthorized()
    {
        // Arrange
        var (controller, db, hasher) = BuildController();
        await SeedUsuarioAsync(db, hasher, "alice@argus.com", "Senha@123");
        var request = new LoginRequest("alice@argus.com", "SenhaErrada");

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_RetornaUnauthorized()
    {
        // Arrange
        var (controller, _, _) = BuildController();
        var request = new LoginRequest("naoexiste@argus.com", "QualquerSenha");

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_RetornaUnauthorized()
    {
        // Arrange
        var (controller, db, hasher) = BuildController();
        await SeedUsuarioAsync(db, hasher, "inativo@argus.com", "Senha@123", ativo: false);
        var request = new LoginRequest("inativo@argus.com", "Senha@123");

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    // ===================== REGISTER =====================

    [Fact]
    public async Task Register_ComContatoEmergenciaCompleto_SalvaTodosOsCampos()
    {
        // Arrange
        var (controller, db, _) = BuildController();
        var request = new RegisterRequest(
            Nome: "Novo Brigadista",
            Email: "novo@argus.com",
            Telefone: "11987654321",
            NomeEmergencia: "Joana da Silva",
            TelefoneEmergencia: "11988888888",
            RelacaoEmergencia: "Mãe",
            Senha: "Senha@123",
            CodigoConvite: CodigoConviteValido
        );

        // Act
        var result = await controller.Register(request);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(created.Value);
        Assert.Equal(PerfilUsuario.Brigadista, response.Usuario.Perfil);
        Assert.Equal("11987654321", response.Usuario.Telefone);
        Assert.Equal("Joana da Silva", response.Usuario.NomeEmergencia);
        Assert.Equal("11988888888", response.Usuario.TelefoneEmergencia);
        Assert.Equal("Mãe", response.Usuario.RelacaoEmergencia);

        var salvo = await db.Usuarios.FirstAsync(u => u.Email == "novo@argus.com");
        Assert.Equal("Joana da Silva", salvo.NomeEmergencia);
        Assert.Equal("11988888888", salvo.TelefoneEmergencia);
        Assert.Equal("Mãe", salvo.RelacaoEmergencia);
        Assert.True(salvo.Ativo);
    }

    [Fact]
    public async Task Register_SemContatoEmergencia_AceitaECriaSemOsCampos()
    {
        // Arrange
        var (controller, db, _) = BuildController();
        var request = new RegisterRequest(
            Nome: "Brigadista Sem Emergencia",
            Email: "sememerg@argus.com",
            Telefone: "11999990000",
            NomeEmergencia: null,
            TelefoneEmergencia: null,
            RelacaoEmergencia: null,
            Senha: "Senha@123",
            CodigoConvite: CodigoConviteValido
        );

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        var salvo = await db.Usuarios.FirstAsync(u => u.Email == "sememerg@argus.com");
        Assert.Equal("11999990000", salvo.Telefone);
        Assert.Null(salvo.NomeEmergencia);
        Assert.Null(salvo.TelefoneEmergencia);
        Assert.Null(salvo.RelacaoEmergencia);
    }

    [Fact]
    public async Task Register_ComCodigoConviteErrado_RetornaForbid()
    {
        // Arrange
        var (controller, db, _) = BuildController();
        var request = new RegisterRequest(
            Nome: "Tentativa Maliciosa",
            Email: "intruso@argus.com",
            Telefone: "11900000000",
            NomeEmergencia: null,
            TelefoneEmergencia: null,
            RelacaoEmergencia: null,
            Senha: "Senha@123",
            CodigoConvite: "CODIGO-ERRADO"
        );

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
        // Garante que NÃO criou o usuário
        Assert.False(await db.Usuarios.AnyAsync(u => u.Email == "intruso@argus.com"));
    }

    [Fact]
    public async Task Register_ComEmailJaExistente_RetornaConflict()
    {
        // Arrange
        var (controller, db, hasher) = BuildController();
        await SeedUsuarioAsync(db, hasher, "duplicado@argus.com", "Senha@123");
        var request = new RegisterRequest(
            Nome: "Outro Fulano",
            Email: "duplicado@argus.com",
            Telefone: "11900000000",
            NomeEmergencia: null,
            TelefoneEmergencia: null,
            RelacaoEmergencia: null,
            Senha: "Senha@456",
            CodigoConvite: CodigoConviteValido
        );

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<ConflictObjectResult>(result.Result);
    }
}
