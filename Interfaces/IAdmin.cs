using Portfolio.Tabelas;

namespace Portfolio.Interfaces
{
    public interface IAdmin
    {
        Task<TabelaProblem<string>> DeletarUsuarios(string Email);
        Task<TabelaProblem<string>> LogarAdm(string Email, string Senha);
    }
}
