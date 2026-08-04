using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;

namespace Infraestructura.Datos;

public class MesaSitecDbContext : DbContext
{
    public MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Solicitud> Solicitudes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Email único a nivel global (RN de Usuario)
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Solicitud tiene DOS relaciones distintas con Usuario:
        // una como Solicitante, otra como Agente.
        // Hay que decirle a EF Core explícitamente cuál FK va con cuál.
        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Solicitante)
            .WithMany()
            .HasForeignKey(s => s.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Agente)
            .WithMany()
            .HasForeignKey(s => s.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Guardamos los enums como texto en vez de número,
        // para que la base de datos sea legible si la abres directo.
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Rol)
            .HasConversion<string>();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Prioridad)
            .HasConversion<string>();

        modelBuilder.Entity<Solicitud>()
            .Property(s => s.Estado)
            .HasConversion<string>();
    }
}