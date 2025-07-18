using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Config;
using Portfolio.Interfaces;
using Portfolio.Tabelas;
using Portfolio.Dto;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Portfolio.Classes
{
    public class Users : IUsers
    {
        private readonly ILogger<Users> logger;
        private readonly AppDbContext Context;
        private readonly Token token;
        private readonly Hash hasher;
        private readonly IHttpContextAccessor responseCookies;
        private readonly EmailSmtp emailSmtp;
        private readonly ObterIp obterIp;
        public Users(ILogger<Users> logger, AppDbContext context, Hash hasher, Token token, IHttpContextAccessor responseCookies, EmailSmtp email, ObterIp obterIp)
        {
            this.logger = logger;
            this.Context = context;
            this.hasher = hasher;
            this.token = token;
            this.responseCookies = responseCookies;
            emailSmtp = email;
            this.obterIp = obterIp;
        }

        private async Task<TabelaProblem<bool>> ValidarToken(string Email)
        {
            var log = new TabelaProblem<bool>();

            try
            {
                var TokenUser = await Context.TokenValidation.AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Email == Email).ConfigureAwait(false);

                if (TokenUser == null)
                {
                    log.success = false;
                    log.Message = "Token Não encontrado";
                    log.Dados = false;
                    return log;
                }

                log.success = true;
                log.Message = "Token Encontrado";
                log.Dados = TokenUser.DataExpiracao < DateTime.UtcNow;
                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }

        }

        public async Task<TabelaProblem<UsuariosDto>> add(Usuarios usuarios, string? Permission)
        {
            var log = new TabelaProblem<UsuariosDto>
            {
                Dados = null
            };

            var validar = new ValidacaoEmail();

            if (usuarios == null)
            {
                log.success = false;
                log.Message = "Preencha usuarios";
                return log;
            }

            var UserIp = obterIp.ValidarIp();

            var valido = validar.EmailValido(usuarios.Email);

            if (!valido)
            {
                log.success = false;
                log.Message = "Email invalido";
                return log;
            }

            var existe = await Context.Usuarios.AsNoTracking()
                        .AnyAsync(p => p.Email == usuarios.Email)
                        .ConfigureAwait(false);

            if (existe)
            {
                log.success = false;
                log.Message = "Esse Email já existe";
                return log;
            }

            var Hash = hasher.Hashar(usuarios.Password);

            string role = "Usuarios";
            var user = responseCookies.HttpContext?.User;

            if (user != null)
            {
                if (string.Equals(user.FindFirst(ClaimTypes.Role)?.Value, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(Permission))
                    {
                        Permission = "Usuarios";
                    }

                    var rolesValidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "admin", "Usuarios" };
                    if (!rolesValidos.Contains(Permission))
                    {
                        log.success = false;
                        log.Message = "Role inválida";
                        return log;
                    }

                    role = Permission;
                }
            }

            var Novo = new Usuarios
            {
                Email = usuarios.Email,
                Password = Hash,
                Name = usuarios.Name,
                Role = role,
                Ip = UserIp
            };

            try
            {

                await Context.AddAsync(Novo).ConfigureAwait(false);
                await Context.SaveChangesAsync().ConfigureAwait(false);

                var UsuariosDtos = new UsuariosDto
                {
                    Email = usuarios.Email,
                    Name = usuarios.Name
                };

                log.Message = "Usuario adicionado com sucesso";
                log.Dados = UsuariosDtos;

                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                logger.LogError($"Erro inesperado: {ex.Message}");

                return log;
            }
        }

        public async Task<TabelaProblem<List<string>>> Logar(string Email, string Senha)
        {
            var log = new TabelaProblem<List<string>>
            {
                Dados = new List<string>()
            };

            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
                {
                    log.success = false;
                    log.Message = "Email e Senha não podem estar vazio";
                    return log;
                }

                var SenhaHash = hasher.Hashar(Senha);

                var pessoa = await Context.Usuarios.AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Email == Email && p.Password == SenhaHash)
                            .ConfigureAwait(false);

                if (pessoa == null)
                {
                    log.success = false;
                    log.Message = "Usuarios ou Senha incorreto";
                    return log;
                }

                var achar = await ValidarToken(Email).ConfigureAwait(false);

                if (!achar.success || achar.Dados)
                {
                    var RefreshToken = token.GenerateRefreshToken();

                    CookieOptions options = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1),
                    };

                    responseCookies.HttpContext?.Response.Cookies.Append("Token", RefreshToken, options);

                    var tokenHashado = hasher.HashToken(RefreshToken);

                    var Validation = new TokenValidation
                    {
                        Email = Email,
                        RefreshToken = tokenHashado
                    };

                    await Context.TokenValidation.AddAsync(Validation).ConfigureAwait(false);
                    await Context.SaveChangesAsync().ConfigureAwait(false);

                }
                else
                {
                    var RefreshTokenBanco = await Context.TokenValidation.AsNoTracking()
                                                .Where(p => p.Email == Email)
                                                .Select(p => p.RefreshToken)
                                                .FirstOrDefaultAsync().ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(RefreshTokenBanco))
                    {
                        log.success = false;
                        log.Message = "RefreshToken não encontrado";
                        return log;
                    }
                }

                var tokenAcesso = token.GenerateToken(pessoa.Name);

                log.success = true;
                log.Message = "Logado com sucesso";
                log.Dados.Insert(0, tokenAcesso);

                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }

        public async Task<TabelaProblem<GoogleJsonWebSignature.Payload>> LogarGoogle(TokenDto dto)
        {
            var log = new TabelaProblem<GoogleJsonWebSignature.Payload>();
            try
            {

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken).ConfigureAwait(false);

                log.Message = "Token valido";
                log.Dados = payload;

                return log;
            }
            catch (InvalidJwtException ex)
            {
                log.success = false;
                log.Message = $"Token invalido: {ex.Message}";
                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }

        public async Task<TabelaProblem<string>> Disconnect()
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            try
            {
                if (responseCookies.HttpContext == null)
                {
                    log.success = false;
                    log.Message = "Contexto HTTP não disponivel";
                    return log;
                }

                string? tokenRequest = responseCookies.HttpContext?.Request.Cookies["Token"]; //Pega o Token armazenado no cookie

                responseCookies.HttpContext?.Response.Cookies.Delete("Token");

                if (string.IsNullOrWhiteSpace(tokenRequest))
                {
                    log.success = false;
                    log.Message = "Nenhum token encontrado";
                    return log;
                }

                var tokenLimpo = Uri.UnescapeDataString(tokenRequest);
                var tokenHashado = hasher.HashToken(tokenLimpo);

                var Deletado = await Context.TokenValidation
                                    .Where(p => p.RefreshToken == tokenHashado)
                                    .ExecuteDeleteAsync().ConfigureAwait(false);

                if (Deletado <= 0)
                {
                    log.success = false;
                    log.Message = "Nenhum Token encontrado";

                    return log;
                }

                log.success = true;
                log.Message = "Token Removido com sucesso";
                log.Dados = null;

                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";

                return log;
            }
        }
    }
}
