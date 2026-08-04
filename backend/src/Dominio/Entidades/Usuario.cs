namespace Dominio.Entidades;

public enum RolUsuario
{
    Admin,
    Agente,
    Solicitante
}

public class Usuario
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;

    public Tenant? Tenant { get; set; }
}