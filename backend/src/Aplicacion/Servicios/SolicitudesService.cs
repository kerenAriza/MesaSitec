using Aplicacion.DTOs;
using Aplicacion.Excepciones;
using Dominio.Entidades;
using Dominio.Reglas;
using Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Servicios;

public class SolicitudesService
{
    private readonly MesaSitecDbContext _db;

    public SolicitudesService(MesaSitecDbContext db)
    {
        _db = db;
    }

    public async Task<SolicitudListResponseDto> Listar(
        Guid tenantId,
        Guid usuarioId,
        string rol,
        EstadoSolicitud? estado,
        PrioridadSolicitud? prioridad,
        Guid? categoriaId,
        Guid? agenteId,
        string? q,
        bool? vencidas,
        int page,
        int pageSize,
        string sort)
    {
        if (page < 1 || pageSize > 100 || pageSize < 1)
        {
            throw new ParametroInvalidoException("page debe ser >= 1 y pageSize debe estar entre 1 y 100.");
        }

        var consulta = _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == tenantId);

        if (rol == "Solicitante")
        {
            consulta = consulta.Where(s => s.SolicitanteId == usuarioId);
        }

        if (estado.HasValue)
        {
            consulta = consulta.Where(s => s.Estado == estado.Value);
        }

        if (prioridad.HasValue)
        {
            consulta = consulta.Where(s => s.Prioridad == prioridad.Value);
        }

        if (categoriaId.HasValue)
        {
            consulta = consulta.Where(s => s.CategoriaId == categoriaId.Value);
        }

        if (agenteId.HasValue)
        {
            consulta = consulta.Where(s => s.AgenteId == agenteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            string qMinuscula = q.ToLower();
            consulta = consulta.Where(s =>
                s.Titulo.ToLower().Contains(qMinuscula) ||
                s.Descripcion.ToLower().Contains(qMinuscula) ||
                s.Codigo.ToLower().Contains(qMinuscula));
        }

        var ahora = DateTime.UtcNow;

        if (vencidas.HasValue)
        {
            var estadosFinales = new[] { EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada, EstadoSolicitud.Cancelada };

            if (vencidas.Value)
            {
                consulta = consulta.Where(s => s.FechaLimiteSla < ahora && !estadosFinales.Contains(s.Estado));
            }
            else
            {
                consulta = consulta.Where(s => s.FechaLimiteSla >= ahora || estadosFinales.Contains(s.Estado));
            }
        }

        consulta = sort switch
        {
            "fechaCreacion" => consulta.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => consulta.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => consulta.OrderBy(s => s.Prioridad),
            "-prioridad" => consulta.OrderByDescending(s => s.Prioridad),
            "codigo" => consulta.OrderBy(s => s.Codigo),
            _ => consulta.OrderByDescending(s => s.FechaCreacion),
        };

        int total = await consulta.CountAsync();
        int totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

        var solicitudes = await consulta
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = solicitudes.Select(s => new SolicitudListItemDto(
            s.Id,
            s.Codigo,
            s.Titulo,
            s.Estado.ToString(),
            s.Prioridad.ToString(),
            new CategoriaResumenDto(s.Categoria!.Id, s.Categoria.Nombre),
            s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
            s.FechaCreacion,
            s.FechaLimiteSla,
            s.FechaLimiteSla < ahora && s.Estado != EstadoSolicitud.Resuelta
                && s.Estado != EstadoSolicitud.Cerrada && s.Estado != EstadoSolicitud.Cancelada
        )).ToList();

        return new SolicitudListResponseDto(items, page, pageSize, total, totalPaginas);
    }

    public async Task<SolicitudDetalleDto> Obtener(Guid tenantId, Guid usuarioId, string rol, Guid solicitudId)
    {
        var solicitud = await _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Include(s => s.Solicitante)
            .FirstOrDefaultAsync(s => s.Id == solicitudId && s.TenantId == tenantId);

        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (rol == "Solicitante" && solicitud.SolicitanteId != usuarioId)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        return MapearADetalle(solicitud);
    }

    public async Task<SolicitudDetalleDto> Crear(
        Guid tenantId, Guid usuarioId, CrearSolicitudDto peticion)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(peticion.Titulo) || peticion.Titulo.Length < 5 || peticion.Titulo.Length > 120)
        {
            errores["titulo"] = new[] { "El título debe tener entre 5 y 120 caracteres." };
        }

        if (string.IsNullOrWhiteSpace(peticion.Descripcion) || peticion.Descripcion.Length < 10 || peticion.Descripcion.Length > 4000)
        {
            errores["descripcion"] = new[] { "La descripción debe tener entre 10 y 4000 caracteres." };
        }

        if (!Enum.TryParse<PrioridadSolicitud>(peticion.Prioridad, out var prioridad))
        {
            errores["prioridad"] = new[] { "La prioridad no es válida." };
        }

        var categoria = await _db.Categorias
            .FirstOrDefaultAsync(c => c.Id == peticion.CategoriaId && c.TenantId == tenantId && c.Activo);

        if (categoria is null)
        {
            errores["categoriaId"] = new[] { "La categoría no existe o no pertenece a tu organización." };
        }

        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }

        var ahora = DateTime.UtcNow;
        string codigo = await GenerarCodigo(tenantId, ahora.Year);

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = codigo,
            Titulo = peticion.Titulo,
            Descripcion = peticion.Descripcion,
            CategoriaId = categoria!.Id,
            Prioridad = prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = usuarioId,
            FechaCreacion = ahora,
            FechaLimiteSla = CalculadoraSla.CalcularFechaLimite(ahora, categoria.SlaHoras, prioridad),
        };

        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync();

        return await Obtener(tenantId, usuarioId, "Admin", solicitud.Id);
    }

    public async Task<SolicitudDetalleDto> Editar(
        Guid tenantId, Guid usuarioId, string rol, Guid solicitudId, EditarSolicitudDto peticion)
    {
        var solicitud = await _db.Solicitudes
            .Include(s => s.Categoria)
            .FirstOrDefaultAsync(s => s.Id == solicitudId && s.TenantId == tenantId);

        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (rol == "Solicitante")
        {
            if (solicitud.SolicitanteId != usuarioId)
            {
                throw new RecursoNoEncontradoException("La solicitud no existe.");
            }

            if (solicitud.Estado != EstadoSolicitud.Nueva)
            {
                throw new OperacionNoPermitidaException("Solo puedes editar solicitudes en estado Nueva.");
            }
        }

        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(peticion.Titulo) || peticion.Titulo.Length < 5 || peticion.Titulo.Length > 120)
        {
            errores["titulo"] = new[] { "El título debe tener entre 5 y 120 caracteres." };
        }

        if (string.IsNullOrWhiteSpace(peticion.Descripcion) || peticion.Descripcion.Length < 10 || peticion.Descripcion.Length > 4000)
        {
            errores["descripcion"] = new[] { "La descripción debe tener entre 10 y 4000 caracteres." };
        }

        if (!Enum.TryParse<PrioridadSolicitud>(peticion.Prioridad, out var prioridad))
        {
            errores["prioridad"] = new[] { "La prioridad no es válida." };
        }

        var categoria = await _db.Categorias
            .FirstOrDefaultAsync(c => c.Id == peticion.CategoriaId && c.TenantId == tenantId && c.Activo);

        if (categoria is null)
        {
            errores["categoriaId"] = new[] { "La categoría no existe o no pertenece a tu organización." };
        }

        if (errores.Count > 0)
        {
            throw new ValidacionException(errores);
        }

        bool cambioPrioridadOCategoria = solicitud.Prioridad != prioridad || solicitud.CategoriaId != categoria!.Id;

        solicitud.Titulo = peticion.Titulo;
        solicitud.Descripcion = peticion.Descripcion;
        solicitud.CategoriaId = categoria!.Id;
        solicitud.Prioridad = prioridad;

        if (cambioPrioridadOCategoria && solicitud.Estado != EstadoSolicitud.Resuelta)
        {
            solicitud.FechaLimiteSla = CalculadoraSla.CalcularFechaLimite(
                solicitud.FechaCreacion, categoria.SlaHoras, prioridad);
        }

        await _db.SaveChangesAsync();

        return await Obtener(tenantId, usuarioId, rol, solicitud.Id);
    }

    public async Task<SolicitudDetalleDto> EjecutarTransicion(
        Guid tenantId, Guid usuarioId, string rol, Guid solicitudId, TransicionRequestDto peticion)
    {
        var solicitud = await _db.Solicitudes
            .FirstOrDefaultAsync(s => s.Id == solicitudId && s.TenantId == tenantId);

        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (rol == "Solicitante" && solicitud.SolicitanteId != usuarioId)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        VerificarPermisoDeAccion(rol, peticion.Accion, solicitud.SolicitanteId, usuarioId);

        var estadoSiguiente = MaquinaEstados.AplicarTransicion(solicitud.Estado, peticion.Accion);

        switch (peticion.Accion)
        {
            case "asignar":
                await ValidarYAsignarAgente(tenantId, solicitud, peticion.AgenteId);
                break;

            case "resolver":
                if (string.IsNullOrWhiteSpace(peticion.Motivo) || peticion.Motivo.Length < 20)
                {
                    throw new MotivoRequeridoException("El motivo de resolución debe tener al menos 20 caracteres.");
                }
                solicitud.MotivoResolucion = peticion.Motivo;
                solicitud.FechaResolucion = DateTime.UtcNow;
                break;

            case "cancelar":
                if (string.IsNullOrWhiteSpace(peticion.Motivo) || peticion.Motivo.Length < 10)
                {
                    throw new MotivoRequeridoException("El motivo de cancelación debe tener al menos 10 caracteres.");
                }
                solicitud.MotivoCancelacion = peticion.Motivo;
                break;
        }

        solicitud.Estado = estadoSiguiente;
        await _db.SaveChangesAsync();

        return await Obtener(tenantId, usuarioId, rol, solicitud.Id);
    }

    private static void VerificarPermisoDeAccion(string rol, string accion, Guid solicitanteId, Guid usuarioId)
    {
        bool esElSolicitante = solicitanteId == usuarioId;

        bool permitido = (rol, accion) switch
        {
            ("Admin", _) => true,
            ("Agente", "cancelar") => false,
            ("Agente", _) => true,
            ("Solicitante", "cerrar") => esElSolicitante,
            ("Solicitante", _) => false,
            _ => false,
        };

        if (!permitido)
        {
            throw new OperacionNoPermitidaException($"Tu rol no permite ejecutar la acción '{accion}'.");
        }
    }

    private async Task ValidarYAsignarAgente(Guid tenantId, Solicitud solicitud, Guid? agenteId)
    {
        if (!agenteId.HasValue)
        {
            throw new AgenteInvalidoException("Debes indicar un agenteId para asignar la solicitud.");
        }

        var agente = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == agenteId.Value);

        bool valido = agente is not null
            && agente.Activo
            && agente.TenantId == tenantId
            && (agente.Rol == RolUsuario.Agente || agente.Rol == RolUsuario.Admin);

        if (!valido)
        {
            throw new AgenteInvalidoException("El agente indicado no existe, está inactivo, no pertenece a tu organización o no tiene el rol adecuado.");
        }

        solicitud.AgenteId = agenteId.Value;
    }

    private async Task<string> GenerarCodigo(Guid tenantId, int anio)
    {
        int cantidadExistente = await _db.Solicitudes
            .CountAsync(s => s.TenantId == tenantId && s.FechaCreacion.Year == anio);

        int siguienteCorrelativo = cantidadExistente + 1;

        return $"SOL-{anio}-{siguienteCorrelativo:D5}";
    }

    private static SolicitudDetalleDto MapearADetalle(Solicitud s)
    {
        var ahora = DateTime.UtcNow;

        return new SolicitudDetalleDto(
            s.Id, s.Codigo, s.Titulo, s.Descripcion, s.Estado.ToString(), s.Prioridad.ToString(),
            new CategoriaResumenDto(s.Categoria!.Id, s.Categoria.Nombre),
            s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
            new UsuarioResumenDto(s.Solicitante!.Id, s.Solicitante.Nombre),
            s.FechaCreacion, s.FechaLimiteSla, s.FechaResolucion,
            s.MotivoResolucion, s.MotivoCancelacion,
            CalculadoraSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora));
    }
}