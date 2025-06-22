
using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;

namespace Portfolio.Background
{
    public class Verificacao : BackgroundService
    {
        private readonly ILogger<Verificacao> logger;
        private readonly IServiceScopeFactory serviceProvider;

        public Verificacao(ILogger<Verificacao> logger, IServiceScopeFactory serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    using var scope = serviceProvider.CreateScope();

                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var agora = DateTime.UtcNow;
                    var expirados = await context.Verificar.Where(p => p.Expiracao < agora).ToListAsync();

                    if(expirados.Count == 0)
                    {
                        logger.LogInformation("Nenhum codigo encontrado");
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                        continue;
                    }

                    context.Verificar.RemoveRange(expirados);
                    await context.SaveChangesAsync(stoppingToken).ConfigureAwait(false);

                    logger.LogInformation("Codigos deletados com sucesso");

                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
            }
        }
    }
}
