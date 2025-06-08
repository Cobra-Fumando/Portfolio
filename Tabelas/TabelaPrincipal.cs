using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portfolio.Tabelas
{
    [Table("Projeto")]
    public class TabelaPrincipal
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Image é obrigatória")]
        public required string Image { get; set; }

        [Required(ErrorMessage = "Urls obrigatória")]
        public required string Urls { get; set; }

        [Required(ErrorMessage = "Titulo é obrigatório")]
        public required string Titulo { get; set; }

        [Required(ErrorMessage = "Descricao é obrigatória")]
        public required string Descricao { get; set; }

    }
}
