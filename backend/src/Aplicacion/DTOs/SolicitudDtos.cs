namespace Aplicacion.DTOs;

public record CategoriaResumenDto(Guid Id, string Nombre);
public record AgenteResumenDto(Guid Id, string Nombre);
public record UsuarioResumenDto(Guid Id, string Nombre);

public record SolicitudListItemDto(
    Guid Id, string Codigo, string Titulo, string Estado, string Prioridad,
    CategoriaResumenDto Categoria, AgenteResumenDto? Agente,
    DateTime FechaCreacion, DateTime FechaLimiteSla, bool Vencida);

public record SolicitudListResponseDto(
    List<SolicitudListItemDto> Items, int Page, int PageSize, int Total, int TotalPaginas);

public record SolicitudDetalleDto(
    Guid Id, string Codigo, string Titulo, string Descripcion, string Estado, string Prioridad,
    CategoriaResumenDto Categoria, AgenteResumenDto? Agente, UsuarioResumenDto Solicitante,
    DateTime FechaCreacion, DateTime FechaLimiteSla, DateTime? FechaResolucion,
    string? MotivoResolucion, string? MotivoCancelacion, bool Vencida);

public record CrearSolicitudDto(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);

public record EditarSolicitudDto(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);

public record TransicionRequestDto(string Accion, Guid? AgenteId, string? Motivo);

public record CategoriaDto(Guid Id, string Nombre, int SlaHoras);