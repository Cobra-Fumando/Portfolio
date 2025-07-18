using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Portfolio.Config;
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
        private readonly EmailSmtp emailSmtp;
        private readonly IMemoryCache memoryCache;
        private readonly ValidacaoEmail validarEmail;
        public UsuariosController(IUsers users, ILogger<UsuariosController> logger, EmailSmtp emailSmtp, IMemoryCache memoryCache, ValidacaoEmail validarEmail)
        {
            this.users = users;
            this.logger = logger;
            this.emailSmtp = emailSmtp;
            this.memoryCache = memoryCache;
            this.validarEmail = validarEmail;
        }

        //[Authorize(Roles = "Administrador,Usuarios")]
        [EnableRateLimiting("Fixed")]
        [HttpPost("Adicionar")]
        public async Task<IActionResult> add([FromBody] Usuarios usuarios, [FromQuery] string? Permission)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (usuarios == null) return NotFound(new { Mensagem = "Dados invalidos" });

            bool valido = validarEmail.EmailValido(usuarios.Email);
            if (!valido) return BadRequest(new { Mensagem = "Email invalido" });

            try
            {

                var Adicionar = await users.add(usuarios, Permission).ConfigureAwait(false);

                if (!Adicionar.success)
                {
                    return NotFound(new { Mensagem = Adicionar.Message });
                }

                if (Adicionar.Dados == null)
                {
                    return BadRequest(new { Mensagem = Adicionar.Message });
                }

                return Ok(new { Mensagem = Adicionar.Message });
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("Logar")]
        public async Task<IActionResult> Logar([FromBody] Logar login)
        {
            var config = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                        .SetPriority(CacheItemPriority.Normal);

            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (login == null) return BadRequest(new { Mensagem = "Dados invalido" });

            bool valido = validarEmail.EmailValido(login.Email);
            if (!valido) return BadRequest(new { Mensagem = "Email invalido" });

            string code = login.Codigo.Trim();
            string Tentativa = $"tentativa";
            string PalavraCode = $"Codigo";

            int tentativa = 0;

            try
            {

                if (!HttpContext.Session.TryGetValue(PalavraCode, out byte[]? CodigoByte))
                {
                    return NotFound(new { Mensagem = "Codigo não encontrado" });
                }

                string Codigo = Encoding.UTF8.GetString(CodigoByte);
                string Code = Codigo?.Trim() ?? string.Empty;

                if (!HttpContext.Session.TryGetValue(Tentativa, out byte[]? TentativaByte))
                {
                    tentativa = 1;
                    HttpContext.Session.SetInt32(Tentativa, tentativa);
                }
                else
                {
                    tentativa = BitConverter.ToInt32(TentativaByte);

                    tentativa += 1;
                    HttpContext.Session.SetInt32(Tentativa, tentativa);
                }

                if (tentativa > 5)
                {
                    HttpContext.Session.Remove(PalavraCode);
                    HttpContext.Session.Remove(Tentativa);

                    return StatusCode(429, new { Mensagem = "Codigo invalido ou expirado" });
                }

                if (Code != code)
                {
                    return BadRequest("Codigo invalido");
                }

                HttpContext.Session.Remove(PalavraCode);
                HttpContext.Session.Remove(Tentativa);

                var token = await users.Logar(login.Email, login.Password).ConfigureAwait(false);

                if (!token.success || token.Dados == null)
                {
                    return NotFound(new { Mensagem = token.Message });
                }

                return Ok(new
                {
                    Mensagem = token.Message,
                    Token = token.Dados.ElementAtOrDefault(0),
                });
            }
            catch (Exception ex)
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
                return Unauthorized(new { Mensagem = result.Message });
            }

            if (result.Dados == null)
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

        [Authorize]
        [EnableRateLimiting("Fixed")]
        [HttpDelete("Desconectar")]
        public async Task<IActionResult> Disconnect()
        {
            try
            {
                var resposta = await users.Disconnect().ConfigureAwait(false);

                if (!resposta.success)
                {
                    return NotFound(new { Mensagem = resposta.Message });
                }

                return Ok(new { Mensagem = resposta.Message });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro inesperado: {ex.Message}");
            }
        }

        [EnableRateLimiting("Fixed")]
        [HttpPost("Verificar")]
        public IActionResult Message([FromBody] Verify logar)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (logar == null) return StatusCode(429, new { Mensagem = "Dados invalidos" });

            bool valido = validarEmail.EmailValido(logar.Email);
            if (!valido) return BadRequest(new { Mensagem = "Email invalido" });

            int[] Codigo = new int[4];

            for (int i = 0; i < Codigo.Length; i++)
            {
                Codigo[i] = RandomNumberGenerator.GetInt32(10);
            }

            var Code = string.Join("", Codigo);

            HttpContext.Session.SetString($"Codigo", Code);

            try
            {
                var mensagem = emailSmtp.CodigoSend("Email", logar.Email, "Codigo", "Nome", Code);

                if (!mensagem.success)
                {
                    return BadRequest(new { Mensagem = mensagem.Message });
                }

                return Ok(new { Mensagem = "Codigo enviado com sucesso" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = $"Erro inesperado: {ex.Message}" });
            }
        }
    }
}
