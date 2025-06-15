using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Portfolio.Classes;
using Portfolio.Tabelas;

namespace Portfolio.Config
{
    public class Hash
    {
        private readonly IPasswordHasher<Usuarios> hasher;

        public Hash(IPasswordHasher<Usuarios> hasher)
        {
            this.hasher = hasher;
        }

        public string Hashar(string Senha)
        {
            return hasher.HashPassword(null, Senha); //hasha a senha do usuario
        }

        public bool Verificar(string Senha, string SenhaHash)
        {
            var resultado = hasher.VerifyHashedPassword(null, SenhaHash, Senha); //compara a senha com o hash
            return resultado == PasswordVerificationResult.Success; //verifica se foi sucesso
        }

        public string HashToken(string tokenHash)
        {
            using var sha256 = SHA256.Create(); //cria a forma para cryptografia
            var bytes = Encoding.UTF8.GetBytes(tokenHash); //transforma o tokenHash em bytes
            var resultado = sha256.ComputeHash(bytes); //cryptografa usando o bytes
            return Convert.ToBase64String(resultado); //converte bytes pata string64
        }
    }
}
