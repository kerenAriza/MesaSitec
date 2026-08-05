namespace Aplicacion.Excepciones;
public class CredencialesInvalidasException : Exception { }

public class ParametroInvalidoException : Exception
{
    public ParametroInvalidoException(string mensaje) : base(mensaje) { }
}

public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
}

public class OperacionNoPermitidaException : Exception
{
    public OperacionNoPermitidaException(string mensaje) : base(mensaje) { }
}

public class AgenteInvalidoException : Exception
{
    public AgenteInvalidoException(string mensaje) : base(mensaje) { }
}

public class MotivoRequeridoException : Exception
{
    public MotivoRequeridoException(string mensaje) : base(mensaje) { }
}

public class ValidacionException : Exception
{
    public Dictionary<string, string[]> Errores { get; }

    public ValidacionException(Dictionary<string, string[]> errores) : base("Error de validación.")
    {
        Errores = errores;
    }
}
