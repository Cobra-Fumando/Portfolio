using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Tabelas
{
    [Table("Verificar")]
    public class Verificar
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Codigo é obrigatório")]
        public required string Codigo { get; set; }

        [Required]
        public DateTime Expiracao { get; set; } = DateTime.UtcNow.AddMinutes(5);

        [Required]
        public DateTime Criacao { get; set; } = DateTime.UtcNow;
    }
}
