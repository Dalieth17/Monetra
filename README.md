# Monetra 💰📱

Monetra es una aplicación de finanzas personales diseñada para brindar soberanía y claridad total sobre el flujo de dinero. Permite a los usuarios registrar y auditar sus ingresos, egresos, presupuestos, deudas, suscripciones y metas de ahorro con soporte multimoneda (efectivo, pesos COP, dólares USD), integración con datos del mercado financiero y categorización inteligente asistida por IA.

---

## 🏛 Arquitectura del Sistema

```
[Android Kotlin (Room DB - Offline First)] ─┐
                                            ├─▶ [BFF C# .NET 8 Web API] ─▶ [MySQL 8]
[Sitio Web (SPA) servido por nginx] ────────┘            │
                                                         └─▶ [Yahoo Finance / Divisas]
```

- **Backend:** C# / .NET 8 REST Web API con Clean Architecture (Domain, Application, Infrastructure, API).
- **Base de Datos:** MySQL 8 con Entity Framework Core 8 (migraciones Code-First aplicadas al arranque).
- **Sitio Web:** SPA en React + Vite, servida por nginx (que además hace *reverse proxy* de `/api` hacia la API). Diseño responsivo con barra lateral en escritorio y navegación inferior en móvil.
- **Frontend Móvil:** Android Nativo (Kotlin) con Room Database (Offline-first) y arquitectura MVVM.
- **Seguridad:** Autenticación JWT HMAC-SHA256 y control de acceso basado en roles (RBAC: Usuario, Administrador, Soporte, Auditor).

---

## 📁 Estructura del Repositorio

```
Monetra/
├── docker-compose.yml       # Orquesta los 3 servicios (db, api, web)
├── .env.example             # Variables de entorno de ejemplo
├── backend/                 # Solución ASP.NET Core .NET 8
│   ├── Dockerfile           # Imagen multietapa de la API
│   ├── src/                 # Api · Application · Domain · Infrastructure
│   │   └── Monetra.Infrastructure/Persistence/Migrations/   # Migraciones EF Core
│   └── tests/               # Pruebas unitarias e integración (xUnit)
├── web/                     # Sitio web (React + Vite)
│   ├── Dockerfile           # Build de Node → nginx que sirve la SPA
│   ├── nginx.conf           # Proxy /api → contenedor api
│   ├── src/                 # Componentes, páginas y capa de datos (API real + demo)
│   └── abrir-web.sh         # Arranque local de un clic (modo demo)
├── android/                 # App Android nativa (Kotlin, Compose, Room, Retrofit)
├── database/
│   └── schema.sql           # DDL del esquema (exportado de las migraciones)
└── docs/                    # PRD, TRD, ROADMAP, USER_FLOW
```

---

## 🚀 Inicio rápido con Docker (recomendado)

Levanta **toda** la plataforma (base de datos + API + sitio web) con un solo comando. No necesitas instalar .NET ni MySQL localmente.

### Prerrequisitos
- Docker y Docker Compose

### Ejecución
```bash
cp .env.example .env     # opcional: ajusta contraseñas y puertos
docker-compose up --build
```

Al terminar:

| Servicio | URL | Descripción |
|----------|-----|-------------|
| Sitio web | http://localhost:8080 | Interfaz web (registro, login y toda la funcionalidad) |
| API + Swagger | http://localhost:5127 | API REST con documentación interactiva |
| MySQL | localhost:3306 | Base de datos (datos persistentes en un volumen) |

La API **aplica las migraciones automáticamente** al arrancar (espera a que MySQL esté listo), de modo que las tablas, los roles y las categorías quedan creados sin pasos manuales. Los datos persisten entre reinicios en el volumen `monetra_db_data`.

Para detener todo:
```bash
docker-compose down          # conserva los datos
docker-compose down -v       # elimina también la base de datos
```

### App Android con el backend en Docker
Con el stack levantado, la API está en `http://<IP-de-tu-PC>:5127`. En el emulador de Android se usa el alias `http://10.0.2.2:5127` (valor por defecto en `android/app/.../core/Constants.kt`). Para un dispositivo físico, reemplázalo por la IP de tu equipo.

---

## 🧪 Modo demostración (sin backend)

Tanto el sitio web como la app Android incluyen un botón **"Entrar en modo demostración"** en la pantalla de inicio: cargan datos de ejemplo en memoria y permiten recorrer toda la aplicación **sin necesidad de levantar el backend**.

- **Web:** ejecuta `./web/abrir-web.sh` (arranca el servidor de desarrollo de Vite y abre el navegador) y pulsa el botón.
- **Android:** ejecuta la app en el emulador y pulsa el botón.

---

## 🔧 Desarrollo sin Docker

### Backend
```bash
cd backend
dotnet restore
dotnet build
cd src/Monetra.Api
UseInMemoryDatabase=true dotnet run   # usa una BD en memoria, sin MySQL
```
Para usar MySQL real, omite `UseInMemoryDatabase=true` y configura la cadena de conexión en `appsettings.json`.

### Pruebas
```bash
cd backend
dotnet test            # 159 pruebas (unitarias + integración)
```

### Migraciones de base de datos
```bash
cd backend
dotnet ef migrations add <Nombre> -p src/Monetra.Infrastructure -s src/Monetra.Infrastructure -o Persistence/Migrations
dotnet ef migrations script -p src/Monetra.Infrastructure -s src/Monetra.Infrastructure -o ../database/schema.sql
```

### Sitio web (apuntando a una API local)
Si sirves la web por separado y la API corre en otro puerto, indícaselo desde la consola del navegador:
```js
localStorage.setItem('monetra_api_url', 'http://localhost:5127')
```
Por defecto la web usa el mismo origen (`/api`), que nginx redirige a la API en Docker.

---

## 📚 Documentación

- `docs/PRD.md` — Documento de Requisitos de Producto.
- `docs/TRD.md` — Documento de Requisitos Técnicos.
- `docs/ROADMAP.md` — Plan de Ejecución e Hitos.
- `docs/USER_FLOW.md` — Flujos de Usuario y Casos Límite.
