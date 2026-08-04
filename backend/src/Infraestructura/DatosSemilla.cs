using Dominio.Entidades;
using Infraestructura.Datos;
using Microsoft.Extensions.Configuration;

namespace Infraestructura.Semilla;

public static class DatosSemilla
{
    public static void Sembrar(MesaSitecDbContext db, IConfiguration configuracion)
    {
        // Si ya hay tenants, asumimos que la base ya fue sembrada antes.
        if (db.Tenants.Any())
        {
            return;
        }

        // Fecha base configurable, con valor por defecto según el enunciado.
        var fechaBaseTexto = configuracion["SEED_FECHA_BASE"] ?? "2026-01-15T08:00:00Z";
        var fechaBase = DateTime.Parse(
            fechaBaseTexto,
            null,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);

        // ---------- Tenants ----------
        var tenantNorte = new Tenant { Id = Guid.NewGuid(), Nombre = "Cooperativa Norte", Activo = true };
        var tenantSur = new Tenant { Id = Guid.NewGuid(), Nombre = "Bufete Sur", Activo = true };
        db.Tenants.AddRange(tenantNorte, tenantSur);

        // ---------- Usuarios ----------
        string hash = BCrypt.Net.BCrypt.HashPassword("Sitec.2026");

        var adminNorte = new Usuario { Id = Guid.NewGuid(), TenantId = tenantNorte.Id, Email = "admin@norte.test", PasswordHash = hash, Nombre = "Admin Norte", Rol = RolUsuario.Admin, Activo = true };
        var agente1Norte = new Usuario { Id = Guid.NewGuid(), TenantId = tenantNorte.Id, Email = "agente1@norte.test", PasswordHash = hash, Nombre = "Agente Uno Norte", Rol = RolUsuario.Agente, Activo = true };
        var agente2Norte = new Usuario { Id = Guid.NewGuid(), TenantId = tenantNorte.Id, Email = "agente2@norte.test", PasswordHash = hash, Nombre = "Agente Dos Norte", Rol = RolUsuario.Agente, Activo = true };
        var user1Norte = new Usuario { Id = Guid.NewGuid(), TenantId = tenantNorte.Id, Email = "user1@norte.test", PasswordHash = hash, Nombre = "Solicitante Uno Norte", Rol = RolUsuario.Solicitante, Activo = true };
        var user2Norte = new Usuario { Id = Guid.NewGuid(), TenantId = tenantNorte.Id, Email = "user2@norte.test", PasswordHash = hash, Nombre = "Solicitante Dos Norte", Rol = RolUsuario.Solicitante, Activo = true };

        var adminSur = new Usuario { Id = Guid.NewGuid(), TenantId = tenantSur.Id, Email = "admin@sur.test", PasswordHash = hash, Nombre = "Admin Sur", Rol = RolUsuario.Admin, Activo = true };
        var user1Sur = new Usuario { Id = Guid.NewGuid(), TenantId = tenantSur.Id, Email = "user1@sur.test", PasswordHash = hash, Nombre = "Solicitante Uno Sur", Rol = RolUsuario.Solicitante, Activo = true };

        db.Usuarios.AddRange(adminNorte, agente1Norte, agente2Norte, user1Norte, user2Norte, adminSur, user1Sur);

        // ---------- Categorías (en ambos tenants) ----------
        var categoriasBase = new (string Nombre, int SlaHoras)[]
        {
            ("Incidente", 8),
            ("Requerimiento", 40),
            ("Consulta", 24),
            ("Falla crítica", 4),
        };

        var categoriasNorte = categoriasBase.Select(c => new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = tenantNorte.Id,
            Nombre = c.Nombre,
            SlaHoras = c.SlaHoras,
            Activo = true
        }).ToList();

        var categoriasSur = categoriasBase.Select(c => new Categoria
        {
            Id = Guid.NewGuid(),
            TenantId = tenantSur.Id,
            Nombre = c.Nombre,
            SlaHoras = c.SlaHoras,
            Activo = true
        }).ToList();

        db.Categorias.AddRange(categoriasNorte);
        db.Categorias.AddRange(categoriasSur);

        // ---------- Solicitudes ----------
        CrearSolicitudesTenant(
            db, tenantNorte, categoriasNorte, fechaBase,
            solicitantes: new[] { user1Norte, user2Norte },
            agentes: new[] { agente1Norte, agente2Norte },
            cantidad: 25);

        CrearSolicitudesTenant(
            db, tenantSur, categoriasSur, fechaBase,
            solicitantes: new[] { user1Sur },
            agentes: new[] { adminSur },
            cantidad: 8);

        db.SaveChanges();
    }

    private static void CrearSolicitudesTenant(
        MesaSitecDbContext db,
        Tenant tenant,
        List<Categoria> categorias,
        DateTime fechaBase,
        Usuario[] solicitantes,
        Usuario[] agentes,
        int cantidad)
    {
        var estados = new[]
        {
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada
        };

        var prioridades = new[]
        {
            PrioridadSolicitud.Baja,
            PrioridadSolicitud.Media,
            PrioridadSolicitud.Alta,
            PrioridadSolicitud.Critica
        };

        var factorPrioridad = new Dictionary<PrioridadSolicitud, double>
        {
            [PrioridadSolicitud.Critica] = 0.5,
            [PrioridadSolicitud.Alta] = 0.75,
            [PrioridadSolicitud.Media] = 1.0,
            [PrioridadSolicitud.Baja] = 2.0,
        };

        int anio = fechaBase.Year;

        for (int i = 1; i <= cantidad; i++)
        {
            var estado = estados[i % estados.Length];
            var prioridad = prioridades[i % prioridades.Length];
            var categoria = categorias[i % categorias. Count];
            var solicitante = solicitantes[i % solicitantes.Length];

            var fechaCreacion = fechaBase.AddDays(-i);
            var fechaLimite = fechaCreacion.AddHours(categoria.SlaHoras * factorPrioridad[prioridad]);

            var solicitud = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Codigo = $"SOL-{anio}-{i:D5}",
                Titulo = $"Solicitud de prueba número {i}",
                Descripcion = $"Descripción de ejemplo generada por datos semilla para la solicitud número {i} de {tenant.Nombre}.",
                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = solicitante.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite,
            };

            if (estado != EstadoSolicitud.Nueva)
            {
                solicitud.AgenteId = agentes[i % agentes.Length].Id;
            }

            if (estado == EstadoSolicitud.Resuelta || estado == EstadoSolicitud.Cerrada)
            {
                solicitud.FechaResolucion = fechaCreacion.AddHours(1);
                solicitud.MotivoResolucion = $"Se atendió y resolvió la solicitud número {i} satisfactoriamente.";
            }

            if (estado == EstadoSolicitud.Cancelada)
            {
                solicitud.MotivoCancelacion = $"Solicitud cancelada de prueba número {i}.";
            }

            db.Solicitudes.Add(solicitud);
        }
    }
}