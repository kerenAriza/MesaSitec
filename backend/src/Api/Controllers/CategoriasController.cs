using Api.Extensions;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriasService _categoriasService;

    public CategoriasController(CategoriasService categoriasService)
    {
        _categoriasService = categoriasService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _categoriasService.Listar(User.ObtenerTenantId());
        return Ok(categorias);
    }
}