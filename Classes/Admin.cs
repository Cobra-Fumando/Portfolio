using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

namespace Portfolio.Classes
{
    public class Admin : IAdmin
    {
        private readonly ILogger<Admin> logger;
        private readonly AppDbContext Context;
        public Admin(ILogger<Admin> logger, AppDbContext context) 
        {
            Context = context;
            this.logger = logger;
        }
        public async Task<TabelaProblem<string>> DeletarUsuarios(string Email)
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            try
            {
                Email = Email.Trim();

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
    }
}
