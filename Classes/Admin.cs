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
        public Admin(ILogger<Admin> logger, AppDbContext context, IHttpContextAccessor httpContextAccessor, Token token) 
        {
            Context = context;
            this.logger = logger;
            this.HttpContextAccessor = httpContextAccessor;
            this.token = token;
        }
        public async Task<TabelaProblem<string>> DeletarUsuarios(string Email)
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            try
            {
                var Emailvalido = new ValidacaoEmail();
                bool Certo = Emailvalido.EmailValido(Email);

                if (!Certo)
                {
                    log.success = false;
                    log.Message = "Email Invalido";
                    return log;
                }

                var token = HttpContextAccessor.HttpContext?.Request.Cookies["Token"];

                if (token == null)
                {
                    log.success = false;
                    log.Message = "Nenhum Token encontrado";
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    log.success = false;
                    log.Message = "Email não está no formato certo";
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
                var Pessoa = await Context.Usuarios.Where(p => p.Email == Email && p.Password == Senha && p.Role == "Admin")
                                    .FirstOrDefaultAsync().ConfigureAwait(false); //Procura a pessoa com descrição especifica

                if (Pessoa == null)
                {
                    log.success = false;
                    log.Message = "Email ou Senha está incorreta";
                    return log;
                }

                var tokenAdm = token.GenerateTokenAdmin(Pessoa.Name); //Gera o Token admin

                log.success = true;
                log.Message = "Usuario logado com sucesso";
                return log;
            }catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }
    }
}
