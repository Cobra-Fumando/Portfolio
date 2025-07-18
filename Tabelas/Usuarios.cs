using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Tabelas
{
    [Table("Usuarios")]
    public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Role é obrigatória")]
        public string Role { get; set; } = "Usuarios";

        public string Ip { get; set; } = string.Empty;
    }
}
