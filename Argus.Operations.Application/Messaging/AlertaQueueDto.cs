namespace Argus.Operations.Application.Messaging;

// Mensagem publicada pela API Java (Intelligence) na fila 'argus.alertas' do
// CloudAMQP. Já vem enriquecida com dados do foco e da região — o consumer
// não faz HTTP extra pra completar. O Java publica só ALTO+CRITICO pra evitar
// virar spam de ocorrência.
//
// Campos vindos do AlertaResponseDTO + FocoCalorResponseDTO + Regiao do Java.
public record AlertaQueueDto(
    long Id,
    string Titulo,
    string? Descricao,
    string Nivel,                       // ALTO | CRITICO
    string Status,
    double? ScoreRisco,
    string? RecomendacaoOperacional,
    DateTime DataGeracao,
    long FocoCalorId,
    double Latitude,
    double Longitude,
    double? Frp,
    double? TemperaturaEstimada,
    string? Confianca,
    string? Satelite,
    string? Sensor,
    long RegiaoId,
    string? RegiaoNome,
    string? NivelRiscoRegiao
);
