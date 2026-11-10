# Monetra 💰📱

Monetra es una aplicación móvil de finanzas personales diseñada para brindar soberanía y claridad total sobre el flujo de dinero. Permite a los usuarios registrar y auditar sus ingresos, egresos, presupuestos, deudas, suscripciones y metas de ahorro con soporte multimoneda (efectivo, pesos COP, dólares USD), integración con datos del mercado financiero y categorización inteligente asistida por IA.

---

## 🏛 Arquitectura del Sistema

```
[Android Kotlin (Room DB - Offline First)] <---> [BFF C# .NET 8 Web API] <---> [MySQL / Azure SQL DB]
                                                                        <---> [Yahoo Finance / Divisas]
```

- **Backend:** C# / .NET 8 REST Web API con Clean Architecture (Domain, Application, Infrastructure, API).
- **Base de Datos:** MySQL / Azure SQL con Entity Framework Core 8 Code-First.
- **Frontend Móvil:** Android Nativo (Kotlin) con Room Database (Offline-first) y arquitectura MVVM.
- **Seguridad:** Autenticación JWT HMAC-SHA256 y control de acceso basado en roles (RBAC: Usuario, Administrador, Soporte, Auditor).

---

## 📁 Estructura del Repositorio

- `backend/` - Solución ASP.NET Core .NET 8 (API REST, Lógica de Dominio, Infraestructura y Pruebas).
- `android/` - Proyecto Android Nativo en Kotlin (Room DB, MVVM, Retrofit, Jetpack UI).
- `docs/` - Especificaciones técnicas y de producto:
  - `PRD.md`: Documento de Requisitos de Producto.
  - `TRD.md`: Documento de Requisitos Técnicos.
  - `ROADMAP.md`: Plan de Ejecución e Hitos.
  - `USER_FLOW.md`: Diagrama de Flujos de Usuario y Casos Límite.

---

## 🚀 Inicio Rápido (Backend)

### Prerrequisitos
- .NET 8 SDK
- MySQL Server (Base de datos `monetra_db`)

### Ejecución
```bash
cd backend
dotnet restore
dotnet build
cd src/Monetra.Api
dotnet run
```
La API estará disponible con documentación Swagger interactiva.

