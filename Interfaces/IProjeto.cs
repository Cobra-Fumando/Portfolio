using Portfolio.Dto;
using Portfolio.Tabelas;

namespace Portfolio.Interfaces
{
    public interface IProjeto
    {
        Task<TabelaProblem<TabelaDto>> add(TabelaPrincipal tabela, IFormFile image);
        Task<TabelaProblem<List<TabelaDto>>> Todos(int pagina, int tamanho);
        Task<TabelaProblem<byte[]>> ObterImagem(string Imagem);
    }
}
