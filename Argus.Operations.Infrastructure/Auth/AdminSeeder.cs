using Argus.Operations.Application.Auth;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
using Argus.Operations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;

namespace Argus.Operations.Infrastructure.Auth;

public static class AdminSeeder
{
    // FIAP Oracle limita 10 sessões/usuário e às vezes leva minutos pra reapear
    // sessões mortas. Em vez de crashar a API no startup, tentamos algumas vezes
    // com espera entre as tentativas — assim a API sobe sozinha quando liberar.
    private const int MaxAttempts = 3;
    private const int DelaySeconds = 10;
    private const int OraSessionsPerUserExceeded = 2391;

    public static async Task SeedAsync(IServiceProvider services)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await SeedOnceAsync(services);
                return;
            }
            catch (OracleException ex) when (ex.Number == OraSessionsPerUserExceeded && attempt < MaxAttempts)
            {
                Console.WriteLine($"[AdminSeeder] ORA-02391 (sessões esgotadas no FIAP). Tentativa {attempt}/{MaxAttempts} — aguardando {DelaySeconds}s antes de tentar de novo...");
                await Task.Delay(TimeSpan.FromSeconds(DelaySeconds));
            }
        }
    }

    private static async Task SeedOnceAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArgusDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var email = config["AdminSeed:Email"] ?? "admin@argus.com";
        var nome = config["AdminSeed:Nome"] ?? "Administrador Argus";
        var telefone = config["AdminSeed:Telefone"] ?? "11900000000";
        var senha = config["AdminSeed:Senha"] ?? "Admin@123";

        var jaExiste = await context.Usuarios.AnyAsync(u => u.Email == email);
        if (jaExiste) return;

        context.Usuarios.Add(new Usuario
        {
            Nome = nome,
            Email = email,
            Telefone = telefone,
            SenhaHash = hasher.Hash(senha),
            Perfil = PerfilUsuario.Admin,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
