# Decisiones técnicas — MesaSitec

## Tres decisiones técnicas

**1. Controllers en vez de Minimal API.**
La plantilla de .NET 8 por defecto viene configurada para Minimal API (rutas definidas directo en `Program.cs`). La descarté porque con 9 endpoints y reglas de negocio complejas (máquina de estados, permisos por rol, cálculo de SLA), tener una clase Controller por recurso, con la lógica movida a una capa de Aplicación separada, se organiza y se prueba mejor que ir acumulando `app.MapGet/MapPost` en un solo archivo.

**2. Guardar los enums de EF Core como texto (`HasConversion<string>`) en vez de como número.**
Por defecto, EF Core guarda un enum como su valor numérico (`0`, `1`, `2`...). Lo cambié a texto porque, al abrir el archivo `.db` con una herramienta como DB Browser for SQLite para verificar los datos semilla, un valor como `"Agente"` es legible de inmediato; un `1` no dice nada sin volver al código a revisar el orden del enum.

**3. Fijar la versión de los paquetes `Microsoft.EntityFrameworkCore.*` en `8.0.11` en vez de dejar la última disponible.**
Al instalar sin especificar versión, NuGet trajo por defecto una versión pensada para .NET 10, incompatible con mi proyecto en .NET 8. Fijar la versión explícitamente evita ese conflicto y hace que el proyecto sea reproducible: cualquiera que lo instale de cero va a bajar exactamente la misma versión, no "la más reciente del día".

## Qué hice con ayuda de IA y qué escribí a mano

Usé Claude (Sonnet, vía chat) durante todo el desarrollo, en modo tutor paso a paso: para cada pieza (entidades, DbContext, JWT, máquina de estados, endpoints, vistas de Vue), Claude me explicaba el concepto nuevo y me daba el código; yo lo escribía/pegaba en mi editor, lo compilaba, y volvía con los errores reales que me salían para que me los explicara antes de seguir. No es código generado y aceptado a ciegas — cada archivo pasó por al menos un ciclo de "esto no compiló, por qué" que me obligó a entender qué estaba pasando debajo (inyección de dependencias, EF Core, el pipeline de middleware de ASP.NET, la reactividad de Vue). La estructura general del proyecto, el orden de los pasos y las explicaciones de cada concepto vinieron de Claude; la implementación, la depuración y las decisiones dentro de cada paso las fui aplicando yo.

## Qué haría distinto con una semana más

Refactorizaría `SolicitudesService` — quedó como una sola clase con demasiadas responsabilidades (listar, obtener, crear, editar, ejecutar transiciones, validar agente, generar código). Con más tiempo la dividiría en servicios más pequeños y específicos (por ejemplo, separar la validación de agente y la generación de código en sus propias clases), para que cada pieza sea más fácil de probar y de leer por separado.

También completaría el selector de agentes real en el modal de "asignar" del frontend (por ahora es un `<select>` vacío — la transición funciona correctamente desde el backend, probada vía Swagger, pero la interfaz no trae la lista de agentes porque el contrato no define un endpoint dedicado para listarlos), y agregaría más pruebas unitarias específicas para la tabla de permisos por rol (RN-03), que hoy solo está cubierta indirectamente a través de los endpoints.

## Dónde me atasqué y cómo lo resolví

Lo que más me costó entender fue JWT combinado con la inyección de dependencias de .NET — venía de Node/Express, donde el manejo de autenticación y de dependencias es mucho más manual y explícito. Entender por qué un servicio "recibe" sus dependencias en el constructor sin que nadie escriba `new` en ningún lado (el contenedor de DI de .NET se encarga solo) me tomó varias vueltas. Se complicó más cuando, después de generar el token, el endpoint `/me` me tiraba un `NullReferenceException` al leer los claims — resultó que ASP.NET Core renombra automáticamente algunos claims conocidos (como `sub`) a nombres largos y estándar al validar el token, así que mis búsquedas por el nombre corto no encontraban nada.lo que hice fue desactivar ese comportamiento (`MapInboundClaims = false`) para que los claims se quedaran exactamente con los nombres que yo les puse al generar el token.