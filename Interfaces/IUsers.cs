using Google.Apis.Auth;
using Portfolio.Dto;
using Portfolio.Tabelas;

namespace Portfolio.Interfaces
{
    public interface IUsers
    {
        Task<TabelaProblem<UsuariosDto>> add(Usuarios usuarios);
        Task<TabelaProblem<List<string>>> Logar(string Email, string Senha);
        Task<TabelaProblem<GoogleJsonWebSignature.Payload>> LogarGoogle(TokenDto dto);

        Task<TabelaProblem<string>> Disconnect (TokenDto Token);
    }
}
