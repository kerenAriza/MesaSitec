using Dominio.Entidades;

namespace Dominio.Reglas;

public static class CalculadoraSla
{
    private static readonly Dictionary<PrioridadSolicitud, double> FactorPrioridad = new()
    {
        [PrioridadSolicitud.Critica] = 0.5,
        [PrioridadSolicitud.Alta] = 0.75,
        [PrioridadSolicitud.Media] = 1.0,
        [PrioridadSolicitud.Baja] = 2.0,
    };

    public static DateTime CalcularFechaLimite(
        DateTime fechaCreacion,
        int slaHorasCategoria,
        PrioridadSolicitud prioridad)
    {
        double factor = FactorPrioridad[prioridad];
        double horasAjustadas = slaHorasCategoria * factor;
        return fechaCreacion.AddHours(horasAjustadas);
    }

    public static bool EstaVencida(DateTime fechaLimiteSla, EstadoSolicitud estado, DateTime ahora)
    {
        bool esEstadoFinal = estado == EstadoSolicitud.Resuelta
            || estado == EstadoSolicitud.Cerrada
            || estado == EstadoSolicitud.Cancelada;

        return fechaLimiteSla < ahora && !esEstadoFinal;
    }
}