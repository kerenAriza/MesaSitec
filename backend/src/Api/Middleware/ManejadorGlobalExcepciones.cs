using Aplicacion.Excepciones;
using Aplicacion.Servicios;
using Dominio.Reglas;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

public class ManejadorGlobalExcepciones : IExceptionHandler
{
    private readonly ILogger<ManejadorGlobalExcepciones> _logger;

    public ManejadorGlobalExcepciones(ILogger<ManejadorGlobalExcepciones> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception excepcion,
        CancellationToken cancellationToken)
    {
        var problema = ConstruirProblemDetails(excepcion);

        if (problema is null)
        {
            // Excepción no reconocida: la registramos para depurar,
            // pero nunca dejamos que el stack trace llegue al cliente.
            _logger.LogError(excepcion, "Error no controlado.");

            problema = new ProblemDetails
            {
                Type = "https://mesasitec.local/errores/error-interno",
                Title = "Error interno del servidor",
                Status = 500,
                Detail = "Ocurrió un error inesperado."
            };
            problema.Extensions["codigo"] = "ERROR_INTERNO";
        }

        httpContext.Response.StatusCode = problema.Status ?? 500;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }

    private static ProblemDetails? ConstruirProblemDetails(Exception excepcion)
    {
        (int status, string codigo, string titulo, string tipo)? info = excepcion switch
        {
            CredencialesInvalidasException => (401, "NO_AUTENTICADO", "No autenticado", "no-autenticado"),
            OperacionNoPermitidaException => (403, "OPERACION_NO_PERMITIDA", "Operación no permitida", "operacion-no-permitida"),
            RecursoNoEncontradoException => (404, "RECURSO_NO_ENCONTRADO", "Recurso no encontrado", "recurso-no-encontrado"),
            TransicionInvalidaException => (409, "TRANSICION_INVALIDA", "Transición inválida", "transicion-invalida"),
            AgenteInvalidoException => (422, "AGENTE_INVALIDO", "Agente inválido", "agente-invalido"),
            MotivoRequeridoException => (422, "MOTIVO_REQUERIDO", "Motivo requerido", "motivo-requerido"),
            ValidacionException => (422, "VALIDACION", "Error de validación", "validacion"),
            ParametroInvalidoException => (400, "PARAMETRO_INVALIDO", "Parámetro inválido", "parametro-invalido"),
            _ => null,
        };

        if (info is null)
        {
            return null;
        }

        var problema = new ProblemDetails
        {
            Type = $"https://mesasitec.local/errores/{info.Value.tipo}",
            Title = info.Value.titulo,
            Status = info.Value.status,
            Detail = excepcion.Message,
        };
        problema.Extensions["codigo"] = info.Value.codigo;

        if (excepcion is ValidacionException validacion)
        {
            problema.Extensions["errores"] = validacion.Errores;
        }

        return problema;
    }
}