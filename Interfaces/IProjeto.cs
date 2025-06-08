using Portfolio.Dto;
using Portfolio.Tabelas;

namespace Portfolio.Interfaces
{
    public interface IProjeto
    {
        Task<TabelaProblem<TabelaDto>> add(TabelaPrincipal tabela);
        Task<TabelaProblem<List<TabelaDto>>> Todos(int pagina, int tamanho);
    }
}
