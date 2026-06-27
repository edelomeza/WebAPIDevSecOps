using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIDevSecOps.Controllers
{
    /// <summary>
    /// Controlador de prueba para verificar autenticación y conectividad.
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/test")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Endpoint protegido que requiere autenticación JWT.
        /// </summary>
        /// <returns>Mensaje de confirmaciín si el token es válido.</returns>
        /// <response code="200">Token válido, acceso concedido.</response>
        /// <response code="401">Token ausente o inválido.</response>
        [Authorize]
        [HttpGet("secure")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Secure()
        {
            return Ok("ok");
        }
    }
}
