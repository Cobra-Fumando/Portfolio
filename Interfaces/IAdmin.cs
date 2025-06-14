using Portfolio.Tabelas;

namespace Portfolio.Interfaces
{
    public interface IAdmin
    {
        Task<TabelaProblem<string>> DeletarUsuarios(string Email);
    }
}
