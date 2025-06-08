using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Config;
using Portfolio.Interfaces;
using Portfolio.Tabelas;
using Portfolio.Dto;

namespace Portfolio.Classes
{
    public class Users : IUsers
    {
        private readonly ILogger<Users> logger;
        private readonly AppDbContext Context;
        private readonly Token token;
        private readonly Hash hasher;
        public Users(ILogger<Users> logger, AppDbContext context, Hash hasher, Token token)
        {
            this.logger = logger;
            this.Context = context;
            this.hasher = hasher;
            this.token = token;
        }

        private async Task<TabelaProblem<bool>> ValidarToken(string Email)
        {
            var log = new TabelaProblem<bool>();

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

        public async Task<TabelaProblem<UsuariosDto>> add(Usuarios usuarios)
        {
            var log = new TabelaProblem<UsuariosDto>();
            var validar = new ValidacaoEmail();

            try
            {

                if (usuarios == null)
                {
                    log.success = false;
                    log.Message = "Preencha usuarios";
                    return log;
                }

                var valido = validar.EmailValido(usuarios.Email);

                if (!valido)
                {
                    log.success = false;
                    log.Message = "Email invalido";
                    return log;
                }

                var existe = await Context.Usuarios.AnyAsync(p => p.Email == usuarios.Email).ConfigureAwait(false);

                if (existe)
                {
                    log.success = false;
                    log.Message = "Esse Email já existe";
                    return log;
                }

                var Hash = hasher.Hashar(usuarios, usuarios.Password);

                var Novo = new Usuarios
                {
                    Email = usuarios.Email,
                    Password = Hash,
                    Name = usuarios.Name,
                };

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

                var pessoa = await Context.Usuarios.AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Email == Email)
                            .ConfigureAwait(false);

                if (pessoa == null)
                {
                    log.success = false;
                    log.Message = "Pessoa não encontrada";
                    return log;
                }

                var verificado = hasher.Verificar(pessoa, Senha, pessoa.Password);

                if (!verificado)
                {
                    log.success = false;
                    log.Message = "Email ou Senha incorreto";
                    return log;
                }

                var achar = await ValidarToken(Email).ConfigureAwait(false);

                if (!achar.success || achar.Dados)
                {
                    var RefreshToken = token.GenerateRefreshToken();
                    log.Dados.Add(RefreshToken);

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

                    log.Dados.Add(RefreshTokenBanco);
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
    }
}
