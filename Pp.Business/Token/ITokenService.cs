using Pp.Data.Domain;
using Pp.Schema;

namespace Pp.Business.Token
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<JwtResponse> ValidateTokenAsync(string token);
    }
}
