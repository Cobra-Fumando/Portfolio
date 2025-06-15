using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Portfolio.Config
{
    public class Token
    {
        private readonly string? _SecretKey;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly ILogger<Token> logger;
        private readonly IConfiguration configure;
        public Token(IConfiguration configuration, ILogger<Token> logger)
        {
            _SecretKey = configuration["Token:Key"];
            _issuer = configuration["Token:Issuer"];
            _audience = configuration["Token:Audience"];
            configure = configuration;
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
                new Claim(ClaimTypes.Role, "Usuarios"),
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

        public string GenerateTokenAdmin(string Nome)
        {
            string? KeyA = configure["TokenAdmin:Key"];
            string? IssuerA = configure["TokenAdmin:Issuer"];
            string? AudienceA = configure["TokenAdmin:Audience"];

            if (string.IsNullOrWhiteSpace(KeyA) || string.IsNullOrWhiteSpace(AudienceA) || string.IsNullOrWhiteSpace(IssuerA))
            {
                logger.LogError("Está faltando informações");
                return "Erro: está faltando informações";
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KeyA));
            var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, Nome),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: IssuerA,
                audience: AudienceA,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credential

            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32]; //32 bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber); //preenche o array de bytes com bytes aleatorio
            }

            return Convert.ToBase64String(randomNumber); //converte o bytes para string64
        }
    }
}
