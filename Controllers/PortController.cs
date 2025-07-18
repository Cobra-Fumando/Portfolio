using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Conexao;
using Portfolio.Dto;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

namespace Portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortController : ControllerBase
    {
        private readonly IProjeto projeto;
        private readonly ILogger<PortController> logger;
        public PortController(IProjeto projeto, ILogger<PortController> logger)
        {
            this.projeto = projeto;
            this.logger = logger;
        }

        [Authorize]
        [EnableRateLimiting("Fixed")]
        [HttpPost("Adicionar")]
        public async Task<IActionResult> Adicionar([FromForm] TabelaPrincipal tabela, [FromForm] IFormFile imagens)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await projeto.add(tabela, imagens).ConfigureAwait(false);

                if (!resultado.success)
                {
                    return Conflict(new { Mensagem = resultado.Message });
                }

                return Ok(new { Mensagem = resultado.Message, Tabela = resultado.Dados });
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpGet("Todos/{pagina}")]
        public async Task<IActionResult> TodosProjetos(int pagina)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await projeto.Todos(pagina, 10).ConfigureAwait(false);

                if (!resultado.success)
                {
                    return BadRequest(new { Mensagem = resultado.Message });
                }

                return Ok(new { Mensagem = resultado.Message, Projeto = resultado.Dados });
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpGet("ObterImagem/{Imagem}")]
        public async Task<IActionResult> ObterImage(string Imagem)
        {
            var local = Path.Combine(Directory.GetCurrentDirectory(), "Imagem");

            var resultado = await projeto.ObterImagem(Imagem).ConfigureAwait(false);

            if (!resultado.success) return BadRequest(new { Mensagem = resultado.Message });

            var extensao = Path.GetExtension(Imagem);

            if (resultado.Dados == null || resultado.Message == null)
            {
                return NotFound(new { Mensagem = "Nenhuma imagem encontrada" });
            }

            return File(resultado.Dados, resultado.Message, $"{Guid.NewGuid()}{extensao}");
        }
    }
}
