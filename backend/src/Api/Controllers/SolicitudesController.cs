using Api.Extensions;
using Aplicacion.DTOs;
using Aplicacion.Servicios;
using Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public class SolicitudesController : ControllerBase
{
    private readonly SolicitudesService _solicitudesService;

    public SolicitudesController(SolicitudesService solicitudesService)
    {
        _solicitudesService = solicitudesService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] PrioridadSolicitud? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "-fechaCreacion")
    {
        var resultado = await _solicitudesService.Listar(
            User.ObtenerTenantId(), User.ObtenerUsuarioId(), User.ObtenerRol(),
            estado, prioridad, categoriaId, agenteId, q, vencidas, page, pageSize, sort);

        return Ok(resultado);
    }
    [HttpGet("{id}")]
public async Task<IActionResult> Obtener(Guid id)
{
    var resultado = await _solicitudesService.Obtener(
        User.ObtenerTenantId(), User.ObtenerUsuarioId(), User.ObtenerRol(), id);

    return Ok(resultado);
}

[HttpPost]
public async Task<IActionResult> Crear([FromBody] CrearSolicitudDto peticion)
{
    var resultado = await _solicitudesService.Crear(
        User.ObtenerTenantId(), User.ObtenerUsuarioId(), peticion);

    return CreatedAtAction(nameof(Obtener), new { id = resultado.Id }, resultado);
}

[HttpPut("{id}")]
public async Task<IActionResult> Editar(Guid id, [FromBody] EditarSolicitudDto peticion)
{
    var resultado = await _solicitudesService.Editar(
        User.ObtenerTenantId(), User.ObtenerUsuarioId(), User.ObtenerRol(), id, peticion);

    return Ok(resultado);
}

[HttpPost("{id}/transiciones")]
public async Task<IActionResult> EjecutarTransicion(Guid id, [FromBody] TransicionRequestDto peticion)
{
    var resultado = await _solicitudesService.EjecutarTransicion(
        User.ObtenerTenantId(), User.ObtenerUsuarioId(), User.ObtenerRol(), id, peticion);

    return Ok(resultado);
}
}