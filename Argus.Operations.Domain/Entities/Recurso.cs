namespace Argus.Operations.Domain.Entities;
using Argus.Operations.Domain.Enums;
public class Recurso
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoRecurso Tipo { get; set; }
    public bool Disponivel { get; set; } = true;

    // Relacionamento: a qual brigada este recurso pertence
    public long BrigadaId { get; set; }
    public Brigada? Brigada { get; set; }
}