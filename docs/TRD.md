# Technical Requirements Document (TRD) - Monetra

## 1. Stack Tecnológico y Arquitectura
- **Frontend / Mobile App:** Android Nativo con Kotlin.
  - **Persistencia Local:** Room Database.
  - **Arquitectura Mobile:** MVVM (Model-View-ViewModel) + Clean Architecture.
- **Backend Service:** C# / .NET 8 (Web API).
  - **Framework:** ASP.NET Core REST API.
  - **Validación de Datos:** FluentValidation + Data Annotations.
  - **ORM:** Entity Framework Core (EF Core 8).
- **Arquitectura de Software:** Backend-for-Frontend (BFF) con API REST desacoplada.

```
[Android Kotlin (Room DB)] <---> [BFF C# .NET 8 API] <---> [Azure SQL DB / APIs Externas]
```

---

## 2. Modelo de Datos y Persistencia
- **Motor de BD:** Azure SQL Database (Tier gratuito / Estudiante) / MySQL Relacional (SQLite / LocalDB para desarrollo local).
- **Estrategia de Migraciones:** Entity Framework Core Code-First Migrations (`dotnet ef migrations add`).
- **Diagrama Entidad-Relación (Estructura Tabular Core):**
  - `Users` (Id, Name, Email, PasswordHash, RoleId, CreatedAt)
  - `Roles` (Id, Name) [Usuario, Administrador, Soporte, Auditor]
  - `Accounts` (Id, UserId, Name, CurrencyCode [USD/COP], CurrentBalance)
  - `Transactions` (Id, AccountId, CategoryId, Amount, Type [Income/Expense], Date, Description)
  - `Categories` (Id, Name, Icon, IsDefault)
  - `Debts` (Id, UserId, Creditor, TotalAmount, InterestRate, DueDate)
  - `SavingsGoals` (Id, UserId, Name, TargetAmount, CurrentAmount, Deadline)
  - `Subscriptions` (Id, UserId, Name, Amount, BillingCycle, NextPaymentDate)

---

## 3. Integración de APIs y Seguridad
- **APIs Externas:**
  - **Divisas y Mercado:** Yahoo Finance API / Alpha Vantage (Tier gratuito).
  - **IA Sugerencias:** Servicio interno o integración via API REST HTTP cliente en .NET.
- **Protección de Credenciales:**
  - Patrón BFF: La aplicación Android interactúa únicamente con la API de .NET.
  - El backend gestiona y oculta todas las API Keys mediante `appsettings.json` (no versionado) y Azure Key Vault / Environment Variables.
  - Autenticación mediante tokens JWT (JSON Web Tokens) con cifrado HMAC-SHA256.

---

## 4. Estrategia de Pruebas y Robustez
- **Pruebas de Integración y API:** Pruebas automatizadas en C# utilizando `Microsoft.AspNetCore.Mvc.Testing` y xUnit para validar contratos de respuesta HTTP.
- **Pruebas Unitarias:** xUnit + Moq para controladores y lógica de conversión multimoneda en el backend; JUnit + Mockk para la capa de repositorios en Kotlin.
- **Validación de DTOs:** Integración de `FluentValidation` en los middleware de ASP.NET Core para interceptar peticiones malformadas antes de llegar a los controladores.

---

## 5. Despliegue, Infraestructura y Observabilidad
- **Alojamiento Cloud:** **Azure App Service (Free Tier F1)** mediante cuenta Azure for Students.
- **Base de Datos Cloud:** **Azure SQL Database (Free Tier)**.
- **Telemetría y Logging:**
  - Implementación de **Serilog** en C# .NET para estructuración de logs en formato JSON.
  - Integración con **Azure Application Insights** (capa gratuita) para monitoreo de excepciones y tiempos de respuesta.

