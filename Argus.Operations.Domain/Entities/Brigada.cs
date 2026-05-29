namespace Argus.Operations.Domain.Entities;

public class Brigada
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string BaseOperacional { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
}