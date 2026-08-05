using Aplicacion.DTOs;
using Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Servicios;

public class CategoriasService
{
    private readonly MesaSitecDbContext _db;

    public CategoriasService(MesaSitecDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoriaDto>> Listar(Guid tenantId)
    {
        return await _db.Categorias
            .Where(c => c.TenantId == tenantId && c.Activo)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.SlaHoras))
            .ToListAsync();
    }
}