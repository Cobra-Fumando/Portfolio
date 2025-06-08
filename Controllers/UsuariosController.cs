using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Portfolio.Dto;
using Portfolio.Interfaces;
using Portfolio.Tabelas;

namespace Portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsers users;
        private readonly ILogger<UsuariosController> logger;
        public UsuariosController(IUsers users, ILogger<UsuariosController> logger)
        {
            this.users = users;
            this.logger = logger;
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("Adicionar")]
        public async Task<IActionResult> add(Usuarios usuarios)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var Adicionar = await users.add(usuarios).ConfigureAwait(false);

                if (!Adicionar.success)
                {
                    return NotFound(new { Mensagem = Adicionar.Message });
                }

                if(Adicionar.Dados == null)
                {
                    return BadRequest(new {Mensagem = Adicionar.Message });
                }

                return Ok(new { Mensagem = Adicionar.Message});
            }catch(Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("Logar")]
        public async Task<IActionResult> Logar(Logar login) 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = await users.Logar(login.Email, login.Password).ConfigureAwait(false);

                if (!token.success)
                {
                    return NotFound(new {Mensagem = token.Message});
                }

                if(token.Dados == null)
                {
                    return NotFound(new {Mensagem = token.Message});
                }

                return Ok(new {
                    Mensagem = token.Message, 
                    Token = token.Dados.ElementAtOrDefault(0),
                    RefreshToken = token.Dados.ElementAtOrDefault(1)
                });
            }catch(Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("LogarGoogle")]
        public async Task<IActionResult> LoginGoogle([FromBody] TokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await users.LogarGoogle(dto);

            if (!result.success)
            {
                logger.LogWarning($"Erro ao logar no google: {result.Message}");
                return Unauthorized(new {Mensagem = result.Message });
            }

            if(result.Dados == null)
            {
                return BadRequest(new { Mensagem = "Payload vazio" });
            }

            return Ok(new 
            {
                Mensagem = result.Message,
                GoogleId = result.Dados.Subject,
                Email = result.Dados.Email,
                Nome = result.Dados.Name,
                Foto = result.Dados.Picture
            });
        }
    }
}
