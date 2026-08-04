using Dominio.Entidades;

namespace Dominio.Reglas;

public class TransicionInvalidaException : Exception
{
    public TransicionInvalidaException(string mensaje) : base(mensaje) { }
}

public static class MaquinaEstados
{
    // Mapa: estado actual -> acción -> estado siguiente.
    // Es exactamente la tabla de la sección RN-02 del enunciado, en código.
    private static readonly Dictionary<EstadoSolicitud, Dictionary<string, EstadoSolicitud>> Transiciones = new()
    {
        [EstadoSolicitud.Nueva] = new()
        {
            ["asignar"] = EstadoSolicitud.Asignada,
            ["cancelar"] = EstadoSolicitud.Cancelada,
        },
        [EstadoSolicitud.Asignada] = new()
        {
            ["iniciar"] = EstadoSolicitud.EnProceso,
            ["asignar"] = EstadoSolicitud.Asignada, // reasignar a otro agente
            ["cancelar"] = EstadoSolicitud.Cancelada,
        },
        [EstadoSolicitud.EnProceso] = new()
        {
            ["resolver"] = EstadoSolicitud.Resuelta,
            ["asignar"] = EstadoSolicitud.Asignada,
            ["cancelar"] = EstadoSolicitud.Cancelada,
        },
        [EstadoSolicitud.Resuelta] = new()
        {
            ["cerrar"] = EstadoSolicitud.Cerrada,
            ["reabrir"] = EstadoSolicitud.EnProceso,
        },
        [EstadoSolicitud.Cerrada] = new(),     // estado final, sin acciones
        [EstadoSolicitud.Cancelada] = new(),   // estado final, sin acciones
    };

    public static EstadoSolicitud AplicarTransicion(EstadoSolicitud estadoActual, string accion)
    {
        if (!Transiciones.TryGetValue(estadoActual, out var accionesPermitidas)
            || !accionesPermitidas.TryGetValue(accion, out var estadoSiguiente))
        {
            throw new TransicionInvalidaException(
                $"No se puede aplicar '{accion}' sobre una solicitud en estado '{estadoActual}'.");
        }

        return estadoSiguiente;
    }
}