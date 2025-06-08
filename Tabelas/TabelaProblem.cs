namespace Portfolio.Tabelas
{
    public class TabelaProblem<T>
    {
        public string? Message { get; set; }
        public bool success { get; set; } = true;

        public T? Dados { get; set; }
    }
}
