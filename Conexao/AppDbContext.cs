using Microsoft.EntityFrameworkCore;
using Portfolio.Tabelas;

namespace Portfolio.Conexao
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<TabelaPrincipal> Projeto {  get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<TokenValidation> TokenValidation { get; set; }
    }
}
