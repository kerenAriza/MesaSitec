using Dominio.Entidades;
using Dominio.Reglas;
using Xunit;

namespace Tests;

public class MaquinaEstadosTests
{
    [Fact]
    public void Nueva_Asignar_DebeQuedarEnAsignada()
    {
        var resultado = MaquinaEstados.AplicarTransicion(EstadoSolicitud.Nueva, "asignar");

        Assert.Equal(EstadoSolicitud.Asignada, resultado);
    }

    [Fact]
    public void Nueva_Resolver_DebeLanzarTransicionInvalida()
    {
        Assert.Throws<TransicionInvalidaException>(() =>
            MaquinaEstados.AplicarTransicion(EstadoSolicitud.Nueva, "resolver"));
    }

    [Fact]
    public void Resuelta_Cerrar_DebeQuedarEnCerrada()
    {
        var resultado = MaquinaEstados.AplicarTransicion(EstadoSolicitud.Resuelta, "cerrar");

        Assert.Equal(EstadoSolicitud.Cerrada, resultado);
    }

    [Fact]
    public void Resuelta_Reabrir_DebeQuedarEnProceso()
    {
        var resultado = MaquinaEstados.AplicarTransicion(EstadoSolicitud.Resuelta, "reabrir");

        Assert.Equal(EstadoSolicitud.EnProceso, resultado);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Cerrada, "cerrar")]
    [InlineData(EstadoSolicitud.Cerrada, "reabrir")]
    [InlineData(EstadoSolicitud.Cancelada, "asignar")]
    public void EstadosFinales_CualquierAccion_DebeLanzarTransicionInvalida(EstadoSolicitud estado, string accion)
    {
        Assert.Throws<TransicionInvalidaException>(() =>
            MaquinaEstados.AplicarTransicion(estado, accion));
    }
}