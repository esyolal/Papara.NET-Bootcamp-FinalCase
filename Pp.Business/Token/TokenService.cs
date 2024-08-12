using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pp.Base.Token;
using Pp.Data.Domain;
using Pp.Schema;

namespace Pp.Business.Token
{
    public class TokenService : ITokenService
    {
        private readonly JwtConfig jwtConfig;

        public TokenService(JwtConfig jwtConfig)
        {
            this.jwtConfig = jwtConfig;
        }

        public Task<string> GenerateTokenAsync(User user)
        {
            var claims = GetClaims(user);
            var secret = Encoding.ASCII.GetBytes(jwtConfig.Secret);

            var jwtToken = new JwtSecurityToken(
                jwtConfig.Issuer,
                jwtConfig.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(jwtConfig.AccessTokenExpiration),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(jwtToken);

            return Task.FromResult(token);
        }

        public async Task<JwtResponse> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = ValidateToken(token, tokenHandler, out var securityToken);
                var jwtToken = (JwtSecurityToken)securityToken;

                var jwtResponse = new JwtResponse
                {
                    IsValid = true,
                    UserId = principal.FindFirst("UserId")?.Value,
                    Name = principal.FindFirst(ClaimTypes.Name)?.Value,
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                    Role = principal.FindFirst(ClaimTypes.Role)?.Value
                };

                return jwtResponse;
            }
            catch
            {
                return new JwtResponse { IsValid = false };
            }
        }

        private Claim[] GetClaims(User user)
        {
            return new[]
            {
                new Claim("UserName", user.Name),
                new Claim("UserId", user.Id.ToString()),
                new Claim("Role", user.Role),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
            };
        }

        private ClaimsPrincipal ValidateToken(string token, JwtSecurityTokenHandler tokenHandler, out SecurityToken validatedToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig.Secret))
            };

            return tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        }
    }
}
