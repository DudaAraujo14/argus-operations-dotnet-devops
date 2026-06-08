namespace Argus.Operations.Application.Integration;

// Payload de POST /api/auth/login do Java (Intelligence API).
// Mesmos campos que o nosso login — convenção do projeto.
public record JavaLoginRequest(string Email, string Senha);
