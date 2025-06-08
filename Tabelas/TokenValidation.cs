using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Tabelas
{
    [Table("Token")]
    public class TokenValidation
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Token é obrigatório")]
        public required string RefreshToken { get; set; }

        [Required(ErrorMessage = "Data de criação é obrigatória")]
        public DateTime DataCriacao {  get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Data de expiração é obrigatória")]
        public DateTime DataExpiracao { get; set; } = DateTime.UtcNow.AddDays(7);
    }
}
