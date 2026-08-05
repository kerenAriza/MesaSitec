namespace Api.Extensions;
using System.Security.Claims;
public static class ClaimsPrincipalExtensions
{
    public static Guid ObtenerTenantId(this ClaimsPrincipal usuario)
    {
        return Guid.Parse(usuario.FindFirst("tenantId")!.Value);
    }

    public static Guid ObtenerUsuarioId(this ClaimsPrincipal usuario)
    {
        return Guid.Parse(usuario.FindFirst("sub")!.Value);
    }

    public static string ObtenerRol(this ClaimsPrincipal usuario)
    {
        return usuario.FindFirst("rol")!.Value;
    }
}