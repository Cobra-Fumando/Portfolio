using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Config;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

namespace Portfolio.Classes
{
    public class Admin : IAdmin
    {
        private readonly ILogger<Admin> logger;
        private readonly AppDbContext Context;
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly Token token;
        private readonly Hash hasher;
        private readonly ObterIp obterIp;
        public Admin(ILogger<Admin> logger, 
            AppDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            Token token, 
            Hash hash, 
            ObterIp obterIp
            )
        {
            Context = context;
            this.logger = logger;
            this.HttpContextAccessor = httpContextAccessor;
            this.token = token;
            hasher = hash;
            this.obterIp = obterIp;
        }
        public async Task<TabelaProblem<string>> DeletarUsuarios(string Email)
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    log.success = false;
                    log.Message = "Email está vazio";
                    return log;
                }

                var Emailvalido = new ValidacaoEmail();
                bool Certo = Emailvalido.EmailValido(Email);

                if (!Certo)
                {
                    log.success = false;
                    log.Message = "Email Invalido";
                    return log;
                }

                var Deleta = await Context.Usuarios.Where(p => p.Email == Email)
                                    .ExecuteDeleteAsync().ConfigureAwait(false);

                var key = await Context.TokenValidation.Where(p => p.Email == Email)
                                    .ExecuteDeleteAsync().ConfigureAwait(false);

                if (Deleta <= 0)
                {
                    log.success = false;
                    log.Message = "Nenhum Usuarios encontrado";
                    return log;
                }

                if (key <= 0)
                {
                    log.success = true;
                    log.Message = "Key não encontrada";
                }

                log.success = true;
                log.Message = "Usuarios deletado com sucesso";

                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }

        public async Task<TabelaProblem<string>> LogarAdm(string Email, string Senha)
        {
            var log = new TabelaProblem<string>()
            {
                Dados = null
            };

            try
            {
                string? SenhaHash = hasher.Hashar(Senha);

                var Pessoa = await Context.Usuarios.Where(p => p.Email == Email && p.Password == SenhaHash && p.Role == "Admin")
                                    .FirstOrDefaultAsync().ConfigureAwait(false); //Procura a pessoa com descrição especifica

                if (Pessoa == null)
                {
                    log.success = false;
                    log.Message = "Email ou Senha está incorreta";
                    return log;
                }

                string? tokenAdm = token.GenerateTokenAdmin(Pessoa.Name); //Gera o Token admin

                log.success = true;
                log.Message = "Usuario logado com sucesso";
                log.Dados = tokenAdm;
                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }
        public async Task<TabelaProblem<string>> Ban(Banimento banimento)
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            if (banimento == null)
            {
                log.success = false;
                log.Message = "Modelo invalido";
                return log;
            }

            try
            {
                bool existe = await Context.Banimentos.AsNoTracking()
                            .AnyAsync(p => p.Ip == banimento.Ip)
                            .ConfigureAwait(false);

                if (existe)
                {
                    log.success = false;
                    log.Message = "Esse ip já está banido";
                    return log;
                }

                await Context.Banimentos.AddAsync(banimento).ConfigureAwait(false);
                await Context.SaveChangesAsync().ConfigureAwait(false);

                log.success = true;
                log.Message = "Banido com sucesso";
                return log;
            }
            catch (Exception ex)
            {
                logger.LogError($"Ocorreu um erro: {ex.Message}");

                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }

        }
    }
}
