# MesaSitec

Sistema de mesa de servicio multi-tenant. Backend en .NET 8 + EF Core + SQLite, frontend en Vue 3 + TypeScript + Pinia.

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 o superior)
- La herramienta `dotnet-ef` instalada globalmente:
```bash
  dotnet tool install --global dotnet-ef --version 8.0.11
```

## Cómo levantar el proyecto

### 1. Backend

```bash
cd backend
dotnet run --project src/Api
```

La primera vez que arranca, la base de datos SQLite se crea, se migra y se siembra con datos de ejemplo automáticamente — no requiere ningún paso manual.

La Api queda disponible en `http://localhost:5080`, con Swagger en `http://localhost:5080/swagger`.

### 2. Frontend

En otra terminal:

```bash
cd frontend
npm install
npm run dev
```

El frontend queda disponible en `http://localhost:5173`.

## Credenciales de prueba

Todos los usuarios semilla usan la contraseña: `Sitec.2026`

| Email | Organización | Rol |
|---|---|---|
| admin@norte.test | Cooperativa Norte | Admin |
| agente1@norte.test | Cooperativa Norte | Agente |
| agente2@norte.test | Cooperativa Norte | Agente |
| user1@norte.test | Cooperativa Norte | Solicitante |
| user2@norte.test | Cooperativa Norte | Solicitante |
| admin@sur.test | Bufete Sur | Admin |
| user1@sur.test | Bufete Sur | Solicitante |

## Pruebas

```bash
cd backend
dotnet test
```

## Qué está implementado

- Los 9 endpoints del contrato, con autenticación JWT, aislamiento multi-tenant (RN-01), máquina de estados (RN-02), permisos por rol (RN-03), cálculo de SLA (RN-04), validación de agente (RN-05), motivos de cierre (RN-06) y generación de código (RN-07).
- Manejador global de excepciones — ninguna excepción no controlada llega al cliente como 500 con stack trace.
- Datos semilla automáticos y reproducibles (basados en `SEED_FECHA_BASE`, no en la fecha actual).
- Pruebas unitarias de la máquina de estados y el cálculo de SLA.
- Las 5 vistas del frontend (login, listado con filtros/paginación/búsqueda server-side, detalle, formulario de creación/edición), con los tres estados de carga (cargando/vacío/error) y los botones de acción visibles solo según estado + rol (sección 7.5).
- Cliente HTTP centralizado con inyección de token y redirección a `/login` ante un 401.

## Qué NO está implementado / está simplificado

- El selector de agentes en el modal de "asignar" (dentro del detalle de una solicitud) no trae la lista real de agentes del backend — no existe un endpoint dedicado a listar usuarios/agentes en el contrato original, así que quedó como un `<select>` vacío. La transición `asignar` sí funciona correctamente desde el backend (probada vía Swagger), pero desde la interfaz no hay forma de seleccionar un agente todavía.
- El Diseño no llegue a completar por lo cual quedo con un diseño basico.
- Llegue a porbar l¿nada mas el flujo solo de Cooperativa Norte.

## Variables de entorno

Ver `.env.example`. El secreto JWT tiene un valor de desarrollo por defecto en `appsettings.json`, pero en producción debe sobreescribirse con la variable de entorno `JWT_SECRETO`.