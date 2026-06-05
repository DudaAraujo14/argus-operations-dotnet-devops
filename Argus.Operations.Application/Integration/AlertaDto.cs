namespace Argus.Operations.Application.Integration;

// Espelha o AlertaResponseDTO da API Java (argus-intelligence-api).
// Coordenadas (latitude/longitude) não estão aqui — vivem na entidade FocoCalor
// referenciada por FocoCalorId. Pra obter posição geográfica de um alerta é
// preciso consultar a API Java em /api/focos-calor/{focoCalorId} (não exposto
// como proxy ainda).
//
// Nivel:  BAIXO | MEDIO | ALTO | CRITICO
// Status: ABERTO | EM_ANALISE | ENCAMINHADO | ENCERRADO
public record AlertaDto(
    long Id,
    string Titulo,
    string? Descricao,
    string Nivel,
    string Status,
    double? ScoreRisco,
    string? RecomendacaoOperacional,
    DateTime DataGeracao,
    DateTime? DataAtualizacao,
    long FocoCalorId
);
