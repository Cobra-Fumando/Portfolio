using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Classes;
using Portfolio.Config;
using Portfolio.Tabelas;

namespace Portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Admin Administrador;
        private readonly ILogger<AdminController> logger;
        private readonly ValidacaoEmail validacao;
        public AdminController(Admin admin, ILogger<AdminController> logger, ValidacaoEmail validacao)
        {
            Administrador = admin;
            this.logger = logger;
            this.validacao = validacao;
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("LoginAdmin")]
        public async Task<IActionResult> LogarAdmin([FromBody] Logar logar)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if(logar == null) return BadRequest(new { Mensagem = "Modelo invalido"});

            try
            {
                var Resultado = await Administrador.LogarAdm(logar.Email, logar.Password);

                if (!Resultado.success)
                {
                    return BadRequest(new { Mensagem = Resultado.Message });
                }

                return Ok(new { Mensagem = Resultado.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [Authorize("Administrador")]
        [EnableRateLimiting("Adm")]
        [HttpDelete("DeletarUsers")]
        public async Task<IActionResult> Deletar(string Email)
        {
            if(string.IsNullOrWhiteSpace(Email)) return BadRequest(new { Mensagem = "Modelo invalido"});
            var EmailEspaco = Email.Trim();

            var valido = validacao.EmailValido(EmailEspaco);

            if(!valido) return BadRequest(new { Mensagem = "Modelo do Email está invalido"});

            try
            {
                var delete = await Administrador.DeletarUsuarios(EmailEspaco);

                if (!delete.success)
                {
                    return NotFound(delete.Message);
                }

                return Ok(delete.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [Authorize("Administrador")]
        [EnableRateLimiting("Adm")]
        [HttpPost("Banir")]
        public async Task<IActionResult> Banimentos(Banimento banimentos)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if (banimentos == null) return BadRequest(new { Mensagem = "Modelo invalido"});

            try
            {
                var resultado = await Administrador.Ban(banimentos);

                if(!resultado.success) return NotFound(new { Mensagem = resultado.Message });

                return Ok(resultado.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }
    }
}
