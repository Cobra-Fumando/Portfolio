using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Classes;
using Portfolio.Tabelas;

namespace Portfolio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly Admin Administrador;
        public AdminController(Admin admin)
        {
            Administrador = admin;
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("LoginAdmin")]
        public async Task<IActionResult> LogarAdmin(Logar logar)
        {
            try
            {
                var Resultado = await Administrador.LogarAdm(logar.Email, logar.Password);

                if (!Resultado.success)
                {
                    return BadRequest(new { Message = Resultado.Message });
                }

                return Ok(new { Message = Resultado.Message });
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
            try
            {
                var delete = await Administrador.DeletarUsuarios(Email);

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
    }
}
