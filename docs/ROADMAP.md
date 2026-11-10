# Development Roadmap & Execution Plan - Monetra

## 1. Definición del MVP vs. Iteraciones Futuras

### Fase 1: MVP (Producto Mínimo Viable)
- Registro e Inicio de Sesión de Usuarios.
- Gestión de Cuentas Multimoneda (USD y COP en efectivo/banco).
- CRUD completo de Transacciones (Ingresos y Egresos) con almacenamiento offline (Room) y sync en Azure.
- Dashboard básico con balance general consolidado.
- Módulo de Suscripciones y Metas de Ahorro simples.

### Fase 2: Métricas, IA y Deudas (Iteración Corta)
- Módulo de Deudas con cálculo de amortizaciones e intereses.
- Clasificación de gastos asistida por IA (sugerencia automática de categoría).
- Integración de API de divisas para actualización en tiempo real de la tasa de cambio.

### Fase 3: Mercado Financiero y Auditoría Avanzada (Fase Extendida)
- Visualización gráfica del mercado de acciones.
- Panel de administración, soporte y auditoría (RBAC extendido).
- Reportes exportables en PDF/Excel.

---

## 2. Hitos Secuenciales (Milestones)

### Hito 1: Fundamento Backend C# & Base de Datos
- **Entregables:** Proyecto .NET 8 Web API configurado, entidades EF Core, migraciones creadas, autenticación JWT funcional y despliegue inicial en Azure App Service.

### Hito 2: App Android Nativa (Core Offline-First)
- **Entregables:** Proyecto Kotlin configurado con Room Database, arquitectura MVVM, pantallas de Login, Registro y Dashboard principal de balances.

### Hito 3: Sincronización y Módulos Financieros
- **Entregables:** Conexión HTTP Retrofit (Android a .NET API), módulo de Metas de Ahorro, Suscripciones y Deudas.

### Hito 4: Inteligencia Artificial, Servicios de Mercado & QA
- **Entregables:** Integración con API de divisas, módulo de IA para categorización, pruebas de integración xUnit aprobadas y validación final.

---

## 3. Puertas de Calidad (Validation Gates)
- **Gate 1 (Build Clean):** Todo commit debe compilar sin advertencias críticas en C# y Kotlin.
- **Gate 2 (Test Pass):** Las pruebas de integración en el backend deben pasar al 100% antes de desplegar cambios a Azure.
- **Human-in-the-Loop (Luiger):** Validación visual y funcional en dispositivo/emulador Android al finalizar cada Hito.

---

## 4. Granularidad de Tareas
Las tareas se estructurarán como **Micro-Tareas Atómicas** (ej. "Crear endpoint POST /api/transactions con validación FluentValidation", "Diseñar tabla Room `transaction_table` en Kotlin").

