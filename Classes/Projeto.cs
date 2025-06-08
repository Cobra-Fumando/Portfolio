using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Dto;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

namespace Portfolio.Classes
{
    public class Projeto : IProjeto
    {
        private readonly ILogger<Projeto> logger;
        private readonly AppDbContext context;
        public Projeto(ILogger<Projeto> logger, AppDbContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        public async Task<TabelaProblem<TabelaDto>> add(TabelaPrincipal tabela)
        {
            var log = new TabelaProblem<TabelaDto>();

            try
            {
                if(tabela == null)
                {
                    log.success = false;
                    log.Message = "Preencha a tabela";
                    return log;
                }

                var existe = await context.Projeto.AnyAsync(p => p.Urls == tabela.Urls).ConfigureAwait(false);

                if (existe)
                {
                    logger.LogInformation("Já existe essa url");

                    log.success = false;
                    log.Message = $"Já existe essa url {tabela.Urls}";

                    return log;
                }

                await context.AddAsync(tabela).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);

                var tabeladto = new TabelaDto
                {
                    Description = tabela.Descricao,
                    Urls = tabela.Urls,
                    Image = tabela.Image,
                    Titulo = tabela.Titulo,
                };

                log.Message = "Adicionado com sucesso";
                log.Dados = tabeladto;

                return log;
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";

                return log;
            }
        }

        public async Task<TabelaProblem<List<TabelaDto>>> Todos(int pagina, int tamanho)
        {
            var log = new TabelaProblem<List<TabelaDto>>();

            try
            {
                var resultado = await context.Projeto.AsNoTracking()
                                .Skip((pagina - 1) * tamanho)
                                .Take(tamanho)
                                .Select(p => new TabelaDto
                                {
                                    Description = p.Descricao,
                                    Urls = p.Urls,
                                    Image = p.Image,
                                    Titulo = p.Titulo,
                                })
                                .ToListAsync().ConfigureAwait(false);

                if (!resultado.Any())
                {
                    log.success = false;
                    log.Message = "Nenhum Projeto encontrado";
                    return log;
                }

                log.Message = "Projeto encontrado";
                log.Dados = resultado;

                return log;
            }
            catch (Exception ex)
            {
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }
    }
}
