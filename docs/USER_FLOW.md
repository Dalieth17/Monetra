# User Flow & Navigation Document - Monetra

## 1. Punto de Entrada y Autenticación
```
[Inicio de la App]
       |
       v
¿Sesión Activa?
  ├── No  ---> [Landing / Login Screen] ---> [Registro] / [Ingreso JWT]
  └── Sí  ---> [Dashboard Principal (Monetra Home)]
```

---

## 2. Camino Crítico Paso a Paso (Happy Path)
1. **Acceso:** El usuario ingresa a Monetra y ve su **Dashboard Principal**.
2. **Visualización de Balances:** Observa su dinero total distribuido por cuentas (Ej: Efectivo COP, Cuenta Banco USD) con la conversión calculada a su moneda principal.
3. **Registro de Movimiento:**
   - Presiona el botón flotante `+` (Añadir Transacción).
   - Selecciona el tipo: *Ingreso* o *Egreso*.
   - Ingresa el monto (ej. `$50.00`) y la descripción (ej. `"Compra Supermercado"`).
   - La IA sugiere automáticamente la categoría `"Alimentación"`. El usuario confirma.
   - Selecciona la cuenta origen y guarda.
4. **Actualización:** El Dashboard se actualiza instantáneamente reflejando el nuevo balance y actualizando las métricas del mes.

---

## 3. Edge Cases y Escenarios Alternativos
- **Sin Conexión a Internet:** La transacción se guarda en la base de datos local **Room** de Android con el flag `isSynced = false`. La app notifica discretamente: *"Guardado offline. Se sincronizará al reconectar"*.
- **Falla en API de Divisas:** Al consultar la pestaña de Mercado, la app muestra un badge informativo: *"Tasa de cambio desactualizada (último valor registrado: hace 2 horas)"*.
- **Monto Inválido:** Si el usuario ingresa un valor negativo o no numérico, el formulario desactiva el botón de envío y muestra un mensaje de validación estricto de UI.

---

## 4. Estados de la Interfaz (UI States)
- **Loading:** Esqueletos de carga (Shimmer effect) en el Dashboard mientras se leen los datos locales/remotos.
- **Empty State:** Si el usuario es nuevo, se muestra una ilustración amigable con la leyenda: *"No tienes movimientos registrados este mes. Toca el botón + para empezar"*.
- **Error State:** Banner rojo superior con botón de "Reintentar" si la sincronización con el servidor de Azure en C# falla por tiempo de espera.

---

## 5. Puntos de Salida y Cierre de Bucle
- **Confirmación de Acción:** Al completar una meta de ahorro o registrar un pago de deuda, se despliega una tarjeta de felicitación/éxito con la opción rápida de *"Ver Progreso"* o *"Volver al Inicio"*.

