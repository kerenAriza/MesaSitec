using Dominio.Entidades;

namespace Aplicacion.Seguridad;

public interface IGeneradorTokenJwt
{
    string Generar(Usuario usuario, out int expiraEnSegundos);
}