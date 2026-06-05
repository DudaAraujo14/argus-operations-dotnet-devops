namespace Argus.Operations.Application.Integration;

// Espelha o FocoCalorResponseDTO da API Java (argus-intelligence-api).
// Representa um foco de calor detectado por satélite (origem NASA FIRMS).
//
// Campos relevantes pro mobile:
//   Latitude/Longitude → plotar no mapa
//   Frp                → Fire Radiative Power (intensidade do foco em MW)
//   TemperaturaEstimada → temperatura aparente em K
//   Confianca          → "ALTA" | "MEDIA" | "BAIXA" (filtrar ruído)
//   DataHora           → quando o satélite detectou (filtrar "ativos")
//   Status             → estado operacional do foco
public record FocoCalorDto(
    long Id,
    double Latitude,
    double Longitude,
    double? Frp,
    double? TemperaturaEstimada,
    string? Confianca,
    string? Satelite,
    string? Sensor,
    string? OrigemDado,
    DateTime DataHora,
    string? Status,
    string? PayloadJson,
    long? RegiaoId
);
