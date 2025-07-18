namespace Portfolio.Config
{
    public class ObterIp
    {
        private readonly IHttpContextAccessor contextAcessor;

        public ObterIp(IHttpContextAccessor contextAcessor)
        {
            this.contextAcessor = contextAcessor;
        }

        public string ValidarIp()
        {
            string? ip = contextAcessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if(string.IsNullOrWhiteSpace(ip)) return "Ip não disponivel";

            return ip;
        }
    }
}
