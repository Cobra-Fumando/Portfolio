using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Tabelas
{
    [Table("Banimentos")]
    public class Banimento
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ip é obrigatório")]
        public string? Ip { get; set; }
    }
}
