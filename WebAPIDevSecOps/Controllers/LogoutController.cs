using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIDevSecOps.Services;

namespace WebAPIDevSecOps.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LogoutController : ControllerBase
    {
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            if (!string.IsNullOrEmpty(token))
            {
                TokenBlacklist.Add(token);
                return Ok(new { mensaje = "Sesión cerrada correctamente." });
            }

            return Unauthorized(new { mensaje = "No se proporcionó un token." });
        }
    }
}
