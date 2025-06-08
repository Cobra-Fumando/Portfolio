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
        public async Task<IActionResult> Adicionar(TabelaPrincipal tabela)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await projeto.add(tabela).ConfigureAwait(false);

                if (!resultado.success)
                {
                    return Conflict(new {Mensagem = resultado.Message });
                }

                return Ok(new {Mensagem = resultado.Message, Tabela = resultado.Dados});
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
                    return BadRequest(new {Mensagem = resultado.Message});
                }

                return Ok(new { Mensagem = resultado.Message, Projeto = resultado.Dados});
            }catch(Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }
    }
}
