using Argus.Operations.Domain.Entities;

namespace Argus.Operations.Application.Auth;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
}
