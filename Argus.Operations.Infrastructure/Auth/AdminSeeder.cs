using Argus.Operations.Application.Auth;
using Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
using Argus.Operations.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Argus.Operations.Infrastructure.Auth;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
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
