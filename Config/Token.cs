using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Portfolio.Config
{
    public class Token
    {
        private readonly string? _SecretKey;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly ILogger<Token> logger;
        public Token(IConfiguration configuration, ILogger<Token> logger)
        {
            _SecretKey = configuration["Token:Key"];
            _issuer = configuration["Token:Issuer"];
            _audience = configuration["Token:Audience"];
            this.logger = logger;
        }
        public string GenerateToken(string Nome)
        {
            if(string.IsNullOrWhiteSpace(_SecretKey) || string.IsNullOrWhiteSpace(_audience) || string.IsNullOrWhiteSpace(_issuer))
            {
                logger.LogError("Está faltando informações");
                return "Erro: está faltando informações";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_SecretKey));
            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, Nome),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credential

            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32]; //256 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber); //preenche o array com bytes
            }

            return Convert.ToBase64String(randomNumber); //converte o bytes para string64
        }
    }
}
