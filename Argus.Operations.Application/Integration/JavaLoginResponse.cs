namespace Argus.Operations.Application.Integration;

// Resposta de POST /api/auth/login do Java. Atualmente devolve só o token
// JWT — duração é lida do claim 'exp' do próprio token, não vem como campo
// separado (diferente do nosso AuthResponse que tem 'expiraEm' explícito).
public record JavaLoginResponse(string Token);
