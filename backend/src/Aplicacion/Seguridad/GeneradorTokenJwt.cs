using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aplicacion.Seguridad;
using Dominio.Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infraestructura.Seguridad;

public class GeneradorTokenJwt : IGeneradorTokenJwt
{
    private readonly IConfiguration _configuracion;

    public GeneradorTokenJwt(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public string Generar(Usuario usuario, out int expiraEnSegundos)
    {
        int horas = int.Parse(_configuracion["Jwt:ExpiracionHoras"] ?? "8");
        expiraEnSegundos = horas * 3600;

        var claims = new[]
        {
            new Claim("sub", usuario.Id.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", usuario.Rol.ToString()),
            new Claim("email", usuario.Email),
        };

        string secreto = Environment.GetEnvironmentVariable("JWT_SECRETO")
            ?? _configuracion["Jwt:SecretoDesarrollo"]
            ?? throw new InvalidOperationException("No hay secreto JWT configurado.");

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(horas),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}