using System.ComponentModel.DataAnnotations;

namespace Portfolio.Tabelas
{
    public class Logar
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        public required string Codigo { get; set; }
    }

    public class Verify
    {
        [Required]
        public required string Email {  set; get; }
        public string? Codigo {  set; get; }
    }
}
