using Argus.Operations.Infrastructure.Auth;

namespace Argus.Operations.Tests.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ComSenhaValida_RetornaHashNaoVazio()
    {
        // Arrange
        var senha = "MinhaSenha@123";

        // Act
        var hash = _sut.Hash(senha);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual(senha, hash);
    }

    [Fact]
    public void Hash_ChamadoDuasVezesComMesmaSenha_GeraHashesDiferentes()
    {
        // Arrange
        var senha = "MinhaSenha@123";

        // Act
        var hash1 = _sut.Hash(senha);
        var hash2 = _sut.Hash(senha);

        // Assert
        // BCrypt usa salt aleatório a cada hash, então mesmo a mesma senha gera hashes distintos
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ComSenhaCorreta_RetornaTrue()
    {
        // Arrange
        var senha = "MinhaSenha@123";
        var hash = _sut.Hash(senha);

        // Act
        var resultado = _sut.Verify(senha, hash);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void Verify_ComSenhaErrada_RetornaFalse()
    {
        // Arrange
        var senha = "MinhaSenha@123";
        var senhaErrada = "OutraSenha@456";
        var hash = _sut.Hash(senha);

        // Act
        var resultado = _sut.Verify(senhaErrada, hash);

        // Assert
        Assert.False(resultado);
    }
}
