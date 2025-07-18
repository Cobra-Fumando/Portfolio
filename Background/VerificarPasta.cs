
namespace Portfolio.Background
{
    public class VerificarPasta : BackgroundService
    {
        private readonly ILogger<VerificarPasta> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public VerificarPasta(ILogger<VerificarPasta> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var local = Path.Combine(Directory.GetCurrentDirectory(), "Imagem");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(local))
                {
                    Directory.CreateDirectory(local);
                    logger.LogInformation("Pasta criada com sucesso");
                }
                else
                {
                    logger.LogInformation("Essa pasta já existe");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
