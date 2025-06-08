using System.ComponentModel.DataAnnotations;

namespace Portfolio.Dto
{
    public class UsuariosDto
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
    }
}
