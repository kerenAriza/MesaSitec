using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthController : ControllerBase
{
    private readonly AutenticacionService _autenticacionService;

    public AuthController(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto peticion)
    {
        var respuesta = await _autenticacionService.Login(peticion);
        return Ok(respuesta);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var usuarioDto = new UsuarioDto(
            Guid.Parse(User.FindFirst("sub")!.Value),
            User.Identity?.Name ?? "",
            User.FindFirst("email")!.Value,
            User.FindFirst("rol")!.Value,
            Guid.Parse(User.FindFirst("tenantId")!.Value),
            "");

        return Ok(usuarioDto);
    }
}