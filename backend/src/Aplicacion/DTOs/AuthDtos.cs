namespace Aplicacion.DTOs;

public record LoginRequestDto(string Email, string Password);

public record UsuarioDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    Guid TenantId,
    string TenantNombre);

public record LoginResponseDto(
    string AccessToken,
    int ExpiraEn,
    UsuarioDto Usuario);