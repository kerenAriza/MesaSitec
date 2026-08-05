using System.Text;
using Aplicacion.Seguridad;
using Aplicacion.Servicios;
using Infraestructura.Datos;
using Infraestructura.Semilla;
using Infraestructura.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<Aplicacion.Servicios.CategoriasService>();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token con el formato: Bearer {tu token}"
    });

    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//  generador de tokens
builder.Services.AddScoped<IGeneradorTokenJwt, GeneradorTokenJwt>();

// servicio de autenticación
builder.Services.AddScoped<AutenticacionService>();


//  servicio de solicitudes
builder.Services.AddScoped<SolicitudesService>();

builder.Services.AddExceptionHandler<Api.Middleware.ManejadorGlobalExcepciones>();
builder.Services.AddProblemDetails();

string secreto = Environment.GetEnvironmentVariable("JWT_SECRETO")
    ?? builder.Configuration["Jwt:SecretoDesarrollo"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false; // ← nueva línea

        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto))
        };
    });

builder.Services.AddAuthorization();

// CORS para el frontend (sección 5.1 del enunciado)
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("FrontendPolicy", politica =>
    {
        politica.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});


var app = builder.Build();
app.UseExceptionHandler(); // middleware de manejo global de excepciones

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.Migrate();
    DatosSemilla.Sembrar(db, app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
