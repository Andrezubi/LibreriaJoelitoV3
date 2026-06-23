# HU-14: Guía de Pruebas y Ejemplos de Uso

## 1. Pruebas Manuales en Postman/Insomnia

### Prueba 1: Reporte Completo (Rango de fechas correcto)

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30
```

**Headers:**
```
Authorization: Bearer <tu-jwt-token>
Content-Type: application/json
```

**Expected Response:**
- Status: `200 OK`
- Content-Type: `application/pdf`
- File: `ResumenRecaudacion_202601010000.pdf` (nombre generado dinámicamente)

**Contenido del PDF:**
```
LIBRERIA JOELITO
Reporte Sumariado de Recaudacion
Resumen con grafico estadistico para analizar el rendimiento de ventas
Estado: Generado
Fecha generacion: 01/01/2026 12:00
Usuario: Juan Perez (obtenido del token o del parámetro)

DATOS GENERALES
Fecha desde: 01/06/2026
Fecha hasta: 30/06/2026
Producto: Todos
Cliente: Todos

RESUMEN DE RECAUDACION
Producto/Categoria | Cantidad Vendida | Total Recaudado Bs | Participacion
Programacion       | 10               | 1115.00            | 65.03%
Literatura         | 8                | 600.00             | 34.97%

RESUMEN
Total unidades vendidas: 18
Total recaudado Bs: 1715.00

DISTRIBUCION DE RECAUDACION (Barras)
Programacion: 65.03%
Literatura: 34.97%

---
Generado por Usuario: Juan Perez
Fecha: 01/01/2026 12:00
```

---

### Prueba 2: Rango de Fechas Parcial (Menos datos)

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-05&fechaHasta=2026-06-15
```

**Ventas incluidas:**
- 2026-06-05: Clean Code (2), Arquitectura Limpia (1) - Programacion
- 2026-06-10: El Principito (3) - Literatura
- 2026-06-15: La Casa de los Espiritus (2) - Literatura

**Totales esperados:**
- Programacion: 3 unidades, 310 Bs (65.97%)
- Literatura: 5 unidades, 135 Bs (34.03%)
- Total: 8 unidades, 445 Bs

---

### Prueba 3: Sin Parámetros de Filtro

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion
```

**Resultado:**
- Incluye todas las ventas disponibles en el repositorio en memoria
- Los filtros mostrarán "Sin filtro" para fechas

---

### Prueba 4: Con Usuario Personalizado

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-01&fechaHasta=2026-06-30&usuario=Maria%20Lopez
```

**Nota:** El usuario se obtiene del parámetro query si se proporciona; si no, se extrae del token de autenticación.

---

### Prueba 5: Error - Fechas Invertidas

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-06-30&fechaHasta=2026-06-01
```

**Expected Response:**
- Status: `400 Bad Request`
- Body:
```json
{
  "error": "La fecha desde no puede ser mayor que la fecha hasta."
}
```

---

### Prueba 6: Error - Rango de Fechas Fuera del Período

**URL:**
```
GET https://localhost:5001/api/reportes/resumen-recaudacion?fechaDesde=2026-07-01&fechaHasta=2026-07-31
```

**Expected Response:**
- Status: `200 OK`
- El PDF contendrá una tabla vacía con resumen en ceros

---

## 2. Pruebas Unitarias (C#)

```csharp
[TestFixture]
public class ReporteServicioTests
{
    private IReporteServicio _servicio;
    private IReporteRepositorio _repositorio;
    private IReporteBuilder _builder;
    private IPlantillaReporteProveedor _plantillas;
    private IGeneradorReporte _generador;

    [SetUp]
    public void Setup()
    {
        _repositorio = new ReporteRepositorioEnMemoria();
        _builder = new ReporteBuilder();
        _plantillas = new PlantillaReporteProveedor();
        _generador = new PdfGeneradorReporte();
        _servicio = new ReporteServicio(_repositorio, _builder, _plantillas, _generador);
    }

    [Test]
    public async Task GenerarResumenRecaudacion_ConRangoValido_RetornaReportePdf()
    {
        // Arrange
        var request = new ReporteRequestDto
        {
            FechaDesde = new DateTime(2026, 6, 1),
            FechaHasta = new DateTime(2026, 6, 30),
            Usuario = "Test User"
        };

        // Act
        var resultado = await _servicio.GenerarResumenRecaudacionAsync(request);

        // Assert
        Assert.IsNotNull(resultado);
        Assert.IsNotEmpty(resultado.Archivo);
        Assert.AreEqual("application/pdf", resultado.ContentType);
        Assert.IsTrue(resultado.NombreArchivo.StartsWith("ResumenRecaudacion_"));
        Assert.IsTrue(resultado.NombreArchivo.EndsWith(".pdf"));
    }

    [Test]
    public async Task GenerarResumenRecaudacion_ConFechasInvertidas_LanzaArgumentException()
    {
        // Arrange
        var request = new ReporteRequestDto
        {
            FechaDesde = new DateTime(2026, 6, 30),
            FechaHasta = new DateTime(2026, 6, 1)
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _servicio.GenerarResumenRecaudacionAsync(request));
    }

    [Test]
    public async Task GenerarResumenRecaudacion_SinFiltros_RetornaTodasLasVentas()
    {
        // Arrange
        var request = new ReporteRequestDto
        {
            Usuario = "Test User"
        };

        // Act
        var resultado = await _servicio.GenerarResumenRecaudacionAsync(request);

        // Assert
        Assert.IsNotNull(resultado);
        Assert.IsNotEmpty(resultado.Archivo);
    }

    [Test]
    public async Task ObtenerResumenRecaudacion_AgrupaPorCategoria()
    {
        // Arrange
        var request = new ReporteRequestDto
        {
            FechaDesde = new DateTime(2026, 6, 1),
            FechaHasta = new DateTime(2026, 6, 30)
        };

        // Act
        var resumen = await _repositorio.ObtenerResumenRecaudacionAsync(request);

        // Assert
        Assert.AreEqual(2, resumen.Count); // Programacion y Literatura
        
        var programacion = resumen.FirstOrDefault(r => r.Grupo == "Programacion");
        Assert.IsNotNull(programacion);
        Assert.AreEqual(10, programacion.CantidadVendida);
        Assert.AreEqual(1115m, programacion.TotalRecaudado);
        Assert.That(programacion.Porcentaje, Is.GreaterThan(64).And.LessThan(66));

        var literatura = resumen.FirstOrDefault(r => r.Grupo == "Literatura");
        Assert.IsNotNull(literatura);
        Assert.AreEqual(8, literatura.CantidadVendida);
        Assert.AreEqual(600m, literatura.TotalRecaudado);
        Assert.That(literatura.Porcentaje, Is.GreaterThan(34).And.LessThan(36));
    }
}
```

---

## 3. Pruebas de Integración (C#)

```csharp
[TestFixture]
public class ReportesControllerTests
{
    private HttpClient _client;
    private TestServer _server;
    private string _authToken;

    [SetUp]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<Startup>();
        
        _server = new TestServer(builder);
        _client = _server.CreateClient();
        _authToken = ObtenerTokenAuditoria();
    }

    [Test]
    public async Task GenerarResumenRecaudacion_ConTokenValido_Retorna200()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        var request = "/api/reportes/resumen-recaudacion" +
                     "?fechaDesde=2026-06-01&fechaHasta=2026-06-30";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/pdf", response.Content.Headers.ContentType.MediaType);

        var content = await response.Content.ReadAsAsync<byte[]>();
        Assert.Greater(content.Length, 0);
    }

    [Test]
    public async Task GenerarResumenRecaudacion_ConFechasInvertidas_Retorna400()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _authToken);

        var request = "/api/reportes/resumen-recaudacion" +
                     "?fechaDesde=2026-06-30&fechaHasta=2026-06-01";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsAsync<dynamic>();
        Assert.IsNotNull(content.error);
        Assert.That(content.error.ToString(), 
            Does.Contain("fecha desde no puede ser mayor"));
    }

    [Test]
    public async Task GenerarResumenRecaudacion_SinAutorizacion_Retorna401()
    {
        // Act
        var response = await _client.GetAsync("/api/reportes/resumen-recaudacion");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private string ObtenerTokenAuditoria()
    {
        // Implementar lógica para obtener token de prueba
        // Esto dependerá de tu configuración de autenticación
        return "token-de-prueba";
    }
}
```

---

## 4. Validación Manual del PDF

Una vez descargado el PDF, verificar:

1. **Encabezado:**
   - ✅ Título: "Reporte Sumariado de Recaudación"
   - ✅ Subtítulo menciona gráfico estadístico
   - ✅ Usuario y fecha de generación presentes

2. **Datos Generales:**
   - ✅ Fechas desde y hasta correctas
   - ✅ Filtros mostrados apropiadamente

3. **Tabla:**
   - ✅ Columnas correctas (Producto/Categoría, Cantidad, Total, Participación)
   - ✅ Datos agrupados por categoría (no por producto individual)
   - ✅ Ordenado por total recaudado descendente

4. **Resumen:**
   - ✅ Total unidades: 18
   - ✅ Total recaudado: 1715.00 Bs

5. **Gráfico:**
   - ✅ Categorías en el eje X
   - ✅ Porcentajes en el eje Y o como etiquetas
   - ✅ Tipo: Barras

6. **Pie de Página:**
   - ✅ Usuario generador
   - ✅ Fecha y hora de generación

---

## 5. Datos de Referencia para Pruebas

### Catálogo de Categorías
- Programacion
- Literatura

### Ventas de Prueba (Rango 2026-06-01 a 2026-06-30)

| Nro | Fecha | Producto | Categoría | Cantidad | Precio | Importe |
|-----|-------|----------|-----------|----------|--------|---------|
| 1 | 2026-06-05 | Clean Code | Programacion | 2 | 95 | 190 |
| 1 | 2026-06-05 | Arquitectura Limpia | Programacion | 1 | 120 | 120 |
| 2 | 2026-06-10 | El Principito | Literatura | 3 | 45 | 135 |
| 3 | 2026-06-15 | La Casa de los Espiritus | Literatura | 2 | 55 | 110 |
| 4 | 2026-06-20 | Design Patterns | Programacion | 1 | 85 | 85 |
| 5 | 2026-06-22 | Programacion en C# | Programacion | 2 | 75 | 150 |
| 6 | 2026-06-25 | Cien Años de Soledad | Literatura | 1 | 50 | 50 |
| 7 | 2026-06-28 | RESTful Web Services | Programacion | 3 | 90 | 270 |
| 8 | 2026-06-30 | El Quijote | Literatura | 2 | 65 | 130 |

**Totales por Categoría:**
- **Programacion:** 10 unidades, 1115 Bs (65.03%)
- **Literatura:** 8 unidades, 600 Bs (34.97%)
- **TOTAL:** 18 unidades, 1715 Bs

---

## 6. Debugging y Troubleshooting

### Problema: El PDF no se descarga

**Solución:**
1. Verificar que el token de autenticación sea válido
2. Verificar logs de servidor: `[1bd8a850-02d1-11d1-bee7-00a0c913d1f8]`
3. Verificar que el formato de fecha sea `yyyy-MM-dd`

### Problema: Las fechas no filtran correctamente

**Solución:**
1. Asegurar que las fechas estén en formato ISO: `2026-06-01`
2. Verificar que `FechaDesde <= FechaHasta`
3. Revisar la zona horaria del servidor

### Problema: El gráfico no aparece

**Solución:**
1. El generador de PDF actual es de texto, no incluye imágenes de gráficos
2. Para gráficos visuales, considerar usar librerías como iText Sharp o Report Viewer
3. Los datos del gráfico (valores) se incluyen como texto en el PDF

---

## 7. Checklist de Validación

- [ ] Endpoint GET `/api/reportes/resumen-recaudacion` accesible
- [ ] Requiere autenticación (token JWT)
- [ ] Parámetros `fechaDesde` y `fechaHasta` funcionan correctamente
- [ ] Validación de rango de fechas activa
- [ ] PDF se genera sin errores
- [ ] PDF contiene tabla con datos agrupados por categoría
- [ ] Porcentajes se calculan correctamente
- [ ] Gráfico de barras incluido en PDF
- [ ] Datos de prueba cargan correctamente (18 unidades totales)
- [ ] Documentación actualizada en repositorio

---

**Última actualización:** 2026-01-XX  
**Versión:** 1.0  
**Estado:** ✅ Listo para QA
