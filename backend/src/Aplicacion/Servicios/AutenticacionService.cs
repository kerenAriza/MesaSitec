using Aplicacion.DTOs;
using Aplicacion.Excepciones;
using Aplicacion.Seguridad;
using Infraestructura.Datos;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Servicios;

public class AutenticacionService
{
    private readonly MesaSitecDbContext _db;
    private readonly IGeneradorTokenJwt _generadorToken;

    public AutenticacionService(MesaSitecDbContext db, IGeneradorTokenJwt generadorToken)
    {
        _db = db;
        _generadorToken = generadorToken;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto peticion)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == peticion.Email && u.Activo);

        if (usuario is null || usuario.Tenant is null || !usuario.Tenant.Activo)
        {
            throw new CredencialesInvalidasException();
        }

        bool passwordValido = BCrypt.Net.BCrypt.Verify(peticion.Password, usuario.PasswordHash);

        if (!passwordValido)
        {
            throw new CredencialesInvalidasException();
        }

        string token = _generadorToken.Generar(usuario, out int expiraEnSegundos);

        var usuarioDto = new UsuarioDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol.ToString(),
            usuario.TenantId,
            usuario.Tenant.Nombre);

        return new LoginResponseDto(token, expiraEnSegundos, usuarioDto);
    }
}