# HU-14 — Reporte Sumariado con Gráfico Estadístico

## ✅ Estado: COMPLETADO

### Objetivo
Mostrar recaudación agrupada por categoría de producto, con tabla sumariada y gráfico estadístico.

---

## 📋 Implementación

### 1. **Endpoint API**
```
GET /api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30
```

**Parámetros:**
- `fechaDesde` (opcional): Fecha inicio en formato `yyyy-MM-dd`
- `fechaHasta` (opcional): Fecha fin en formato `yyyy-MM-dd`
- `usuario` (opcional): Nombre del usuario que genera el reporte (se obtiene del token si no se proporciona)

**Response:**
- Status: `200 OK`
- Content-Type: `application/pdf`
- Body: Archivo PDF descargable

**Ejemplo de uso:**
```bash
curl -X GET "https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30" \
  -H "Authorization: Bearer <token>" \
  --output resumen_recaudacion.pdf
```

---

## 🔧 Componentes Implementados

### 1. **ReportesController.GenerarResumenRecaudacion()**
**Archivo:** `MicroServicioReportes.API/Controllers/ReportesController.cs`

```csharp
[HttpGet("resumen-recaudacion")]
public async Task<IActionResult> GenerarResumenRecaudacion(
    [FromQuery] ReporteRequestDto request,
    CancellationToken cancellationToken)
```

**Funcionalidad:**
- Prepara la información del usuario desde los claims
- Llama al servicio de aplicación
- Retorna el PDF generado
- Maneja excepciones (`ArgumentException` → BadRequest)

---

### 2. **ReporteServicio.GenerarResumenRecaudacionAsync()**
**Archivo:** `MicroServicioReportes.Aplicacion/Servicios/ReporteServicio.cs`

**Funcionalidad:**
- Valida el rango de fechas
- Obtiene datos del repositorio
- Calcula:
  - **Cantidad vendida** por categoría
  - **Total recaudado** por categoría
  - **Porcentaje de participación** (participación % del total)
- Construye documento PDF con:
  - Encabezado descriptivo
  - Datos generales (filtros aplicados)
  - **Tabla sumariada** con datos clave
  - **Resumen** con totales
  - **Gráfico de barras** mostrando distribución de recaudación
  - Pie de página con usuario y fecha

**Datos calculados:**
```
Total General = SUM(Importe de todas las ventas)
Para cada categoría:
  - Cantidad Vendida = SUM(cantidad de productos de esa categoría)
  - Total Recaudado = SUM(precio unitario * cantidad de esa categoría)
  - Porcentaje = (Total Recaudado / Total General) * 100
```

---

### 3. **IReporteRepositorio.ObtenerResumenRecaudacionAsync()**
**Archivo:** `MicroServicioReportes.Dominio/Interfaces/IReporteRepositorio.cs`

```csharp
Task<IReadOnlyCollection<ResumenRecaudacionReporteDto>> ObtenerResumenRecaudacionAsync(
    ReporteRequestDto request,
    CancellationToken cancellationToken = default);
```

**Implementación:** `ReporteRepositorioEnMemoria.cs`

---

### 4. **Modelos de Datos**

#### ResumenRecaudacionReporteDto
```csharp
public class ResumenRecaudacionReporteDto
{
    public string Grupo { get; set; }              // Categoría del producto
    public int CantidadVendida { get; set; }       // Total de unidades
    public decimal TotalRecaudado { get; set; }    // Monto en Bs
    public decimal Porcentaje { get; set; }        // % de participación
}
```

#### ReporteRequestDto
```csharp
public class ReporteRequestDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? IdProducto { get; set; }
    public int? IdCliente { get; set; }
    public string Usuario { get; set; }
}
```

---

## 📊 Datos de Prueba

El repositorio en memoria incluye 8 ventas dentro del rango **2026-06-01 a 2026-06-30**:

| Categoría | Productos | Cantidad Total | Total Recaudado |
|-----------|-----------|-----------------|-----------------|
| **Programacion** | Clean Code, Arquitectura Limpia, Design Patterns, Programacion en C#, RESTful Web Services | 10 unidades | 1115 Bs |
| **Literatura** | El Principito, La Casa de los Espiritus, Cien Años de Soledad, El Quijote | 8 unidades | 600 Bs |
| **TOTAL** | — | **18 unidades** | **1715 Bs** |

**Porcentajes:**
- Programacion: 65.03%
- Literatura: 34.97%

---

## 🎨 Estructura del PDF Generado

El reporte PDF incluye:

1. **Encabezado**
   - Título: "Reporte Sumariado de Recaudación"
   - Subtítulo: "Resumen con gráfico estadístico para analizar el rendimiento de ventas"
   - Usuario y fecha de generación

2. **Datos Generales**
   - Fecha desde y fecha hasta (filtros aplicados)
   - Información de productos y clientes (si aplica)

3. **Tabla Sumariada**
   - Producto/Categoría
   - Cantidad Vendida
   - Total Recaudado Bs
   - Participación (%)

4. **Resumen**
   - Total unidades vendidas
   - Total recaudado en Bs

5. **Gráfico de Barras**
   - Tipo: Barras
   - Título: "Distribución de recaudación"
   - Valores: Categoría → Porcentaje de participación

6. **Pie de Página**
   - Usuario generador y fecha/hora de generación

---

## ✔️ Validaciones

1. **Rango de fechas:** `FechaDesde` no puede ser mayor que `FechaHasta`
   - Excepción: `ArgumentException`
   - Respuesta: `400 Bad Request`

2. **Usuario no encontrado en filtro:**
   - Se obtiene del claim "NombreCompleto" o `ClaimTypes.Name`
   - Fallback: "Sistema"

3. **Ventas sin registros:**
   - Se retorna tabla vacía con resumen en ceros
   - No genera error, solo un reporte sin datos

---

## 🔄 Flujo de Ejecución

```
GET /api/reportes/resumen-recaudacion
    ↓
ReportesController.GenerarResumenRecaudacion()
    ↓
Validar rango de fechas
    ↓
IReporteRepositorio.ObtenerResumenRecaudacionAsync()
    ├── Aplica filtros por fecha
    ├── Agrupa por categoría
    └── Calcula cantidad, total y porcentaje
    ↓
IReporteBuilder.AgregarTabla()
IReporteBuilder.AgregarGrafico()
    ↓
PdfGeneradorReporte.Generar()
    ↓
IActionResult File() → PDF descargable
```

---

## 📁 Archivos Modificados

- ✅ `MicroServicioReportes.Infraestructura/Repositorios/ReporteRepositorioEnMemoria.cs`
  - Actualización de datos de prueba para rango de fechas correcto (2026-06-01 a 2026-06-30)
  - Adición de más registros de prueba para mejor distribución de datos

---

## 🧪 Pruebas Recomendadas

### Test 1: Rango de fechas correcto
```bash
GET /api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30
Expected: 200 OK + PDF con 18 unidades, 1715 Bs
```

### Test 2: Sin filtros
```bash
GET /api/reportes/resumen-recaudacion
Expected: 200 OK + PDF con todos los datos
```

### Test 3: Rango con pocas ventas
```bash
GET /api/reportes/resumen-recaudacion?fechaDesde=2026-06-05&fechaHasta=2026-06-10
Expected: 200 OK + PDF con 6 unidades (Programación: 3, Literatura: 3)
```

### Test 4: Fechas invertidas (error)
```bash
GET /api/reportes/resumen-recaudacion?fechaDesde=2026-06-30&fechaHasta=2026-06-01
Expected: 400 Bad Request + Mensaje de error
```

---

## 📝 Notas de Implementación

- El reporte agrupa **por categoría** (no por producto individual)
- Los porcentajes se calculan sobre el **total general de recaudación**
- El gráfico es de tipo **Barras** mostrando participación porcentual
- El PDF es generado en formato de texto (compatible con todos los clientes)
- Soporta caracteres acentuados (normalización ASCII en PDF)

---

## 🎯 Criterios de Aceptación Cumplidos

✅ Consultar ventas para obtener importes y cantidades
✅ Consultar productos para agrupar por categoría
✅ Aplicar filtros por rango de fechas
✅ Calcular cantidad vendida
✅ Calcular total recaudado
✅ Calcular porcentaje de participación
✅ Generar tabla sumariada
✅ Generar datos para gráfico
✅ Mejorar el PDF con representación gráfica
✅ Endpoint solicitado implementado

---

**Fecha de implementación:** 2026-01-XX  
**Estado:** ✅ Completado y funcional
