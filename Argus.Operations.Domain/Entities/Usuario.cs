using Argus.Operations.Domain.Enums;

namespace Argus.Operations.Domain.Entities;

public class Usuario
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    // Contato pra acionar em caso de emergência durante operação em campo.
    // Os 3 campos andam juntos — ou todos preenchidos, ou nenhum. Opcionais
    // porque usuários administrativos (Admin/Coordenador de escritório) podem
    // não precisar registrar.
    public string? NomeEmergencia { get; set; }
    public string? TelefoneEmergencia { get; set; }
    public string? RelacaoEmergencia { get; set; }

    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoLogin { get; set; }

    // Liga o usuário à entidade operacional Brigadista correspondente.
    // Nullable porque:
    //   - Admin/Coordenador podem não atuar em campo (não viram brigadistas)
    //   - Um usuário pode existir antes de ser atribuído a uma brigada
    //   - Auto-cadastro via /api/auth/register não tenta linkar (ficaria
    //     adivinhando) — coordenador vincula depois via PUT /api/usuarios/{id}
    public long? BrigadistaId { get; set; }
    public Brigadista? Brigadista { get; set; }
}