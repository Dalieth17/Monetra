# Product Requirements Document (PRD) - Monetra

## 1. Visión General del Producto y Objetivos de Negocio
**Nombre del Proyecto:** Monetra  
**Propuesta de Valor:** Monetra es una aplicación móvil de finanzas personales diseñada para brindar soberanía y claridad total sobre el flujo de dinero. Permite a los usuarios registrar y auditar sus ingresos, egresos, presupuestos, deudas, suscripciones y metas de ahorro con soporte multimoneda (efectivo, pesos, dólares), integración con datos del mercado financiero (divisas y acciones) y categorización inteligente asistida por IA.

### KPIs y Métricas de Éxito
- **Tasa de Retención de Usuarios (Retention Rate):** >60% de retención activa al día 30 post-registro.
- **Frecuencia de Registro de Transacciones:** Promedio de al menos 3 registros por usuario activo a la semana.
- **Precisión de Clasificación de IA:** >85% de sugerencias de categoría aceptadas sin modificación manual por el usuario.
- **Tasa de Cumplimiento de Metas:** Incremento del 20% en el número de usuarios que completan sus metas de ahorro activas dentro del plazo trazado.

---

## 2. Roles de Usuario y Matriz de Permisos (RBAC)

| Rol | Descripción | Permisos Principales |
| :--- | :--- | :--- |
| **Usuario** | Usuario final de la aplicación móvil. | Crear/Editar/Eliminar sus propias transacciones, metas, deudas y suscripciones. Configurar sus cuentas multimoneda y visualizar sus reportes. |
| **Administrador** | Gestor global de la plataforma Monetra. | Administrar usuarios registrados, supervisar la salud de las APIs de terceros, gestionar parámetros globales del sistema. |
| **Soporte** | Personal técnico de atención al cliente. | Visualizar únicamente información técnica de errores reportados por usuarios (sin acceso a sus montos ni datos financieros sensibles). |
| **Auditor** | Perfil de cumplimiento y auditoría interna. | Acceso de solo lectura a logs de operaciones, trazabilidad de cambios del sistema y registros de seguridad. |

---

## 3. Funcionalidades Clave y Criterios de Aceptación

### F1: Gestión Multimoneda y Cuentas
- **Descripción:** Soporte para balances en múltiples divisas (ej. USD, COP) y efectivo.
- **Criterios de Aceptación:**
  - El usuario puede crear cuentas asociadas a una divisa específica.
  - El sistema calcula el patrimonio neto equivalente en la moneda base del usuario utilizando la tasa de cambio del día.
  - El usuario puede alternar la visualización del balance general entre su moneda local y USD.

### F2: Registro de Movimientos y Categorización con IA
- **Descripción:** Ingreso manual y clasificado de ingresos y egresos, asistido por IA para sugerir categorías.
- **Criterios de Aceptación:**
  - Cada transacción debe solicitar obligatoriamente: monto, tipo (ingreso/egreso), cuenta origen, fecha y categoría.
  - Al ingresar un texto descriptivo (ej. "Mercado en Éxito"), el modelo de IA debe proponer la categoría más probable (ej. "Alimentación").

### F3: Control de Deudas e Intereses
- **Descripción:** Módulo para la gestión y proyección de deudas personales.
- **Criterios de Aceptación:**
  - Debe requerir: capital inicial, tasa de interés (EA/EM), entidad acreedora y fecha de pago.
  - El sistema debe proyectar el plan de pagos (amortización) y restar automáticamente los abonos aplicados por el usuario.

### F4: Metas de Ahorro y Suscripciones Recurrentes
- **Descripción:** Mapeo de gastos fijos/suscripciones y progreso de ahorro acumulativo.
- **Criterios de Aceptación:**
  - Las suscripciones deben generar alertas automáticas previas a la fecha de cobro.
  - Las metas de ahorro deben reflejar visualmente un porcentaje de avance basado en los depósitos vinculados.

### F5: Conexión con Datos de Mercado (Divisas y Acciones)
- **Descripción:** Consulta informativa del mercado financiero global.
- **Criterios de Aceptación:**
  - Visualización del valor actualizado de las principales divisas y cotización de acciones seleccionadas.
  - La falla de la API externa de mercado no debe bloquear el uso offline o la carga de transacciones locales.

---

## 4. Restricciones, Límites y Exclusiones (El "No")
- **Exclusión de Ejecución de Operaciones Financieras:** Monetra **NO** realizará transferencias bancarias reales ni órdenes de compra/venta de acciones directamente en la plataforma.
- **Restricciones Técnicas:**
  - Todas las operaciones en la app móvil Android deben funcionar en modo offline mediante sincronización local previo a la persistencia remota.
  - Ninguna credencial bancaria ni llave de API debe ser almacenada en el cliente móvil.

---

## 5. Casos Límite y Validaciones Críticas
- **Redondeo y Centavos:** Todos los montos financieros deben almacenarse utilizando tipos de datos decimales de alta precisión (128 bits / `decimal` en C#) para evitar errores de redondeo en conversiones de divisas.
- **Falla de API de Mercado:** Si la API de Yahoo Finance / Alpha Vantage falla, el sistema utilizará la última tasa cambiaria guardada en caché local con una advertencia visual de fecha de actualización.

