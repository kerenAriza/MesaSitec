using Dominio.Entidades;
using Dominio.Reglas;
using Xunit;

namespace Tests;

public class CalculadoraSlaTests
{
    [Fact]
    public void CategoriaIncidente_PrioridadCritica_LimiteA4Horas()
    {
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var limite = CalculadoraSla.CalcularFechaLimite(fechaCreacion, slaHorasCategoria: 8, PrioridadSolicitud.Critica);

        // Ejemplo exacto del enunciado: Incidente (8h) x Critica (0.5) = 4 horas.
        Assert.Equal(fechaCreacion.AddHours(4), limite);
    }

    [Fact]
    public void CategoriaConsulta_PrioridadBaja_LimiteA48Horas()
    {
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var limite = CalculadoraSla.CalcularFechaLimite(fechaCreacion, slaHorasCategoria: 24, PrioridadSolicitud.Baja);

        // Ejemplo exacto del enunciado: Consulta (24h) x Baja (2.0) = 48 horas.
        Assert.Equal(fechaCreacion.AddHours(48), limite);
    }

    [Fact]
    public void FechaLimiteEnElPasado_EstadoNoFinal_EstaVencida()
    {
        var ahora = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechaLimite = ahora.AddDays(-1);

        bool vencida = CalculadoraSla.EstaVencida(fechaLimite, EstadoSolicitud.EnProceso, ahora);

        Assert.True(vencida);
    }

    [Fact]
    public void FechaLimiteEnElPasado_EstadoResuelta_NoEstaVencida()
    {
        var ahora = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechaLimite = ahora.AddDays(-1);

        bool vencida = CalculadoraSla.EstaVencida(fechaLimite, EstadoSolicitud.Resuelta, ahora);

        Assert.False(vencida);
    }
}