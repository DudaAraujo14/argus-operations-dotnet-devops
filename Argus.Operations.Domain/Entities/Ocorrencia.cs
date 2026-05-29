using Argus.Operations.Domain.Enums;

namespace Argus.Operations.Domain.Entities;

public class Ocorrencia
{
    public long Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public StatusOcorrencia Status { get; set; } = StatusOcorrencia.Aberta;
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFinalizacao { get; set; }

  // Relacionamento: qual brigadista é responsável (interno .NET)
    public long BrigadistaId { get; set; }
    public Brigadista? Brigadista { get; set; }

    // Relacionamento: qual brigada está atendendo (interno .NET)
    public long BrigadaId { get; set; }
    public Brigada? Brigada { get; set; }

    // Relacionamento cross-domain: alerta de origem (do Java)
    // Nullable porque a ocorrência pode ser criada manualmente sem alerta
    // FK formal será adicionada via ALTER TABLE no script de banco consolidado
    public long? AlertaId { get; set; }
}