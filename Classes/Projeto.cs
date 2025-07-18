using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache cache;
        public Projeto(ILogger<Projeto> logger, AppDbContext context, IMemoryCache cache)
        {
            this.logger = logger;
            this.context = context;
            this.cache = cache;
        }

        public async Task<TabelaProblem<TabelaDto>> add(TabelaPrincipal tabela, IFormFile image)
        {
            var log = new TabelaProblem<TabelaDto>();

            var local = Path.Combine(Directory.GetCurrentDirectory(), "Imagem");

            if (image == null || image.Length == 0)
            {
                log.success = false;
                log.Message = "Imagem não foi enviada";
                log.Dados = null;

                return log;
            }

            if (!Directory.Exists(local))
            {
                log.success = false;
                log.Message = "Pasta não encontrada";
                log.Dados = null;

                return log;
            }

            var extensão = Path.GetExtension(image.FileName);
            var NomeArquivo = $"{Guid.NewGuid()}{extensão}";

            var CaminhoArquivo = Path.Combine(local, NomeArquivo);

            using (var stream = new FileStream(CaminhoArquivo, FileMode.Create))
            {
                await image.CopyToAsync(stream).ConfigureAwait(false);
            }

            try
            {
                if (tabela == null)
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

                tabela.Image = NomeArquivo;

                await context.AddAsync(tabela).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);

                var tabeladto = new TabelaDto
                {
                    Description = tabela.Descricao,
                    Urls = tabela.Urls,
                    Image = NomeArquivo,
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

            if(pagina < 1)
            {
                logger.LogWarning("Pagina não pode ser menor que 1");
                logger.LogInformation("Mudando para 1");

                pagina = 1;
            }

            if(tamanho < 1)
            {
                logger.LogWarning("Tamanho não pode ser menor que 1");
                logger.LogInformation("Mudando para 1");

                tamanho = 1;
            }

            try
            {
                if (!cache.TryGetValue(pagina, out List<TabelaDto>? resultado))
                {
                    resultado = await context.Projeto.AsNoTracking()
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

                    cache.Set(pagina, resultado, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1),
                        Priority = CacheItemPriority.Normal
                    });
                }

                log.Message = "Projetos encontrado";
                log.Dados = resultado;

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

        public async Task<TabelaProblem<byte[]>> ObterImagem(string Imagem)
        {
            var log = new TabelaProblem<byte[]>();

            try
            {

                var projeto = await context.Projeto.AsNoTracking()
                                .Where(p => p.Image == Imagem)
                                .FirstOrDefaultAsync().ConfigureAwait(false);

                if (projeto == null || projeto.Image == null)
                {
                    log.success = false;
                    log.Message = "Nenhuma imagem encontrada";

                    return log;
                }

                var local = Path.Combine(Directory.GetCurrentDirectory(), "Imagem");

                if (!Directory.Exists(local))
                {
                    log.success = false;
                    log.Message = "Não existe essa pasta";
                    return log;
                }

                var NomeArquivo = Path.Combine(local, projeto.Image);

                if (!System.IO.File.Exists(NomeArquivo))
                {
                    log.success = false;
                    log.Message = "Arquivo não existe";
                    return log;
                }

                var imagem = System.IO.File.ReadAllBytes(NomeArquivo);

                var extensao = Path.GetExtension(projeto.Image);
                var mime = $"image/{extensao.Replace(".", "")}";

                log.success = true;
                log.Message = mime;
                log.Dados = imagem;

                return log;
            }catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";

                return log;
            }
        }
    }
}
